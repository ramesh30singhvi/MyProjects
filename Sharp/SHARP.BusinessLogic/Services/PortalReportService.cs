using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using SHARP.DAL;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.Common.Constants;
using Microsoft.EntityFrameworkCore;
using SHARP.BusinessLogic.DTO.Portal;
using System.Net.Mime;

namespace SHARP.BusinessLogic.Services
{
    public class PortalReportService : IPortalReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;
        private readonly IReportRequestService _reportRequestService;
        private readonly IFacilityService _facilityService;
        public PortalReportService(IUnitOfWork unitOfWork, IAuditService auditservice,IFacilityService facilityService,
            IUserService userService, IConfiguration configuration, IMapper mapper, IReportRequestService reportRequestService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _auditService = auditservice;
            _userService = userService;
            _reportRequestService = reportRequestService;
            _facilityService = facilityService;
        }
        public async Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsAsync(PortalReportFilter filter)
        {
            Expression<Func<PortalReport, object>> orderBySelector =
                 OrderByHelper.GetOrderBySelector<PortalReportFilterColumn, Expression<Func<PortalReport, object>>>(filter.OrderBy, GetColumnOrderSelector);

            //filter.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            var tuple = await _unitOfWork.PortalReportRepository.GetPortalReportsAsync(filter, orderBySelector);
            return new Tuple<IReadOnlyCollection<PortalReportDto>, int>(_mapper.Map<IReadOnlyCollection<PortalReportDto>>(tuple.Item1), tuple.Item2);
        }
        private Expression<Func<PortalReport, object>> GetColumnOrderSelector(PortalReportFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<PortalReportFilterColumn, Expression<Func<PortalReport, object>>>
            {
                { PortalReportFilterColumn.OrganizationName, i => i.Facility.Organization.Name },
                { PortalReportFilterColumn.FacilityName, i => i.Facility.Name },
                { PortalReportFilterColumn.ReportName, i => i.Name },
                { PortalReportFilterColumn.ReportCategoryName, i => i.ReportCategory.ReportCategoryName },
                { PortalReportFilterColumn.Date, i => i.CreatedAt }

            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }
        public Task<OptionDto[]> GetReportTypes()
        {
            var types = _unitOfWork.ReportTypeRepository.GetAll();

            return Task.FromResult(_mapper.Map<OptionDto[]>(types));
        }
        private async Task<(string blobNameReport, string containerName, string Uri, string error)> UploadPortalReportToStorage(IFormFile file)
        {
            var stoageURL = _configuration["PortalReportStorage"] ?? string.Empty;


            BlobServiceClient blobServiceClient = new BlobServiceClient(stoageURL);

            var reportContainer = _configuration["PortalReportContainer"] ?? "pdf-pcc-report";
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(reportContainer);
            string ext = System.IO.Path.GetExtension(file.FileName);
            string name = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
            name = $"{name}-{DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss")}.{ext}";
            BlobClient blobClient = containerClient.GetBlobClient(name);

            var resp = await blobClient.UploadAsync(file.OpenReadStream(), true);
            if (resp.GetRawResponse().Status != 201)
            {
                return ("", "", "", resp.GetRawResponse().IsError ? resp.GetRawResponse().ReasonPhrase : "Can not upload report");
            }

            return (name, blobClient.BlobContainerName, blobClient.Uri.OriginalString, "");
        }

        private ReportType GetReportType(string contextType)
        {
            if (contextType == "application/pdf")
                return _unitOfWork.ReportTypeRepository.GetAll().FirstOrDefault(x => x.TypeName == "PDF");

            return _unitOfWork.ReportTypeRepository.GetAll().FirstOrDefault(x => x.TypeName == "Excel");
        }
        public async Task<Tuple<PortalReportDto, string>> UploadNewReportAsync(UploadNewReportDto reportUploadDto)
        {
            var fileExtension = System.IO.Path.GetExtension(reportUploadDto.FileUpload.FileName);
            var (reportName, reportContainer, uri, error) = await UploadPortalReportToStorage(reportUploadDto.FileUpload);
            if (!string.IsNullOrEmpty(error))
            {
                return new Tuple<PortalReportDto, string>(null, error);
            }
            var userId = _userService.GetLoggedUserId();
            var portal = new PortalReport();
            portal.Name = System.IO.Path.GetFileNameWithoutExtension(reportUploadDto.FileUpload.FileName);
            portal.ReportCategoryId = reportUploadDto.ReportCategoryId;
            portal.ReportTypeId = GetReportType(reportUploadDto.FileUpload.ContentType).Id;
            portal.AuditId = null;
            portal.CreatedAt = DateTime.UtcNow;
            portal.CreatedByUserID = userId;
            portal.StorageReportName = reportName;
            portal.StorageURL = uri;
            portal.StorageContainerName = reportContainer;
            portal.OrganizationId = reportUploadDto.OrganizationId;
            portal.FacilityId = reportUploadDto.FacilityId;
            await _unitOfWork.PortalReportRepository.AddAsync(portal);

            await _unitOfWork.SaveChangesAsync();

            var portalSaved = await _unitOfWork.PortalReportRepository.GetPortalReportAsync(portal.Id);
            return new Tuple<PortalReportDto, string>(_mapper.Map<PortalReportDto>(portalSaved), "");
        }

        public Task<OptionDto[]> GetReportRanges()
        {
            var ranges = _unitOfWork.ReportRangeRepository.GetAll();

            return Task.FromResult(_mapper.Map<OptionDto[]>(ranges));
        }


        public Task<OptionDto[]> GetReportCategories()
        {
            var categories = _unitOfWork.ReportCategoryRepository.GetAll().OrderBy(x => x.ReportCategoryName);

            return Task.FromResult(_mapper.Map<OptionDto[]>(categories));
        }

        public async Task<SendReportDto> AddToSending(string userEmail, IList<int> portalReports, string tokenCode)
        {
            var userLoggerId = _userService.GetLoggedUserId();
            var sendReportDto = new SendReportDto();
            var namesReports = new List<PortalReportDto>();
            foreach (var portalReportId in portalReports)
            {
                var report = await _unitOfWork.PortalReportRepository.GetPortalReportAsync(portalReportId);
                if (report == null)
                    continue;
                var sendReportToUser = new SendReportToUser();
                sendReportToUser.PortalReportId = report.Id;
                sendReportToUser.UserEmail = userEmail;
                sendReportToUser.SendByUserId = userLoggerId;
                sendReportToUser.Token = tokenCode;
                sendReportToUser.CreatedAt = DateTime.UtcNow;
                sendReportToUser.Status = false;
                sendReportToUser.FacilityId = report.FacilityId;

                sendReportDto.Facility = new OptionDto() {  Id = report.Facility.Id, Name = report.Facility.Name };
                sendReportDto.Organization = _mapper.Map<OrganizationDto>(report.Organization);
                sendReportDto.ExpiredTime = sendReportToUser.CreatedAt.AddDays(1).Ticks;
                namesReports.Add(_mapper.Map<PortalReportDto>(report));
                await _unitOfWork.SendReportToUserRepository.AddAsync(sendReportToUser);
            }
            await _unitOfWork.SaveChangesAsync();
            sendReportDto.Reports = namesReports;
            return sendReportDto;
        }

        public async Task<byte[]> DownloadPortalReport(SelectedDto portalReportSelectedDto)
        {
            if (portalReportSelectedDto == null)
                return null;

            if (!portalReportSelectedDto.SelectedIds.Any())
                return null;
            var report = await _unitOfWork.PortalReportRepository.GetPortalReportAsync(portalReportSelectedDto.SelectedIds.FirstOrDefault<int>());
            if (report == null)
                return null;

            var userLoggerId = _userService.GetLoggedUserId();

            DownloadsTracking prevDownloadsTracking = await _unitOfWork.DownloadsTrackingRepository.GetPortalDownloadsTrackingByUserIdPortalReportIdAsync(userLoggerId, report.Id);

            if (prevDownloadsTracking == null)
            {
                DownloadsTracking downloadsTracking = new DownloadsTracking();
                downloadsTracking.UserId = userLoggerId;
                downloadsTracking.PortalReportId = report.Id;
                downloadsTracking.DateAndTime = DateTime.UtcNow;
                await _unitOfWork.DownloadsTrackingRepository.AddAsync(downloadsTracking);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                prevDownloadsTracking.DateAndTime = DateTime.UtcNow;
                _unitOfWork.DownloadsTrackingRepository.Update(prevDownloadsTracking);
                await _unitOfWork.SaveChangesAsync();
            }

            if (report.Audit != null)
            {
                Audit audit = await _unitOfWork.AuditRepository.GetAuditWithTypeAsync(report.Audit.Id);

                if(audit.FormVersion.Form.AuditType.Name == CommonConstants.TRACKER )
                    return await _auditService.GetAuditExcelAsync(report.Audit.Id);

                return await _auditService.GetAuditPdfAsync(report.Audit.Id);
            }else if(report.ReportRequestId != null)
            {
                return await _reportRequestService.GetReportAsync(report.StorageURL);
            }
            var result=  await GetReportAsync(report.StorageReportName,report.StorageContainerName);
            return result.Item1;
        }
        public async Task<Tuple<byte[],string>> GetReportAsync(string report,string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["PortalReportStorage"]);

            BlobContainerClient containerClient = string.IsNullOrEmpty(containerName) ?
                blobServiceClient.GetBlobContainerClient(_configuration["PortalReportContainer"]) : blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(report);

            BlobDownloadResult file = await blobClient.DownloadContentAsync();

            return new Tuple<byte[],string>(file.Content.ToArray(),file.Details.ContentType);
        }

        public async Task<Tuple<bool, string,int>> HasAccess(FacilityAccessDto facilityAccess)
        {
            var facilities =  await _unitOfWork.FacilityRepository.GetFacilitiesByNameAsync(facilityAccess.FacilityName);
            if (!facilities.Any())
                return new Tuple<bool, string,int>(false, "Facility did not find",0);
            foreach (var facility in facilities)
            {
                if(facility.OrganizationId == 45 )
                    return new Tuple<bool, string, int>(false, "No access", 0);

                var result = await _unitOfWork.SendReportToUserRepository.HasAccessFromEmail(facility.Id, facilityAccess.Password);
                if (result.Item1 == true)
                    return result;
            }
            return new Tuple<bool, string,int>(false, "No access",0);
        }

        public async Task<bool> DeletePortalReport(int id)
        {
            var report = await _unitOfWork.PortalReportRepository.GetAsync(id);
            if (report == null) return false;

            _unitOfWork.PortalReportRepository.Remove(report);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsByPageAsync(PortalReportFilter filter)
        {
            Expression<Func<PortalReport, object>> orderBySelector =
                  OrderByHelper.GetOrderBySelector<PortalReportFilterColumn, Expression<Func<PortalReport, object>>>(filter.OrderBy, GetColumnOrderSelector);

            //filter.UserOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            var tuple = await _unitOfWork.PortalReportRepository.GetPortalReportsAsyncByPage(filter, orderBySelector);
            return new Tuple<IReadOnlyCollection<PortalReportDto>, int>(_mapper.Map<IReadOnlyCollection<PortalReportDto>>(tuple.Item1), tuple.Item2);

        }


        public async Task<Tuple<IReadOnlyCollection<PortalReportDto>, int>> GetPortalReportsForFacilityAsync(PortalReportFacilityViewFilter filter)
        {
             
            Expression<Func<PortalReport, object>> orderBySelector =
                    OrderByHelper.GetOrderBySelector<PortalReportFilterColumn, Expression<Func<PortalReport, object>>>(filter.OrderBy, GetColumnOrderSelector);

            var reportFilter = _mapper.Map<PortalReportFilter>(filter);
            var facilitiesFilter = new List<FilterOption>();

            //if (filter?.Organization?.Id == 45)
            //    return new Tuple<IReadOnlyCollection<PortalReportDto>, int>(new List<PortalReportDto>().ToArray(), 0);

            var facilitiesIds = new List<int>();
            if(filter.Facility != null && filter.Facility.Id > 0 )
            {
                facilitiesIds.Add(filter.Facility.Id ?? 0);
                facilitiesFilter.Add(filter.Facility);
            }
            else
            {
                var facilities = await _facilityService.GetFacilityOptionsAsync(filter?.Organization?.Id ?? 0);
                var ids = facilities.Select(x => x.Id);
                var filterOptions = _mapper.Map< IReadOnlyCollection<FilterOption>>(facilities);
                facilitiesIds.AddRange(ids);
                facilitiesFilter.AddRange(filterOptions);
            }

            reportFilter.Facilities = facilitiesFilter;
            var selectedReports = await _unitOfWork.SendReportToUserRepository.GetReportForUserAsync(facilitiesIds.ToArray());
            if (!selectedReports.Any())
              return  new Tuple<IReadOnlyCollection<PortalReportDto>, int>(new List<PortalReportDto>().ToArray(), 0);

            reportFilter.ReportIds = selectedReports.Distinct().ToList();

            var tuple = await _unitOfWork.PortalReportRepository.GetPortalReportsAsyncByPage(reportFilter, orderBySelector);

            return new Tuple<IReadOnlyCollection<PortalReportDto>, int>(_mapper.Map<IReadOnlyCollection<PortalReportDto>>(tuple.Item1), tuple.Item2);

        }
        public Task<OptionDto[]> GetPortalFeatures()
        {
            var portalFeatures = _unitOfWork.PortalFeatureRepository.GetAll();

            return Task.FromResult(_mapper.Map<OptionDto[]>(portalFeatures));

        }

        public bool IsEmailSent(string email, string emailToken)
        {
            var allReportSent =  _unitOfWork.SendReportToUserRepository.GetAll().Where( x => x.Token == emailToken).ToArray();
            if (!allReportSent.Any())
            {
                return false;
            }

            var includeEmail = allReportSent.Where( x => x.UserEmail.Split(",").Contains(email));
            if (!includeEmail.Any())
            {
                return false;
            }

            return true;
        }
        public bool IsTokenExpired(string email, string emailToken)
        {
            var allReportSent = _unitOfWork.SendReportToUserRepository.GetAll().Where(x => x.Token == emailToken).ToArray();

            var includeEmail = allReportSent.Where(x => x.UserEmail.Split(",").Contains(email));
            if (!includeEmail.Any())
            {
                return false;
            }
            var specificReports = includeEmail.LastOrDefault(x => DateTime.UtcNow.AddDays(-1) <= x.CreatedAt);
            return specificReports != null;
        }

        public async Task<Tuple<IReadOnlyCollection<DownloadsTrackingDto>, int>> GetPortalDownloadsTrackingAsync(PortalDownloadsTrackingViewFilter filter)
        {
            Expression<Func<DownloadsTracking, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<DownloadsTrackingFilterColumn, Expression<Func<DownloadsTracking, object>>>(filter.OrderBy, GetPortalDownloadsTrackingColumnOrderSelector);

            var tuple = await _unitOfWork.DownloadsTrackingRepository.GetPortalDownloadsTrackingAsync(filter, orderBySelector);
            return new Tuple<IReadOnlyCollection<DownloadsTrackingDto>, int>(_mapper.Map<IReadOnlyCollection<DownloadsTrackingDto>>(tuple.Item1), tuple.Item2);
        }

        private Expression<Func<DownloadsTracking, object>> GetPortalDownloadsTrackingColumnOrderSelector(DownloadsTrackingFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<DownloadsTrackingFilterColumn, Expression<Func<DownloadsTracking, object>>>
            {
                { DownloadsTrackingFilterColumn.Name, i => i.PortalReport.Name },
                { DownloadsTrackingFilterColumn.ReportDate, i => i.PortalReport.CreatedAt },
                { DownloadsTrackingFilterColumn.NumberOfDownloads, i => i.NumberOfDownloads },
                { DownloadsTrackingFilterColumn.DateAndTime, i => i.DateAndTime }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }



        public async Task<Tuple<byte[], string>> DownloadPortalReportForAttachment(int reportId)
        {
            if (reportId == 0)
                return null;


            var report = await _unitOfWork.PortalReportRepository.GetPortalReportAsync(reportId);
            if (report == null)
                return null;
            if (report.Audit != null)
            {
                Audit audit = await _unitOfWork.AuditRepository.GetAuditWithTypeAsync(report.Audit.Id);

                if (audit.FormVersion.Form.AuditType.Name == CommonConstants.TRACKER || report.ReportTypeId == 2)
                {
                    var file = await _auditService.GetAuditExcelAsync(report.Audit.Id);
                    return new Tuple<byte[], string>(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                }
                var filePdf = await _auditService.GetAuditPdfAsync(report.Audit.Id);
                return new Tuple<byte[], string>(filePdf, MediaTypeNames.Application.Pdf);

            }
            else if (report.ReportRequestId != null)
            {

                var filePdfReportRequest = await _reportRequestService.GetReportAsync(report.StorageURL);
                return new Tuple<byte[], string>(filePdfReportRequest, MediaTypeNames.Application.Pdf);
            }


            return await GetReportAsync(report.StorageReportName, report.StorageContainerName);
            
        }

    }
}
