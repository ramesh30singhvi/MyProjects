using Azure.Storage.Blobs;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.PDF;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using SHARP.DAL;
using System.Diagnostics;
using SHARP.BusinessLogic.ReportAI;
using SHARP.BusinessLogic.ReportAI.Models;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http;
using ScottPlot.Drawing.Colormaps;
using SHARP.Common.Enums;
using SHARP.DAL.Models;
using SHARP.Common.Filtration;
using SHARP.BusinessLogic.Helpers;
using SHARP.Common.Filtration.Enums;
using System.Linq.Expressions;
using System.IO.Compression;
using SHARP.BusinessLogic.Extensions;

namespace SHARP.BusinessLogic.Services
{
    public class ReportAIService : IReportAIService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IOpenAIService _openAiService;

        public ReportAIService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IOpenAIService openAiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _openAiService = openAiService;
        }
        private async Task<string> GetUploadKeywords(IFormFile keywordsJson)
        {

            string fileContents = string.Empty;
            using (var stream = keywordsJson.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                fileContents = await reader.ReadToEndAsync();
            }

            return fileContents;
        }

        private async Task<Dictionary<string, List<Dictionary<string, object>>>> RunOpenAIProcess(string buidlWordIndex)
        {
            var results = new Dictionary<string, List<Dictionary<string, object>>>();
            try
            {

                var serializer = new JsonSerializer();


                using (var jsonTextReader = new JsonTextReader(new StringReader(buidlWordIndex)))
                {
                    var buildIndexWord = serializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(jsonTextReader);

                    if (buildIndexWord != null)
                    {
                        if (buildIndexWord is Dictionary<string, List<Dictionary<string, string>>> b)
                            results = await _openAiService.Search(b);
                    }
                }


            }
            catch (Exception ex)
            {

            }
            return results;
        }

        public async Task<AzureReportProcessResultDto> SendToAIAzureFunction(PdfReportUploadAzureDto uploadPdfAzureDto, Dictionary<string, string> telemetryProperties = null)
        {
            AzureReportProcessResultDto result = new AzureReportProcessResultDto();

            //Dictionary<string, List<Dictionary<string, object>>>  results = await RunOpenAIProcess( uploadPdfAzureDto.BuidlWordIndex);

            //if(results != null)
            //{
            //    result.JsonResult = WriteToJson(results).ToString();
            //    result.Error = "";
            //    result.SearchWord = uploadPdfAzureDto.Keyword;
            //    result.ReportFileName = "";
            //    result.ContainerName = "";
            //    return result;
            //}

            var reportContainer = _configuration["ReportContainer"] ?? "pdf-pcc-report";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            telemetryProperties?.Add("Start SendToAIAzureFunction -> GetUploadKeywords", $"Time: {DateTime.UtcNow}.");

            result.Keywords = await GetUploadKeywords(uploadPdfAzureDto.KeywordFileJson);

            stopWatch.Stop();
            telemetryProperties?.Add("End SendToAIAzureFunction -> GetUploadKeywords", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");
            stopWatch.Restart();
            telemetryProperties?.Add("Start SendToAIAzureFunction -> UploadBuildIndexWordsToAzureAsync", $"Time: {DateTime.UtcNow}.");

            var buildWordIndexFileName = await UploadBuildIndexWordsToAzureAsync(uploadPdfAzureDto.PdfFile.FileName, uploadPdfAzureDto.BuidlWordIndex, uploadPdfAzureDto.Keyword);

            stopWatch.Stop();
            telemetryProperties?.Add("End SendToAIAzureFunction -> UploadBuildIndexWordsToAzureAsync", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");
            stopWatch.Restart();
            telemetryProperties?.Add("Start SendToAIAzureFunction -> RunAzureFunctionProcess", $"Time: {DateTime.UtcNow}.");

            var resultFromAzureFunction = await RunAzureFunctionProcess(reportContainer, buildWordIndexFileName);

            stopWatch.Stop();
            telemetryProperties?.Add("End SendToAIAzureFunction -> RunAzureFunctionProcess", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");

            if (!string.IsNullOrEmpty(resultFromAzureFunction.JsonResult))
            {

                result.JsonResult = resultFromAzureFunction.JsonResult; //WriteToJson(r).ToString();
                result.Error = "";
                result.SearchWord = uploadPdfAzureDto.Keyword;
                result.ReportFileName = "";
                result.ContainerName = "";
            }


            return result;
        }

        private async Task DeleteBuildIndexFileFromAzure(string buildWordIndexFileName)
        {
            var stoageURL = _configuration["ReportStorage"] ?? string.Empty;

            BlobServiceClient blobServiceClient = new BlobServiceClient(stoageURL);

            var reportContainer = _configuration["ReportContainer"] ?? "pdf-pcc-report";

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(reportContainer);

            await containerClient.GetBlobClient(buildWordIndexFileName).DeleteIfExistsAsync();
        }

        private List<ReportPages> ExtractReportPages(IFormFile pdf, PdfReportParser pdfParser)
        {
            var reports = new List<ReportPages>();
            using (var ms = new MemoryStream())
            {
                pdf.CopyTo(ms);

                var fileBytes = ms.ToArray();
                // extract report from PDF uploaded file
                reports = pdfParser.ExtractReports(fileBytes).ToList();

            }
            return reports;
        }

        private string[] ExtractKeywords(IFormFile keywordFileJson, PdfReportAnalysisDto analysis)
        {
            using (var reader = new StreamReader(keywordFileJson.OpenReadStream()))
            {
                var fileContent = reader.ReadToEnd();
                try
                {
                    analysis.Keywords = fileContent;
                    return JsonConvert.DeserializeObject<string[]>(fileContent);
                }
                catch (Exception ex)
                {

                }

            }
            return null;
        }


        private List<PCCReport> ProcessAndRedactedReports(List<ReportPages> reports, PdfReportParser pdfParser, PdfReportAnalysisDto analysis)
        {
            var processedRedactedReports = new List<PCCReport>();

            foreach (var report in reports)
            {
                try
                {

                    PCCReport processReport = pdfParser.ProcessReport(report);

                    if (processReport != null)
                    {
                        processedRedactedReports.Add(processReport);

                        // need date and time for creating pdf report header after user is good with AI search
                        if (string.IsNullOrEmpty(analysis.Date))
                            if (!string.IsNullOrEmpty(processReport.Header))
                                analysis.Date = pdfParser.GetReportDate(processReport.Header);

                        if (string.IsNullOrEmpty(analysis.Time))
                            if (!string.IsNullOrEmpty(processReport.Header))
                                analysis.Time = pdfParser.GetReportTime(processReport.Header);

                    }

                }
                catch (Exception e)
                {

                }

            }
            return processedRedactedReports;
        }
        public async Task<PdfReportAnalysisDto> ParsePdfAndBuildIndex(PdfReportUploadAzureDto reportUploadAzureDto, Dictionary<string, string> telemetryProperties = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            telemetryProperties?.Add("Start ParsePdfAndBuildIndex", $"Time: {DateTime.UtcNow}.");

            var pdfParser = new PdfReportParser();

            PdfReportAnalysisDto analysis = new PdfReportAnalysisDto();

            try
            {
                var reports = ExtractReportPages(reportUploadAzureDto.PdfFile, pdfParser);

                if (!reports.Any())
                {
                    analysis.Error = "We have the problem to parse the pdf file on step of pagination.";
                    return analysis;
                }

                string[] keywords = ExtractKeywords(reportUploadAzureDto.KeywordFileJson, analysis);
                ;

                if (keywords == null)
                {
                    analysis.Error = $"We have the problem to parse the keyword file on step of parsing content.";
                    return analysis;
                }


                if (reports.Any())
                {

                    var processedAndRedactedReports = ProcessAndRedactedReports(reports, pdfParser, analysis);
                    foreach (var report in reports)
                        report.Dispose();

                    reports?.Clear();

                    if (processedAndRedactedReports.Any())
                    {

                        if (keywords != null && processedAndRedactedReports.Any())
                        {
                            var buildIndex = pdfParser.BuildWordIndex(processedAndRedactedReports, keywords.Distinct().ToList());

                            foreach (var item in processedAndRedactedReports)
                                item.Dispose();

                            processedAndRedactedReports.Clear();
                            processedAndRedactedReports = null;
                            keywords.ToList().Clear();
                            keywords = null;

                            if (buildIndex.Count > 0)
                            {
                                JObject jObject = WriteToJson(buildIndex);
                                analysis.BuildIndexJson = jObject.ToString();
                                analysis.NumberKeywordsFound = buildIndex.Count;

                                foreach (var list in buildIndex.Values)
                                {
                                    list.ForEach(item => item = null);
                                    list.Clear();
                                }
                                buildIndex.Clear();
                                buildIndex = null;
                            }
                            else
                            {
                                analysis.Error = $"We have the problem to build words index after parsing.";
                                return analysis;
                            }

                        }
                        else
                        {
                            analysis.Error = "We have the problem to parse the keyword file or make process report";
                            return analysis;
                        }

                    }

                }

            }
            catch (Exception ex)
            {

                stopWatch.Stop();
                telemetryProperties?.Add("Error ParsePdfAndBuildIndex", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}. Exception: {ex.ToString()}.");
                analysis.Error = ex.Message;
            }


            pdfParser.Clear();
            pdfParser = null;
            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
                telemetryProperties?.Add("End ParsePdfAndBuildIndex", $"Time: {DateTime.UtcNow}. Duration: {stopWatch.Elapsed}.");
            }

            return analysis;
        }

        private JObject WriteToJson(Dictionary<string, List<FormattedReport>> buildIndex)
        {
            JObject json = new JObject();

            foreach (var key in buildIndex.Keys)
            {
                var jArray = new JArray();

                foreach (var item in buildIndex[key])
                {

                    var newJObject = item.ToJObject();

                    jArray.Add(newJObject);

                }

                if (jArray.Count > 0)
                    json.Add(key, jArray);

            }

            return json;

        }

        private async Task<string> UploadBuildIndexWordsToAzureAsync(string fileNamePdf, string json, string keyword)
        {

            var stoageURL = _configuration["ReportStorage"] ?? string.Empty;

            BlobServiceClient blobServiceClient = new BlobServiceClient(stoageURL);

            var reportContainer = _configuration["ReportContainer"] ?? "pdf-pcc-report";

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(reportContainer);

            string name = System.IO.Path.GetFileNameWithoutExtension(fileNamePdf);

            name = $"{name}-{keyword}-{DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss.fff")}.json";

            BlobClient blobClient = containerClient.GetBlobClient(name);

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var resp = await blobClient.UploadAsync(ms, true);

                if (resp.GetRawResponse().Status != 201)
                {
                    return string.Empty;
                }
            }

            return name;

        }

        private async Task<(string blobNameReport, string blobKewordName, string containerName, string error)> UploadFilesToStorage(IFormFile pdfFile, IFormFile keywordFileJson)
        {
            var stoageURL = _configuration["ReportStorage"] ?? string.Empty;


            BlobServiceClient blobServiceClient = new BlobServiceClient(stoageURL);

            var reportContainer = _configuration["ReportContainer"] ?? "pdf-pcc-report";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(reportContainer);

            string name = System.IO.Path.GetFileNameWithoutExtension(pdfFile.FileName);
            name = $"{name}-{DateTime.Now.ToString("MM-dd-yyyy")}.pdf";
            BlobClient blobClient = containerClient.GetBlobClient(name);

            var resp = await blobClient.UploadAsync(pdfFile.OpenReadStream(), true);
            if (resp.GetRawResponse().Status != 201)
            {
                return ("", "", "", resp.GetRawResponse().IsError ? resp.GetRawResponse().ReasonPhrase : "Can not upload pdf");
            }
            blobClient = containerClient.GetBlobClient(keywordFileJson.FileName);
            resp = await blobClient.UploadAsync(keywordFileJson.OpenReadStream(), true);
            if (resp.GetRawResponse().Status != 201)
            {
                return ("", "", "", resp.GetRawResponse().IsError ? resp.GetRawResponse().ReasonPhrase : "Can not upload keyword");
            }

            return (name, keywordFileJson.FileName, reportContainer, "");
        }

        private async Task<AzureReportProcessResultDto> RunAzureFunctionProcess(string containerName, string buildIndexWordFileName)
        {
            AzureReportProcessResultDto result = new AzureReportProcessResultDto();
            var azureFunction = _configuration["AzureFunctionHttpTrigger"] ?? string.Empty;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(azureFunction);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 1200000;

            Dictionary<string, string> points = new Dictionary<string, string>
            {
                { "container",containerName },
            };

            if (!string.IsNullOrEmpty(buildIndexWordFileName))
            {
                points.Add("buildindex", buildIndexWordFileName);
            }
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = JsonConvert.SerializeObject(points, Formatting.Indented);
                streamWriter.Write(json);
            }


            try
            {

                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string strStream = streamReader.ReadToEnd();
                            try
                            {
                                if (!string.IsNullOrEmpty(strStream))
                                {
                                    JObject jObject = JObject.Parse(strStream);
                                    result.JsonResult = (string)jObject["result"];
                                    result.Error = (string)jObject["error"];
                                    result.Date = (DateTime)jObject["date"];
                                    result.Time = (string)jObject["time"];
                                }
                                else
                                {
                                    result.Error = "No result";
                                }
                            }
                            catch (Exception e)
                            {
                                result.Error = e.Message;
                            }

                        }
                        await DeleteBuildIndexFileFromAzure(buildIndexWordFileName);
                    }
                    else
                    {
                        result.Error = response.StatusCode.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.InnerException?.Message;
            }


            return result;
        }

        public async Task<int> ParsePdfByPatients(CreateAIReportDto createAIReportDto)
        {
            var pdfParser = new PdfReportParser();
            try
            {
                var reports = ExtractReportPages(createAIReportDto.PdfFile, pdfParser);
                if (reports.Any())
                    return await ProcessAndRedactedReportsV2(reports, pdfParser, createAIReportDto);
            }
            catch (Exception ex)
            {
                var err = ex.Message;
            }
            return 0;
        }
        private string DeCompress(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public async Task<Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>> SendToAnthropicAIService(PCCNotesDto notesDto)
        {

            try
            {
                var patientNotes = await _unitOfWork.AuditAIPatientPdfNotesRepository.GetPatientNotes(notesDto.PatientNotesId);

                if (patientNotes == null)
                    return new Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>(new List<AuditAIKeywordSummaryDto>().ToArray(), $"Patient notes does not find");


                notesDto.Date = patientNotes.Audit.AuditDate;
                notesDto.DateTimeNotes = patientNotes.DateTime;
                notesDto.Time = patientNotes.Audit.AuditTime;
                notesDto.FacilityName = patientNotes.Audit.Facility.Name;
                notesDto.FacilityId = patientNotes.Audit.Facility.Id;
                notesDto.PatientName = patientNotes.PatientName;
                notesDto.PatientId = patientNotes.PatientId;
                notesDto.PatientNotes = DeCompress(patientNotes.PdfNotes);
                notesDto.ReportId = patientNotes.Id;

                using (var client = new HttpClient())
                {
                    string url = _configuration["AnthropicAIUrl"];
                    string token = _configuration["AuthrizationBearAI"];
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);


                    var jsonData = notesDto.ToJsonString();
                    // var jsonData = System.Text.Json.JsonSerializer.Serialize(notesDto);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = content;
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await client.SendAsync(request);
                    }
                    catch (Exception ex)
                    {

                    }
                    string responseString = await response?.Content?.ReadAsStringAsync();
                    if (response?.StatusCode == HttpStatusCode.OK)
                    {
                        var jObject = JObject.Parse(responseString);
                        if (jObject != null)
                        {
                            if (jObject.ContainsKey("status"))
                            {
                                if ((string)jObject["status"] == "200")
                                {
                                    if (jObject.ContainsKey("results"))
                                    {
                                        JArray res = jObject["results"].ToObject<JArray>();
                                        if (res.Any())
                                        {
                                            var respondList = await CreateAIProgressNotesDto(res, notesDto.PatientNotesId);
                                            return new Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>(respondList, "");
                                        }
                                        return new Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>(new List<AuditAIKeywordSummaryDto>().ToArray(), "The result is empty");


                                    }
                                }
                                return new Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>(new List<AuditAIKeywordSummaryDto>().ToArray(), $"the service returend status {(string)jObject["status"]}");

                            }
                        }
                        //  return CreateAIProgressNotesDto(responseString);
                    }


                }

            }
            catch (Exception ex)
            {

            }

            return new Tuple<IReadOnlyCollection<AuditAIKeywordSummaryDto>, string>(new List<AuditAIKeywordSummaryDto>().ToArray(), "The result is empty");

        }

        private async Task<IReadOnlyCollection<AuditAIKeywordSummaryDto>> CreateAIProgressNotesDto(JArray results, int patientNoteId)
        {
            var resultList = new List<AuditAIKeywordSummary>();
            foreach (var res in results)
            {
                var result = AIProgressNotesDto.FromJson(res.ToObject<JObject>());
                var summary = new AuditAIKeywordSummary();
                summary.AuditAIPatientPdfNotesID = patientNoteId;
                summary.Accept = false;
                summary.Summary = result.Summary;
                summary.Keyword = result.Keyword;

                resultList.Add(summary);

            }
            _unitOfWork.AuditAIKeywordSummaryRepository.AddRange(resultList);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<IReadOnlyCollection<AuditAIKeywordSummaryDto>>(resultList);
        }

        private async Task<int> ProcessAndRedactedReportsV2(List<ReportPages> reports, PdfReportParser pdfParser, CreateAIReportDto createReportAIDto)
        {
            var processedRedactedReports = new List<AuditAIPatientPdfNotes>();

            var facility = _unitOfWork.FacilityRepository.GetSingleOrDefault(x => x.Id == Int32.Parse(createReportAIDto.FacilityId));

            var auditAIV2Report = new AuditAIReportV2();
            if (facility != null)
            {
                auditAIV2Report.FacilityId = facility.Id;
            }
            else
            {
                auditAIV2Report.FacilityId = Int32.Parse(createReportAIDto.FacilityId);
            }

            auditAIV2Report.OrganizationId = Int32.Parse(createReportAIDto.OrganizationId);
            auditAIV2Report.AuditorName = createReportAIDto.User;
            auditAIV2Report.PdfFileName = createReportAIDto.PdfFile.FileName;
            auditAIV2Report.Status = ReportAIStatus.InProgress;
            auditAIV2Report.CreatedAt = DateTime.UtcNow;
            auditAIV2Report.State = AIAuditState.Active;

            foreach (var report in reports)
            {
                try
                {

                    var processReportInfo = pdfParser.ProcessReportV2(report);

                    if (processReportInfo?.Item1 != null)
                    {
                        //        processReportInfo.Item1.FacilityName = facility?.Name;
                        //        processReport.FacilityId = Int32.Parse(createReportAIDto.FacilityId);
                        if (auditAIV2Report.Id == 0)
                        {
                            auditAIV2Report.AuditDate = processReportInfo.Item2;
                            auditAIV2Report.AuditTime = processReportInfo.Item3.Replace("Time:", "") ?? "";
                            var dateTIme = DateTime.ParseExact(processReportInfo.Item1.DateTime, "MM/dd/yyyy HH:mm", null);

                            auditAIV2Report.FilteredDate = dateTIme.ToShortDateString();
                            var saved = _unitOfWork.AuditAIReportV2Repository.Add(auditAIV2Report);
                            await _unitOfWork.SaveChangesAsync();
                            auditAIV2Report.Id = saved.Id;
                        }



                        processReportInfo.Item1.AuditAIReportV2Id = auditAIV2Report.Id;
                        var patientKeywordValue = _mapper.Map<AuditAIPatientPdfNotes>(processReportInfo.Item1);
                        processedRedactedReports.Add(patientKeywordValue);

                    }

                }
                catch (Exception e)
                {

                }

            }
            if (auditAIV2Report.Id > 0)
            {
                try
                {
                    if (processedRedactedReports.Any())
                    {
                        // await _unitOfWork.AuditAIPatientKeywordSummaryRepository.AddPatientValaues(processedRedactedReports);
                        _unitOfWork.AuditAIPatientPdfNotesRepository.AddRange(processedRedactedReports);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }catch(Exception e)
                {
                    _unitOfWork.AuditAIReportV2Repository.Remove(auditAIV2Report);
                    await _unitOfWork.SaveChangesAsync();
                    return 0;
                }
            }
            return auditAIV2Report.Id;
        }

        public async Task<IReadOnlyCollection<AuditAIReportV2Dto>> GetAIAuditV2ListAsync(AuditAIReportFilter filter)
        {
            Expression<Func<AuditAIReportV2, object>> orderBySelector =
             OrderByHelper.GetOrderBySelector<AuditAIReportFilterColumn, Expression<Func<AuditAIReportV2, object>>>(filter.OrderBy, GetColumnOrderSelector);

            var reportAIContent = await _unitOfWork.AuditAIReportV2Repository.GetAuditsAsync(filter, orderBySelector);

            var reportAIContentDto = _mapper.Map<IReadOnlyCollection<AuditAIReportV2Dto>>(reportAIContent);

            return reportAIContentDto;
        }

        private Expression<Func<AuditAIReportV2, object>> GetColumnOrderSelector(AuditAIReportFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditAIReportFilterColumn, Expression<Func<AuditAIReportV2, object>>>
            {
                { AuditAIReportFilterColumn.OrganizationName, i => i.Facility.Organization.Name },
                { AuditAIReportFilterColumn.FacilityName, i => i.Facility.Name },
                { AuditAIReportFilterColumn.AuditorName, i => i.AuditorName },
                { AuditAIReportFilterColumn.AuditTime, i => i.AuditTime },
                { AuditAIReportFilterColumn.AuditDate, i => i.AuditDate },
                { AuditAIReportFilterColumn.FilteredDate, i => i.FilteredDate },
                { AuditAIReportFilterColumn.CreatedAt, i => i.CreatedAt },
                { AuditAIReportFilterColumn.Status, i => i.Status },
                { AuditAIReportFilterColumn.SubmittedDate, i => i.SubmittedDate },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        public async Task<AuditAIReportV2Dto> GetAIAuditAsync(int id)
        {
            var audit = await _unitOfWork.AuditAIReportV2Repository.GetAIAuditAsync(id);

            return _mapper.Map<AuditAIReportV2Dto>(audit);
        }
        public async Task<AuditAIReportV2Dto> UpdateAuditReportV2(AuditAIReportV2Dto auditDto)
        {
            var audit = await _unitOfWork.AuditAIReportV2Repository.GetAIAuditAsync(auditDto.Id);
            if(audit != null)
            {
                var listSummaries = new List<AuditAIKeywordSummaryDto>();
              
                foreach( var value in auditDto.Values.Where(x => x.Summaries.Any()))
                { 
                     listSummaries.AddRange(value.Summaries);
                }
                var summaries = _mapper.Map<IReadOnlyCollection<AuditAIKeywordSummary>>(listSummaries);
                _unitOfWork.AuditAIKeywordSummaryRepository.UpdateRange(summaries.AsEnumerable());
                await _unitOfWork.SaveChangesAsync();
            }

            return auditDto;
        }

        public async Task<AuditAIPatientPdfNotesDto> UpdatePatientInfoKeySummary(AuditAIPatientPdfNotesDto patientInfoDto)
        {
            var patientInfo = _mapper.Map<AuditAIPatientPdfNotes>(patientInfoDto);
            if (patientInfo == null)
                return null;

           
            _unitOfWork.AuditAIPatientPdfNotesRepository.Update(patientInfo);

            await _unitOfWork.SaveChangesAsync();

  

            return _mapper.Map<AuditAIPatientPdfNotesDto>(patientInfo);
        }
        public async Task<AuditAIPatientPdfNotesDto> AddPatientInfoKeySummary(AuditAIPatientPdfNotesDto patientInfoDto)
        {
            var patientInfo = _mapper.Map<AuditAIPatientPdfNotes>(patientInfoDto);
            if (patientInfo == null)
                return null;

            if (patientInfo.Id == 0)
                _unitOfWork.AuditAIPatientPdfNotesRepository.Add(patientInfo);

            await _unitOfWork.SaveChangesAsync();

            //foreach (var item in patientInfo.Summaries)
            //{
            //    item.AuditAIPatientPdfNotesID = patientInfo.Id;
            //}
            //_unitOfWork.AuditAIKeywordSummaryRepository.AddRange(patientInfo.Summaries);
            //await _unitOfWork.SaveChangesAsync();

           //var saved = await _unitOfWork.AuditAIPatientPdfNotesRepository.GetPatientNotes(patientInfo.Id);

            return _mapper.Map<AuditAIPatientPdfNotesDto>(patientInfo);
        }
        public async Task<AuditAIKeywordSummaryDto> AddAuditKeySummary(AuditAIKeywordSummaryDto keySummaryDto)
        {
            var keySummary = _mapper.Map<AuditAIKeywordSummary>(keySummaryDto);
            _unitOfWork.AuditAIKeywordSummaryRepository.Add(keySummary);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AuditAIKeywordSummaryDto>(keySummary);

        }
        public async Task<AuditAIKeywordSummaryDto> UpdateAuditKeySummary(AuditAIKeywordSummaryDto keySummaryDto)
        {
            var keySummary = _mapper.Map<AuditAIKeywordSummary>(keySummaryDto);
            _unitOfWork.AuditAIKeywordSummaryRepository.Update(keySummary);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AuditAIKeywordSummaryDto>(keySummary);

        }

        public async Task<ReportAIStatus> UpdateAIAuditV2StatusAsync(int aiAuditId, ReportAIStatus status)
        {
            List<string> listModifiedProps = new List<string>();

            var aiAudit = await _unitOfWork.AuditAIReportV2Repository.GetAIAuditAsync(aiAuditId);

            if (aiAudit is null)
            {
                throw new NotFoundException();
            }

            ReportAIStatus OldStatus = aiAudit.Status;

            aiAudit.Status = status;
            listModifiedProps.Add(nameof(aiAudit.Status));

            if (status == ReportAIStatus.Submitted)
            {
                aiAudit.SubmittedDate = DateTime.UtcNow;
                listModifiedProps.Add(nameof(aiAudit.SubmittedDate));
            }

            if (status == ReportAIStatus.WaitingForApproval && OldStatus != ReportAIStatus.WaitingForApproval)
            {
                aiAudit.SentForApprovalDate = DateTime.UtcNow;
                listModifiedProps.Add(nameof(aiAudit.SentForApprovalDate));
            }

            _unitOfWork.AuditAIReportV2Repository.Update(aiAudit, listModifiedProps.ToArray());
            await _unitOfWork.SaveChangesAsync();

            return aiAudit == null ? OldStatus : aiAudit.Status;
        }


        public async Task<bool> UpdateAIAuditV2State(int auditId, AIAuditState state)
        {
            List<string> listModifiedProps = new List<string>();

            var audit = await _unitOfWork.AuditAIReportV2Repository.GetAIAuditAsync(auditId);

            if (audit is null)
            {
                throw new NotFoundException();
            }

            audit.State = state;
            listModifiedProps.Add(nameof(audit.State));

            _unitOfWork.AuditAIReportV2Repository.Update(audit, listModifiedProps.ToArray());
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

    }

}