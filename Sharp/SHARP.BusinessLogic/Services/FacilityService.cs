using AutoMapper;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL;
using System.Collections.Generic;
using System.Threading.Tasks;
using SHARP.DAL.Models;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.Helpers;
using System.Linq.Expressions;
using System;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Extensions;
using System.Linq;
using SHARP.BusinessLogic.Extensions;
using SHARP.DAL.Models.QueryModels;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json.Linq;

namespace SHARP.BusinessLogic.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public FacilityService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<IEnumerable<FacilityDto>> GetFacilitiesAsync(FacilityFilter filter)
        {
            var orderBySelector = OrderByHelper.GetOrderBySelector<FacilityFilterColumn, Expression<Func<Facility, object>>>(
                    filter.OrderBy,
                    GetOrderBySelector);

            var facilities = await _unitOfWork.FacilityRepository.GetAsync(filter, orderBySelector);

            return _mapper.Map<IEnumerable<FacilityDto>>(facilities);
        }

        public async Task<FacilityDetailsDto> GetFacilityDetailsAsync(int id)
        {
            Facility facility = await _unitOfWork.FacilityRepository.GetFacilityAsync(id);

            return _mapper.Map<FacilityDetailsDto>(facility);
        }

        public async Task<bool> AddFacilityAsync(AddFacilityDto addFacilityDto)
        {
            Facility facility = _mapper.Map<Facility>(addFacilityDto);

            await _unitOfWork.FacilityRepository.AddAsync(facility);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditFacilityAsync(EditFacilityDto editFacilityDto)
        {
            Facility facility = await _unitOfWork.FacilityRepository.FirstOrDefaultAsync(
                facility => facility.Id == editFacilityDto.Id,
                facility => facility.Recipients);

            _mapper.Map(editFacilityDto, facility);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IReadOnlyCollection<FacilityOptionDto>> GetFacilityOptionsAsync(int organizationId)
        {
            IReadOnlyCollection<Facility> facilities = await _unitOfWork.FacilityRepository.GetListAsync(
                facility => facility.OrganizationId == organizationId && facility.Active,
                facility => facility.Name,
                include: facility => facility.TimeZone,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<FacilityOptionDto>>(facilities);
        }

        public async Task<IReadOnlyCollection<FacilityOptionDto>> GetFacilityOptionsAsync(FacilityOptionFilter filter)
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            ICollection<int> userFacilityIds = await _userService.GetUserFacilityIdsAsync();

            if (userFacilityIds.Any())
            {
                filter.FacilityIds = userFacilityIds.ToList();
            }

            if (userOrganizationIds.Any() && !filter.OrganizationIds.Any())
            {
                filter.OrganizationIds = userOrganizationIds.ToList();
            }

            IReadOnlyCollection<Facility> facilities = await _unitOfWork.FacilityRepository.GetFacilityOptionsAsync(filter);

            return _mapper.Map<IReadOnlyCollection<FacilityOptionDto>>(facilities);
        }

        public async Task<IReadOnlyCollection<TimeZoneOptionDto>> GetTimeZoneOptionsAsync()
        {
            IReadOnlyCollection<FacilityTimeZone> timeZones = await _unitOfWork.FacilityTimeZoneRepository.GetListAsync(
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<TimeZoneOptionDto>>(timeZones);
        }

        public async Task<bool> IsFacilityNameAlreadyExist(string facilityName, int? organizationId, int? facilityId = null)
        {
            Expression<Func<Facility, bool>> predicate = i =>
            i.Name.ToUpper().Trim() == facilityName.ToUpper().Trim() &&
            i.Organization.Id == organizationId /*&&
            i.Active == true*/;

            if (facilityId.HasValue)
            {
                predicate = predicate.And(i => i.Id != facilityId.Value);
            }

            return await _unitOfWork.FacilityRepository.ExistsAsync(predicate);
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(FacilityFilterColumnSource<FacilityFilterColumn> columnData)
        {
            if (columnData.Column == FacilityFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var columnSelector = GetColumnSelector(columnData.Column);

            var columnValues = await _unitOfWork.FacilityRepository.GetDistinctColumnAsync(columnData, columnSelector);

            if (columnData.Column == FacilityFilterColumn.TimeZoneName)
            {
                List<FilterOption> timeZoneNames = new List<FilterOption>();

                foreach (var columnValue in columnValues)
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(columnValue.Value);

                    timeZoneNames.Add(new FilterOption() { Id = columnValue.Id, Value = $"{timeZoneInfo.DisplayName} ({columnValue.Description})" });
                }

                return timeZoneNames;
            }

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private Expression<Func<Facility, object>> GetOrderBySelector(FacilityFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<FacilityFilterColumn, Expression<Func<Facility, object>>>
            {
                {
                    FacilityFilterColumn.Name,
                    i => i.Name
                },
                {
                    FacilityFilterColumn.TimeZoneName,
                    i => i.TimeZone.Name
                },
                {
                    FacilityFilterColumn.RecipientsCount,
                    i => i.Recipients.Count
                },
                {
                    FacilityFilterColumn.Active,
                    i => i.Active
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var selector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return selector;
        }

        private Expression<Func<Facility, FilterOptionQueryModel>> GetColumnSelector(FacilityFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<FacilityFilterColumn, Expression<Func<Facility, FilterOptionQueryModel>>>
            {
                {
                    FacilityFilterColumn.Name,
                    i => new FilterOptionQueryModel { Value = i.Name }
                },
                {
                    FacilityFilterColumn.TimeZoneName,
                    i => new FilterOptionQueryModel { Id = i.TimeZoneId, Value = i.TimeZone.Name, Description = i.TimeZone.ShortName }
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        //For AutoTests
        public async Task<bool> DeleteFacilityAsync(int id)
        {
            var facility = await _unitOfWork.FacilityRepository.GetAsync(id);

            if (facility == null)
            {
                throw new NotFoundException($"Facility with Id: {id} is not found");
            }

            _unitOfWork.FacilityRepository.Remove(facility);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddFacilityRecipientsAsync(AddEmailRecipientsDto addEmailRecipientsFacilityDto)
        {
            var facility = await _unitOfWork.FacilityRepository.GetAsync(addEmailRecipientsFacilityDto.Id);

            if (facility == null)
            {
                return false;
            }
            var allRecipients =  _unitOfWork.FacilityRecipientRepository.GetAll();
            var emailsForFacility = allRecipients.Where(x => x.Id == facility.Id);

             foreach (var recipient in addEmailRecipientsFacilityDto.Emails)
             {
                    if (emailsForFacility.Any(x => x.Email.ToLower() == recipient.ToLower()))
                        continue;
                    var newRecipient = new FacilityRecipient();
                    newRecipient.FacilityId = facility.Id;
                    newRecipient.Email = recipient.Trim();

                    _unitOfWork.FacilityRecipientRepository.Add(newRecipient);
             }
               
             await  _unitOfWork.SaveChangesAsync();

            return true;
        }


        public async Task<string[]> GetEmailRecipients(int facilityID)
        {
            var facilitySelected = await _unitOfWork.FacilityRepository.GetFacilityAsync(facilityID);
            if(facilitySelected == null)
            { 
                return null;
            }
            Expression<Func<FacilityRecipient, bool>> predicate = null;
            IReadOnlyCollection<FacilityRecipient> facilityRecipients = await _unitOfWork.FacilityRecipientRepository.GetListAsync(
                facilityValue => facilityValue.FacilityId == facilitySelected.Id,
                asNoTracking: true);
          

            if(!facilityRecipients.Any())
            {
                return null;
            }
            return facilityRecipients.Select( x => x.Email ).ToArray();

        }
        public async Task ExportFacilityRecipientsToAnotherDB()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Data Source=tcp:sharp-live-sql.database.windows.net,1433;Initial Catalog=sharp-live-db;User Id=sharpuser;Password=DkgjEOR5$*7Hk";  // "Data Source=sharp-test.database.windows.net,1433;Initial Catalog=sharp-test-new;User Id=sharpuser@sharp-test;Password=Qwerty1!";
                conn.Open();

                var allrecipients = _unitOfWork.FacilityRecipientRepository.GetAll();

                foreach (var recipient in allrecipients)
                {
                    SqlCommand command = new SqlCommand("SELECT * FROM FacilityRecipients WHERE Email = @0 and FacilityId = @1", conn);
                    // Add the parameters.
                    command.Parameters.Add(new SqlParameter("0", recipient.Email));
                    command.Parameters.Add(new SqlParameter("1", recipient.FacilityId));
                    /* Get the rows and display on the screen! 
                     * This section of the code has the basic code
                     * that will display the content from the Database Table
                     * on the screen using an SqlDataReader. */

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        
                        while (reader.Read())
                        {
                            if(reader.FieldCount > 0)
                                Debug.WriteLine(String.Format("{0} \t | {1} \t ",
                                    reader[0], reader[1]));
                            continue;
                        }
                    }

                    SqlCommand insertCommand = new SqlCommand("INSERT INTO FacilityRecipients (FacilityId, Email) VALUES (@0, @1)", conn);

                    // In the command, there are some parameters denoted by @, you can 
                    // change their value on a condition, in my code they're hardcoded.

                    insertCommand.Parameters.Add(new SqlParameter("0", recipient.FacilityId));
                    insertCommand.Parameters.Add(new SqlParameter("1", recipient.Email));


                    // Execute the command, and print the values of the columns affected through
                    // the command executed.

                    Debug.WriteLine("Commands executed! Total rows affected are " + insertCommand.ExecuteNonQuery());
                }
                // use the connection here
            }
        }

        public async Task<FacilityDetailsDto> GetFacilityByNameAsync(string name, int organizationid)
        {
            var facility = await _unitOfWork.FacilityRepository.GetFacilityByNameAsync(name);
            return _mapper.Map<FacilityDetailsDto>(facility);
        }



        public async Task<string> GetProviderInformationByFacilityAsync(string facilityName)
        {
            string result = string.Empty;

            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://data.cms.gov");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "provider-data/api/1/search?fulltext=Provider%20Information%20&keyword=Ratings&page=1&page-size=10000&sort=modified&sort-order=desc&theme=Nursing%20homes%20including%20rehab%20services&facets=0";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            string identifier = GetCMSDatasetIdentifier(res);

            if (!string.IsNullOrEmpty(identifier))
            {
                using var client2 = new HttpClient();

                client2.BaseAddress = new Uri("https://data.cms.gov");
                client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url2 = "provider-data/api/1/datastore/query/" + WebUtility.UrlEncode(identifier) + "?keys=true&offset=0&conditions[0][property]=provider_name&conditions[0][operator]==&conditions[0][value]=" + WebUtility.UrlEncode(facilityName);
                HttpResponseMessage response2 = await client.GetAsync(url2);
                response2.EnsureSuccessStatusCode();
                var data = await response2.Content.ReadAsStringAsync();
                result = data;
            }

            return result;
        }

        public async Task<string> GetHealthDeficienciesByFacilityAsync(string facilityName)
        {
            string result = string.Empty;

            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://data.cms.gov");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "provider-data/api/1/search/facets?fulltext=Health%20Deficiencies&page=1&page-size=25&sort=modified&sort-order=desc&theme=Nursing%20homes%20including%20rehab%20services&facets=keyword";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            string identifier = GetCMSDatasetIdentifier(res);

            if (!string.IsNullOrEmpty(identifier))
            {
                using var client2 = new HttpClient();

                client2.BaseAddress = new Uri("https://data.cms.gov");
                client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url2 = "provider-data/api/1/datastore/query/" + WebUtility.UrlEncode(identifier) + "?keys=true&offset=0&conditions[0][property]=provider_name&conditions[0][operator]==&conditions[0][value]=" + WebUtility.UrlEncode(facilityName);
                HttpResponseMessage response2 = await client.GetAsync(url2);
                response2.EnsureSuccessStatusCode();
                var data = await response2.Content.ReadAsStringAsync();
                result = data;
            }

            return result;
        }

        public async Task<string> GetMDSQualityMeasuresDataByFacilityAsync(string facilityName)
        {
            string result = string.Empty;

            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://data.cms.gov");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "provider-data/api/1/search/facets?fulltext=MDS%20Quality%20Measures&keyword=MDS&page=1&page-size=25&sort=modified&sort-order=desc&theme=Nursing%20homes%20including%20rehab%20services&facets=keyword";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            string identifier = GetCMSDatasetIdentifier(res);

            if (!string.IsNullOrEmpty(identifier))
            {
                using var client2 = new HttpClient();

                client2.BaseAddress = new Uri("https://data.cms.gov");
                client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url2 = "provider-data/api/1/datastore/query/" + WebUtility.UrlEncode(identifier) + "?keys=true&offset=0&conditions[0][property]=provider_name&conditions[0][operator]==&conditions[0][value]=" + WebUtility.UrlEncode(facilityName);
                HttpResponseMessage response2 = await client.GetAsync(url2);
                response2.EnsureSuccessStatusCode();
                var data = await response2.Content.ReadAsStringAsync();
                result = data;
            }

            return result;
        }

        public async Task<string> GetMedicareClaimsQualityMeasuresDataByFacilityAsync(string facilityName)
        {
            string result = string.Empty;

            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://data.cms.gov");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "provider-data/api/1/search/facets?fulltext=Medicare%20Claims%20Quality%20Measures&page=1&page-size=25&sort=modified&sort-order=desc&theme=Nursing%20homes%20including%20rehab%20services&facets=keyword";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            string identifier = GetCMSDatasetIdentifier(res);

            if (!string.IsNullOrEmpty(identifier))
            {
                using var client2 = new HttpClient();

                client2.BaseAddress = new Uri("https://data.cms.gov");
                client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var url2 = "provider-data/api/1/datastore/query/" + WebUtility.UrlEncode(identifier) + "?keys=true&offset=0&conditions[0][property]=provider_name&conditions[0][operator]==&conditions[0][value]=" + WebUtility.UrlEncode(facilityName);
                HttpResponseMessage response2 = await client.GetAsync(url2);
                response2.EnsureSuccessStatusCode();
                var data = await response2.Content.ReadAsStringAsync();
                result = data;
            }

            return result;
        }


        private string GetCMSDatasetIdentifier(string response)
        {
            string result = string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(response))
                {
                    JObject jsonObject = JObject.Parse(response);

                    var results = jsonObject["results"];

                    foreach (var item in results)
                    {
                        string propName = ((JProperty)item).Name;
                        if (propName.Contains("dkan_dataset/"))
                        {
                            result = jsonObject["results"][propName]["%Ref:distribution"][0]["identifier"].ToString();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }



        public async Task<AuditSummaryDto[]> GetAuditSummaryAsync(int facilityId, string fromDate, string toDate)
        {
            List<AuditSummaryDto> auditSummaryList = new List<AuditSummaryDto>();

            List<AuditSummary> auditSummary = await _unitOfWork.FacilityRepository.GetAuditSummaryAsync(facilityId, fromDate, toDate);

            var newList = auditSummary.GroupBy(g => new { g.TypeOfAudit, g.NumberOfAudits, g.CompliancePercentage }).Select(s => s.Key).ToList();

            foreach (var item in newList)
            {
                AuditSummaryDto auditSummaryDto = new AuditSummaryDto();
                auditSummaryDto.TypeOfAudit = item.TypeOfAudit;
                auditSummaryDto.NumberOfAudits = item.NumberOfAudits;
                auditSummaryDto.CompliancePercentage = item.CompliancePercentage;

                var summaryOfFindingslist =
                    auditSummary.Where(x => x.TypeOfAudit.Equals(item.TypeOfAudit) && x.NumberOfAudits.Equals(item.NumberOfAudits))
                        .Select(s => new SummaryOfFindings
                        {
                            NonCompliantQuestion = s.NonCompliantQuestion,
                            NonCompliantResident = s.NonCompliantResident
                        })
                        .Where(w => !string.IsNullOrWhiteSpace(w.NonCompliantResident) || !string.IsNullOrWhiteSpace(w.NonCompliantResident))
                        .ToList();

                var newFindingsList = summaryOfFindingslist.GroupBy(g => new { g.NonCompliantQuestion }).Select(s => s.Key).ToList();

                List<SummaryOfFindings> groupedfindingsList = new List<SummaryOfFindings>();

                foreach (var itemFindings in newFindingsList)
                {
                    var list = summaryOfFindingslist.Where(x => x.NonCompliantQuestion.Equals(itemFindings.NonCompliantQuestion)).Select(s => s.NonCompliantResident).ToArray();

                    SummaryOfFindings findings = new SummaryOfFindings();
                    findings.NonCompliantQuestion = itemFindings.NonCompliantQuestion;
                    findings.NonCompliantResident = string.Join("\n", list);
                    groupedfindingsList.Add(findings);
                }

                auditSummaryDto.SummaryOfFindings = groupedfindingsList;

                auditSummaryList.Add(auditSummaryDto);
            }

            return auditSummaryList.ToArray();
        }
  

    }
}
