using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClosedXML.Excel;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.PDF;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using Table = iText.Layout.Element.Table;

namespace SHARP.BusinessLogic.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;
        public ReportService(IUnitOfWork unitOfWork,IAuditService auditservice, IUserService userService, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _auditService = auditservice;
            _userService = userService;
        }


        public async Task<FallReportDto> GetFallReport(ReportFallFilterModel filter)
        {
            List<Form> forms = await _unitOfWork.FormRepository.GetFallFormsByOrganizationAsync(filter.OrganizationID);
            List<int> formIds = forms.ConvertAll<int>(form => form.Id);
            List<Audit> audits = await _unitOfWork.AuditRepository.GetSubmitedAuditsAsyncForFallAndWound(formIds, filter);



            var auditIds = audits.Select(audit => audit.Id).ToArray();
            var formVersionsIds = audits.Select(audit => audit.FormVersionId).Distinct().ToArray();
            var fields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersionsIds);
            var fieldAnswers = await _unitOfWork.AuditFieldValueRepository.GetListAsync(
                answer => auditIds.Contains(answer.AuditId));


            foreach (var audit in audits)
            {
                audit.FormVersion.FormFields = fields.Where(field => field.FormVersionId == audit.FormVersionId).ToList();
                audit.AuditFieldValues = fieldAnswers.Where(answer => answer.AuditId == audit.Id).ToList();
            }

            FallReportDto fallReportDto = new FallReportDto();

            foreach (var month in filter.Months)
            {
                ByMonth byMonth = new ByMonth();
                DateTime date = new DateTime(filter.Year, month, 1);
                byMonth.Name = date.ToString("MMMM");
                byMonth.MajorInjury = FilterAuditsHelper.GetMajorInjuryCount(audits, filter.Year, month);
                byMonth.SentToHospital = FilterAuditsHelper.GetSentToHospitalCount(audits, filter.Year, month);
                byMonth.Total = FilterAuditsHelper.GetFilteredByMonth(audits, filter.Year, month).Count;

                byMonth.ByDay.Monday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Monday);
                byMonth.ByDay.Tuesday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Tuesday);
                byMonth.ByDay.Wednesday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Wednesday);
                byMonth.ByDay.Thursday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Thursday);
                byMonth.ByDay.Friday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Friday);
                byMonth.ByDay.Saturday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Saturday);
                byMonth.ByDay.Sunday = FilterAuditsHelper.GetByDayCount(audits, filter.Year, month, DayOfWeek.Sunday);
                fallReportDto.ByMonth.Add(byMonth);

            }

            fallReportDto.ByShift = FilterAuditsHelper.GetByShift(audits);

            fallReportDto.ByPlace = FilterAuditsHelper.GeyByPlace(audits);

            fallReportDto.ByActivity = FilterAuditsHelper.GetByActivity(audits);

            return fallReportDto;
        }

        public async Task<WoundReportDto> GetWoundReport(ReportFallFilterModel filter)
        {
            WoundReportDto woundReportDto = new WoundReportDto();

            List<Form> forms = await _unitOfWork.FormRepository.GetWoundFormsByOrganizationAsync(filter.OrganizationID);
            List<int> formIds = forms.ConvertAll<int>(form => form.Id);
            List<Audit> audits = await _unitOfWork.AuditRepository.GetSubmitedAuditsAsyncForFallAndWound(formIds, filter);

            var auditIds = audits.Select(audit => audit.Id).ToArray();
            var formVersionsIds = audits.Select(audit => audit.FormVersionId).Distinct().ToArray();
            var fields = await _unitOfWork.FormFieldRepository.GetFormFieldsAsync(formVersionsIds);
            var fieldAnswers = await _unitOfWork.AuditFieldValueRepository.GetListAsync(
                answer => auditIds.Contains(answer.AuditId));


            foreach (var audit in audits)
            {
                audit.FormVersion.FormFields = fields.Where(field => field.FormVersionId == audit.FormVersionId).ToList();
                audit.AuditFieldValues = fieldAnswers.Where(answer => answer.AuditId == audit.Id).ToList();
            }

            foreach (var month in filter.Months)
            {
                WoundReportDto.ByMonth byMonth = new WoundReportDto.ByMonth();
                DateTime date = new DateTime(filter.Year, month, 1);
                byMonth.Name = date.ToString("MMMM");
                byMonth.InHouseAcquired = FilterAuditsHelper.GetByInHouseAcquired(audits, filter.Year, month);
                byMonth.ReHospitalization = FilterAuditsHelper.GetByReHospitalization(audits, filter.Year, month);
                byMonth.byTypes = FilterAuditsHelper.GetByTypes(audits, filter.Year, month, fields);
                byMonth.Total = FilterAuditsHelper.GetFilteredByMonth(audits, filter.Year, month).Count;
                woundReportDto.byMonths.Add(byMonth);
            }


            return woundReportDto;
        }

        public async Task<byte[]> GetDownloadFallReport(ReportFallFilterModel filter)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            using Document document = new Document(pdfDocument, PageSize.A4, true);

            var organization = await _unitOfWork.OrganizationRepository.SingleAsync(filter.OrganizationID);
            var facility = await _unitOfWork.FacilityRepository.GetFacilityAsync(filter.FacilityID);

            List<string> months = new List<string>();

            foreach (var month in filter.Months)
            {
                DateTime date = new DateTime(filter.Year, month, 1);
                months.Add(date.ToString("MMMM yyyy"));
            }

            FallReportHeaderEventHandler header = new FallReportHeaderEventHandler(document, organization, facility, string.Join(", ", months));

            AddRepeatedElements(pdfDocument, header);
            SetMargins(document, header);

            var fallReport = await this.GetFallReport(filter);

            Table byMonth = PdfHelper.CreateByMonthData(fallReport);
            document.Add(byMonth);
            PdfHelper.AddByMonthDataBarChart(fallReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byDay = PdfHelper.CreateByDayData(fallReport);
            document.Add(byDay);
            PdfHelper.AddByWeekDayDataBarChart(fallReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byShift = PdfHelper.CreateByShiftData(fallReport);
            document.Add(byShift);
            PdfHelper.AddByShiftDayDataBarChart(fallReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byActivity = PdfHelper.CreateByActivityData(fallReport);
            document.Add(byActivity);
            PdfHelper.AddByActivityDayDataBarChart(fallReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byPlace = PdfHelper.CreateByPlaceData(fallReport);
            document.Add(byPlace);
            PdfHelper.AddByPlaceDataBarChart(fallReport, document, pdfDocument);

            document.Close();

            return stream.ToArray();
        }

        public async Task<byte[]> GetDownloadWoundReport(ReportFallFilterModel filter)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            using Document document = new Document(pdfDocument, PageSize.A4, true);

            var organization = await _unitOfWork.OrganizationRepository.SingleAsync(filter.OrganizationID);
            var facility = await _unitOfWork.FacilityRepository.GetFacilityAsync(filter.FacilityID);

            List<string> months = new List<string>();

            foreach (var month in filter.Months)
            {
                DateTime date = new DateTime(filter.Year, month, 1);
                months.Add(date.ToString("MMMM yyyy"));
            }

            WoundReportHeaderEventHandler header = new WoundReportHeaderEventHandler(document, organization, facility, string.Join(", ", months));

            AddRepeatedElements(pdfDocument, header);
            SetMargins(document, header);

            var woundReport = await this.GetWoundReport(filter);

            Table byMonth = PdfHelper.CreateByMonthData(woundReport);
            document.Add(byMonth);
            PdfHelper.AddByMonthDataBarChart(woundReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byUlcer = PdfHelper.CreateByUlcerData(woundReport);
            document.Add(byUlcer);
            PdfHelper.AddByUlcerDataBarChart(woundReport, document, pdfDocument);
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            Table byNotUlcer = PdfHelper.CreateByNotUlcerData(woundReport);
            document.Add(byNotUlcer);
            PdfHelper.AddByNotUlcerDataLineChart(woundReport, document, pdfDocument);


            document.Close();

            return stream.ToArray();
        }
        private void AddRepeatedElements(PdfDocument pdfDocument, ManualKeywordReportAIEventHandler header)
        {
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new AuditFooterEventHandler());
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());
        }
        private void AddRepeatedElements(PdfDocument pdfDocument, FallReportHeaderEventHandler header)
        {
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new AuditFooterEventHandler());
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());
        }

        private void AddRepeatedElements(PdfDocument pdfDocument, WoundReportHeaderEventHandler header)
        {
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new AuditFooterEventHandler());
            pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new WatermarkingEventHandler());
        }
        private void SetMargins(Document document, float height)
        {
            var sideMargin = 20;
            var topMargin = 20 + height;
            var bottomMargin = 50;
            document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);
        }
        private void SetMargins(Document document, FallReportHeaderEventHandler header)
        {
            var sideMargin = 20;
            var topMargin = 20 + header.Height;
            var bottomMargin = 50;
            document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);
        }

        private void SetMargins(Document document, WoundReportHeaderEventHandler header)
        {
            var sideMargin = 20;
            var topMargin = 20 + header.Height;
            var bottomMargin = 50;
            document.SetMargins(topMargin, sideMargin, bottomMargin, sideMargin);
        }

        public async Task<byte[]> GetDownloadCriteriaReport(ReportCriteriaFilter filter)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Criteria Report");

            var organization = await _unitOfWork.OrganizationRepository.SingleAsync(filter.OrganizationID);

            var currentLine = 1;
            ws.Cell(currentLine, 1).Value = "CRITERIA REPORT";
            ws.Cell(currentLine, 1).Style.Font.Bold = true;
            ws.Cell(currentLine, 1).Style.Font.FontSize = 12;
            var range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
            range.Merge();
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentLine++;

            ws.Cell(currentLine, 1).Value = $"Organizaton: {organization.Name}";
            ws.Cell(currentLine, 1).Style.Font.Bold = true;
            ws.Cell(currentLine, 1).Style.Font.FontSize = 11;
            range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
            range.Merge();

            currentLine++;

            ws.Cell(currentLine, 1).Value = $"Filtered Date Range: {filter.FromDate} - {filter.ToDate}";
            ws.Cell(currentLine, 1).Style.Font.Bold = true;
            ws.Cell(currentLine, 1).Style.Font.FontSize = 11;
            range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
            range.Merge();

            currentLine++;

            var compliCompliantLabel = "Yes";

            switch (filter.CompliantType)
            {
                case 1:
                    compliCompliantLabel = "Yes";
                    break;
                case 2:
                    compliCompliantLabel = "No";
                    break;
                case 3:
                    compliCompliantLabel = "N/A";
                    break;
            }

            ws.Cell(currentLine, 1).Value = $"Compliant residents: {compliCompliantLabel}";
            ws.Cell(currentLine, 1).Style.Font.Bold = true;
            ws.Cell(currentLine, 1).Style.Font.FontSize = 11;
            range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
            range.Merge();

            currentLine++;

            ws.Cell(currentLine, 1).Value = "Facility";
            ws.Cell(currentLine, 2).Value = "Audit Name";
            ws.Cell(currentLine, 3).Value = "Audit Date";
            ws.Cell(currentLine, 4).Value = "Criteria Identified";
            ws.Cell(currentLine, 5).Value = "Resident";

            range = ws.Range(ws.Cell(currentLine, 1), ws.Cell(currentLine, 5));
            range.Style.Font.Bold = true;
            range.Style.Font.FontSize = 11;

            var facilities = new int[] { };

            if (filter.FacilityIDs.Contains(0))
            {
                facilities = organization.Facilities.Select(facility => facility.Id).OfType<int>().ToArray();
            }
            else
            {
                facilities = filter.FacilityIDs;
            }

            foreach (var QuestionId in filter.QuestionsIds)
            {
                var column = await _unitOfWork.TableColumnRepository.GetCriteriaQuestionAsync(QuestionId);
                var audits = await _unitOfWork.AuditRepository.GetCompliantResidentsForCriteriaFilter(filter.OrganizationID, facilities, filter.FormVersionIds, filter.FromDate, filter.ToDate, filter.FromAuditDate, filter.ToAuditDate, QuestionId, filter.CompliantType);

                var startQuestionRow = currentLine + 1;
                var endQuestionRow = currentLine + 1;
                var startAuditNameRow = currentLine + 1;
                var endAuditNameRow = currentLine + 1;

                Audit? lastAudit = null;
                foreach (var audit in audits)
                {
                    currentLine++;
                    ws.Cell(currentLine, 1).Value = audit.Facility.Name;
                    ws.Cell(currentLine, 2).Value = audit.FormVersion.Form.Name;
                    ws.Cell(currentLine, 3).Value = audit.SubmittedDate;
                    ws.Cell(currentLine, 4).Value = column.Name;
                    ws.Cell(currentLine, 5).Value = audit.ResidentName;

                    if (lastAudit?.FormVersion.Form.Name == audit.FormVersion.Form.Name)
                    {
                        endAuditNameRow++;
                    }

                    lastAudit = audit;
                    endQuestionRow = currentLine;
                }

                range = ws.Range(ws.Cell(startAuditNameRow, 1), ws.Cell(endAuditNameRow, 1));
                range.Merge();

                range = ws.Range(ws.Cell(startQuestionRow, 3), ws.Cell(endQuestionRow, 3));
                range.Merge();
            }



            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        public async Task<byte[]> Create24KeywordReportByAI(AzureReportProcessResultDto resultAiDto)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            using Document document = new Document(pdfDocument, PageSize.A4, true);

            try
            {
                var (reportTable, filteredDate) = PdfHelper.CreateByReportAIData(resultAiDto.JsonResult);

                ManualKeywordReportAIEventHandler header =
                    new ManualKeywordReportAIEventHandler(document, resultAiDto.Organization?.Name, resultAiDto.Facility?.Name,
                      resultAiDto.Date.ToString("MMM-dd, yyyy"), resultAiDto.Time?.Replace("Time:", ""), resultAiDto.User, filteredDate);

                AddRepeatedElements(pdfDocument, header);
                SetMargins(document, header.Height);

                var keywordTable = PdfHelper.CreateKeywordsForAIReport(resultAiDto.JsonResult, resultAiDto.Keywords);
                document.Add(keywordTable);

                document.Add(reportTable);
                document.Close();
                var reportAIContent = _mapper.Map<AuditAIReport>(resultAiDto);

                reportAIContent.FacilityId = resultAiDto.Facility?.Id;
                reportAIContent.FilteredDate = filteredDate;
                reportAIContent.AuditTime = resultAiDto.Time?.Replace("Time:", "") ?? "";
                reportAIContent.AuditDate = resultAiDto.Date.ToString();
                reportAIContent.AuditorName = resultAiDto.User;
                reportAIContent.SummaryAI = CompressJson(resultAiDto.JsonResult);
                reportAIContent.CreatedAt = DateTime.UtcNow;
                reportAIContent.Status = ReportAIStatus.InProgress;
                await _unitOfWork.AuditAIReportRepository.AddAsync(reportAIContent);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string e = ex.Message;
            }

            return stream.ToArray();
        }




        public async Task<byte[]> Create24KeywordReportByAIV2(int id)
        {
            var audit = await _unitOfWork.AuditAIReportV2Repository.GetAIAuditAsync(id);

            if (audit == null)
                return null;

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            using Document document = new Document(pdfDocument, PageSize.A4, true);

            try
            {
                var (reportTable,keyList) = PdfHelper.CreateByReportAIDataV2(audit);

                ManualKeywordReportAIEventHandler header =
                    new ManualKeywordReportAIEventHandler(document, audit.Organization?.Name, audit.Facility?.Name,
                      audit.AuditDate, audit.AuditTime, audit.AuditorName, audit.FilteredDate);

                AddRepeatedElements(pdfDocument, header);
                SetMargins(document, header.Height);

                var keywordTable = PdfHelper.CreateKeywordsForAIReportV2(keyList);
                document.Add(keywordTable);

                document.Add(reportTable);
                document.Close();

            }
            catch (Exception ex)
            {
                string e = ex.Message;
            }

            return stream.ToArray();

        }
        public async Task<IEnumerable<ReportAIContentDto>> GetReportAIContentAsync(AuditAIReportFilter filter)
        {
            Expression<Func<AuditAIReport, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<AuditAIReportFilterColumn, Expression<Func<AuditAIReport, object>>>(filter.OrderBy, GetColumnOrderSelector);

            var reportAIContent = await _unitOfWork.AuditAIReportRepository.GetReportAsync(filter, orderBySelector);

            var reportAIContentDto = _mapper.Map<IEnumerable<ReportAIContentDto>>(reportAIContent);

            return reportAIContentDto;
        }

        private Expression<Func<AuditAIReport, object>> GetColumnOrderSelector(AuditAIReportFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<AuditAIReportFilterColumn, Expression<Func<AuditAIReport, object>>>
            {
                { AuditAIReportFilterColumn.OrganizationName, i => i.Facility.Organization.Name },
                { AuditAIReportFilterColumn.FacilityName, i => i.Facility.Name },
                { AuditAIReportFilterColumn.SummaryAI, i => i.SummaryAI },
                { AuditAIReportFilterColumn.Keywords, i => i.Keywords },
                { AuditAIReportFilterColumn.PdfFileName, i => i.PdfFileName },
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

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(AuditAIReportFilterColumnSource<AuditAIReportFilterColumn> columnFilter)
        {
            if (columnFilter.Column == AuditAIReportFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var queryRule = GetColumnQueryRule(columnFilter.Column);

            var columnValues = await _unitOfWork.AuditAIReportRepository.GetDistinctColumnAsync(columnFilter, queryRule);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel> GetColumnQueryRule(AuditAIReportFilterColumn columnName)
        {
            var columnQueryRuleMap = new Dictionary<AuditAIReportFilterColumn, ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>>
            {
                {
                    AuditAIReportFilterColumn.OrganizationName,
                    new ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Organization.Id, Value = i.Organization.Name }
                    }
                },
                {
                    AuditAIReportFilterColumn.FacilityName,
                    new ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Id = i.Facility.Id, Value = i.Facility.Name }
                    }
                },
                {
                    AuditAIReportFilterColumn.AuditorName,
                    new ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.AuditorName }
                    }
                },
                {
                    AuditAIReportFilterColumn.AuditTime,
                    new ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.AuditTime }
                    }
                },
                {
                    AuditAIReportFilterColumn.FilteredDate,
                    new ColumnOptionQueryRule<AuditAIReport, FilterOptionQueryModel>
                    {
                        SingleSelector = i => new FilterOptionQueryModel { Value = i.FilteredDate }
                    }
                }
            };

            if (!columnQueryRuleMap.TryGetValue(columnName, out var queryRule))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return queryRule;
        }



        public async Task<byte[]> CreateUpdated24KeywordReport(ReportAIContentDto resultAiDto, int Id)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            var pdfDocument = new PdfDocument(writer);

            using Document document = new Document(pdfDocument, PageSize.A4, true);

            try
            {
                var (reportTable, filteredDate) = PdfHelper.CreateByReportAIData(resultAiDto.SummaryAI);

                ManualKeywordReportAIEventHandler header =
                    new ManualKeywordReportAIEventHandler(document, resultAiDto.Organization?.Name, resultAiDto.Facility?.Name,
                      resultAiDto.AuditDate.ToString("MMM-dd, yyyy"), resultAiDto.AuditTime?.Replace("Time:", ""), resultAiDto.AuditorName, filteredDate);

                AddRepeatedElements(pdfDocument, header);
                SetMargins(document, header.Height);

                var keywordTable = PdfHelper.CreateKeywordsForAIReport(resultAiDto.SummaryAI, resultAiDto.Keywords);
                document.Add(keywordTable);

                document.Add(reportTable);
                document.Close();
            }
            catch (Exception ex)
            {
                string e = ex.Message;
            }

            return stream.ToArray();
        }


        public async Task<ReportAIContentDto> GetReportAIContentAsync(int id)
        {
            AuditAIReport reportAIContent = await _unitOfWork.AuditAIReportRepository.GetAuditAIReportAsync(id);

            if (reportAIContent == null)
            {
                return null;
            }

            var reportAIContentDto = _mapper.Map<ReportAIContentDto>(reportAIContent);

            return reportAIContentDto;
        }
        public async Task<AuditAIReportNoSummaryDto> UpdateReportAIDataReportAsync(AzureReportProcessResultDto resultAiDto, int ReportAIId)
        {
        
            try
            {
                AuditAIReport reportAIContent = await _unitOfWork.AuditAIReportRepository.GetAuditAIReportAsync(ReportAIId);  //GetAuditAIReportAsync(ReportAIId);
                if (reportAIContent is null)
                {
                    throw null;
                }

                reportAIContent.SummaryAI = CompressJson(resultAiDto.JsonResult);
                reportAIContent.Keywords = resultAiDto.Keywords;
                ReportAIStatus OldStatus = reportAIContent.Status;

                reportAIContent.Status = resultAiDto.Status;

                if (reportAIContent.Status == ReportAIStatus.Submitted)
                {
                    reportAIContent.SubmittedDate = DateTime.UtcNow;
                }

                if (reportAIContent.Status == ReportAIStatus.WaitingForApproval && OldStatus != ReportAIStatus.WaitingForApproval && reportAIContent.SentForApprovalDate == null)
                {
                    reportAIContent.SentForApprovalDate = DateTime.UtcNow;
                }
                await _unitOfWork.AuditAIReportRepository.UpdateAuditAIReportAsync(reportAIContent.Id, organizationId: null,
                    facilityId: null,
                    summaryAI: reportAIContent.SummaryAI, keywords: reportAIContent.Keywords, pdfFileName: null, auditorName: null, auditTime: null, auditDate: null, filteredDate: null, createdAt: null,
                      status: (int)reportAIContent.Status, submittedDate: reportAIContent.SubmittedDate, sentForApprovalDate: reportAIContent.SentForApprovalDate, null);


                return _mapper.Map<AuditAIReportNoSummaryDto>(reportAIContent);
            }
            catch (Exception ex)
            {
                string e = ex.Message;
            }
            return null;
        }

        public async Task<ReportAIStatus> SetAIAuditStatusAsync(int reportAIContentId, ReportAIStatus status)
        {
            List<string> listModifiedProps = new List<string>();

            AuditAIReport reportAIContent = await _unitOfWork.AuditAIReportRepository.GetAuditAIReportSelectedColumnsAsync(reportAIContentId);

            if (reportAIContent is null)
            {
                throw new NotFoundException();
            }

            ReportAIStatus OldStatus = reportAIContent.Status;

            reportAIContent.Status = status;
            listModifiedProps.Add(nameof(reportAIContent.Status));

            if (status == ReportAIStatus.Submitted)
            {
                reportAIContent.SubmittedDate = DateTime.UtcNow;
                listModifiedProps.Add(nameof(reportAIContent.SubmittedDate));
            }

            if (status == ReportAIStatus.WaitingForApproval && OldStatus != ReportAIStatus.WaitingForApproval && reportAIContent.SentForApprovalDate == null)
            {
                reportAIContent.SentForApprovalDate = DateTime.UtcNow;
                listModifiedProps.Add(nameof(reportAIContent.SentForApprovalDate));
            }

            _unitOfWork.AuditAIReportRepository.Update(reportAIContent, listModifiedProps.ToArray());
            await _unitOfWork.SaveChangesAsync();

            return reportAIContent == null ? OldStatus : reportAIContent.Status;
        }

   
        private byte[] CompressJson(string jsonResult)
        {

            JObject jObject = JObject.Parse(jsonResult);
            string serializedResult = JsonConvert.SerializeObject(jObject);
            var bytes = Encoding.UTF8.GetBytes(serializedResult);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
        public async Task<ReportAIContentDto> SaveReportAIDataAsync(AzureReportProcessResultDto resultAiDto)
        {
            try
            {
                AuditAIReport reportAIContent = new AuditAIReport();
                reportAIContent.OrganizationId = resultAiDto.Organization.Id;
                reportAIContent.Keywords = resultAiDto.Keywords.Replace('\n',' ').Trim();
                reportAIContent.FacilityId = resultAiDto.Facility?.Id;
                reportAIContent.FilteredDate = GetReportAIFilteredDate(resultAiDto.JsonResult);
                reportAIContent.AuditTime = resultAiDto.Time?.Replace("Time:", "") ?? "";
                reportAIContent.AuditDate = resultAiDto.Date.ToString();
                reportAIContent.AuditorName = resultAiDto.User;
                reportAIContent.SummaryAI = CompressJson(resultAiDto.JsonResult);
                reportAIContent.CreatedAt = DateTime.UtcNow;
                reportAIContent.PdfFileName = resultAiDto.ReportFileName;
                reportAIContent.Status = ReportAIStatus.InProgress;
                reportAIContent.State = AIAuditState.Active;
                int id = await _unitOfWork.AuditAIReportRepository.InsertAuditAIReportAsync(reportAIContent);
                reportAIContent.Id = id;
               
                return _mapper.Map<ReportAIContentDto>(reportAIContent);
            }
            catch(Exception ex)
            {

            }
            return null;
         }



        public async Task MigrateGzipToSqlCompressAsync()
        {

            await _unitOfWork.AuditAIReportRepository.MigrateToSQLCompress();
        }

        private string GetReportAIFilteredDate(string jsonResult)
        {
            JObject aiRep = JObject.Parse(jsonResult);
            string filteredDate = "";

            foreach (JProperty property in aiRep.Properties())
            {
                if (property.Value.HasValues)
                {

                    if (property.Value.Type == JTokenType.Array)
                    {
                        var items = (JArray)property.Value;
                        string date = string.Empty;

                        foreach (JObject content in items.Children<JObject>())
                        {
                            foreach (JProperty prop in content.Properties())
                            {
                                if (prop.Name.ToUpper() == "Date".ToUpper())
                                {
                                    var dateTIme = DateTime.ParseExact(prop.Value.ToString(), "MM/dd/yyyy HH:mm", null);

                                    date = dateTIme.ToShortDateString();
                                    if (string.IsNullOrEmpty(filteredDate))
                                        filteredDate = date;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            return filteredDate;
        }

        public async Task<bool> UpdateAIAuditState(int reportAIContentId, AIAuditState state)
        {

            await _unitOfWork.AuditAIReportRepository.UpdateAuditAIReportAsync(reportAIContentId,organizationId: null,
                facilityId: null,
                summaryAI: null, keywords:null, pdfFileName:null, auditorName:null, auditTime:null,auditDate: null, filteredDate: null, createdAt: null,
                  status:null, submittedDate:null, sentForApprovalDate:null,(int)state);
           
            return true;
        }

    }

}

