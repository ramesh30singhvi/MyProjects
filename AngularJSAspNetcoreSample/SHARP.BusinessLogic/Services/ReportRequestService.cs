using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ScottPlot.Renderable;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Helpers;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Models;
using SHARP.DAL;
using SHARP.DAL.Models;
using SHARP.DAL.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class ReportRequestService : IReportRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly AppConfig _appConfig;
        private readonly IAuditService _auditService;

        public ReportRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IUserService userService,IAuditService auditService,
            AppConfig appConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _userService = userService;
            _appConfig = appConfig;
            _auditService = auditService;
        }

        public async Task<IReadOnlyCollection<ReportRequestDto>> GetReportRequestsAsync(ReportRequestFilter reportRequestFilter)
        {
            Expression<Func<ReportRequest, object>> orderBySelector =
                OrderByHelper.GetOrderBySelector<ReportRequestFilterColumn, Expression<Func<ReportRequest, object>>>(reportRequestFilter.OrderBy, GetColumnOrderSelector);

            reportRequestFilter.UserId = GetUserIdFilter();

            IReadOnlyCollection<ReportRequest> reportRequests = await _unitOfWork.ReportRequestRepository.GetListAsync(reportRequestFilter, orderBySelector);

            return _mapper.Map<IReadOnlyCollection<ReportRequestDto>>(reportRequests);
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetFilterColumnSourceDataAsync(ReportRequestFilterColumnSource<ReportRequestFilterColumn> columnData)
        {
            if (columnData.Column == ReportRequestFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            columnData.UserId = GetUserIdFilter();

            var columnSelector = GetColumnSelector(columnData.Column);

            IReadOnlyCollection<FilterOptionQueryModel> columnValues = await _unitOfWork.ReportRequestRepository.GetDistinctColumnAsync(columnData, columnSelector);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        public async Task<MessageResponse> AddReportRequestAsync(AddReportRequestDto addReportRequestDto)
        {
            var filter = _mapper.Map<PdfFilter>(addReportRequestDto);
            filter.UserId = _userService.GetLoggedUserIdIfUserIsOnlyAuditor();

            bool existsAudits = await _unitOfWork.AuditRepository.ExistsSubmittedAuditsAsync(filter);

            if (!existsAudits)
            {
                return new MessageResponse() { Status = MessageType.Info, Message = CommonConstants.NO_AUDITS_FILTER };
            }

            ReportRequest reportRequest = _mapper.Map<ReportRequest>(addReportRequestDto);

            string dbUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(CommonConstants.DB_USER_ID);

            reportRequest.UserId = int.Parse(dbUserId);
            reportRequest.RequestedTime = DateTime.UtcNow;
            reportRequest.Status = ReportRequestStatus.InProcces;

            await _unitOfWork.ReportRequestRepository.AddAsync(reportRequest);

            await _unitOfWork.SaveChangesAsync();

            await SendPdfFilterMessage(reportRequest);

           

            return new MessageResponse() { Status = MessageType.Success, Message = CommonConstants.REPORT_REQUEST_ADDED };
        }

        public async Task<MessageResponse> EditReportRequestAsync(EditReportRequestDto editReportRequestDto)
        {
            ReportRequest reportRequest = await _unitOfWork.ReportRequestRepository.GetAsync(editReportRequestDto.Id);

            _mapper.Map(editReportRequestDto, reportRequest);

            await _unitOfWork.SaveChangesAsync();
           // if (editReportRequestDto.Status == ReportRequestStatus.Success)
            {
                await AddReportToPortalReport(reportRequest);
            }
     
            return new MessageResponse() { Status = MessageType.Success };
        }

        private async Task AddReportToPortalReport(ReportRequest reportRequest)
        {
            try
            {
                var reports = _unitOfWork.PortalReportRepository.GetAll();
                if (reports.Any(x => x.ReportRequestId != null))
                {
                    var report = reports.FirstOrDefault(r => r.ReportRequestId != null && r.ReportRequestId == reportRequest.Id);
                    if (report != null)
                    {
                        return;
                    }
                }

                var request = await _unitOfWork.ReportRequestRepository.GetReportRequestAsync(reportRequest.Id);

                var portalReport = new PortalReport();
                portalReport.ReportRequestId = request.Id;
                portalReport.Name = $"{request.Form?.Name}";
                portalReport.ReportCategoryId = 1;
                portalReport.ReportTypeId = 1;
                //  portalReport.ReportRangeId = auditDto.Form.ReportRange == null ? 1 : auditDto.Form.ReportRange.Id;
                portalReport.CreatedAt = DateTime.UtcNow;
                portalReport.CreatedByUserID = request.UserId;
                portalReport.StorageReportName = null;
                portalReport.StorageURL = reportRequest.Report;
                portalReport.StorageContainerName = null;
                portalReport.OrganizationId = request.Organization.Id;
                portalReport.FacilityId = request.Facility.Id;
                portalReport.AuditTypeId = request.Form?.AuditTypeId;
                await _unitOfWork.PortalReportRepository.AddAsync(portalReport);

                await _unitOfWork.SaveChangesAsync();
            }catch(Exception){

            }

        
        }

        public async Task<string> SaveToBlobAsync(byte[] file)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["Blob"]);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["BlobContainerName"]);

            var fileName = Guid.NewGuid().ToString();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(new BinaryData(file), true);

            return fileName;
        }

        public async Task<byte[]> GetReportAsync(string report)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["Blob"]);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["BlobContainerName"]);

            BlobClient blobClient = containerClient.GetBlobClient(report);

            BlobDownloadResult file = await blobClient.DownloadContentAsync();
            
            return file.Content.ToArray();
        }

        public async Task ResendReportRequestAsync(int id)
        {
            ReportRequest reportRequest = await _unitOfWork.ReportRequestRepository.GetAsync(id);

            if(reportRequest == null)
            {
                throw new NotFoundException("Report Request is not found");
            }

            reportRequest.Exception = null;
            reportRequest.Status = ReportRequestStatus.InProcces;

            await _unitOfWork.SaveChangesAsync();

            await SendPdfFilterMessage(reportRequest);
        }

        private async Task SendPdfFilterMessage(ReportRequest reportRequest)
        {   
            ServiceBusClient client = new ServiceBusClient(_configuration["ServiceBus"]);
            ServiceBusSender sender = client.CreateSender(_configuration["ServiceBusQueue"]);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            var request = _mapper.Map<PdfRequest>(reportRequest);
            request.UserId = _userService.GetLoggedUserIdIfUserIsOnlyAuditor();

            string message = JsonConvert.SerializeObject(request);

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(message)))
            {
                reportRequest.Exception = $"The message: {message} can't be send.";
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
            }
            catch (Exception ex)
            {
                reportRequest.Exception += $"Send Message Error: {ex.Message}. InnerException: {ex.InnerException}";
            }
            finally
            {

                await sender.DisposeAsync();
                await client.DisposeAsync();

                if(!string.IsNullOrEmpty(reportRequest.Exception))
                {
                    await _unitOfWork.SaveChangesAsync();
                }
            }


        }
        private async Task LocalTest(ReportRequest reportRequest,string message)
        {
            var req = reportRequest;
            if (req != null)
            {
                var request = JsonConvert.DeserializeObject<PdfRequest>(message);

                byte[]? pdf = null;

                var filter = _mapper.Map<PdfFilter>(request);
                pdf = await _auditService.GetCriteriaPdfAsync(filter, null);


                string filename = await SaveToBlobAsync(pdf);

                if (request != null)
                {
                    await EditReportRequestAsync(new EditReportRequestDto() { Id = request.Id, GeneratedTime = DateTime.UtcNow, Report = filename, Status = ReportRequestStatus.Success });
                }
            

            }
        }
        private int? GetUserIdFilter()
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            if (!roles.Contains(UserRoles.Admin.ToString()))
            {
                return _userService.GetLoggedUserId();
            }

            return default(int?);
        }

        private Expression<Func<ReportRequest, FilterOptionQueryModel>> GetColumnSelector(ReportRequestFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<ReportRequestFilterColumn, Expression<Func<ReportRequest, FilterOptionQueryModel>>>
            {
                { ReportRequestFilterColumn.AuditType, i => new FilterOptionQueryModel { Value = i.AuditType } },
                { ReportRequestFilterColumn.OrganizationName, i => new FilterOptionQueryModel { Id = i.OrganizationId, Value = i.Organization.Name } },
                { ReportRequestFilterColumn.FacilityName, i => new FilterOptionQueryModel { Id = i.FacilityId, Value = i.Facility.Name } },
                { ReportRequestFilterColumn.FormName, i => new FilterOptionQueryModel { Id = i.FormId, Value = i.Form.Name } },                
                { ReportRequestFilterColumn.UserFullName, i => new FilterOptionQueryModel { Id = i.UserId, Value = i.User.FullName } },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        private Expression<Func<ReportRequest, object>> GetColumnOrderSelector(ReportRequestFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<ReportRequestFilterColumn, Expression<Func<ReportRequest, object>>>
            {
                { ReportRequestFilterColumn.AuditType, i => i.AuditType },
                { ReportRequestFilterColumn.OrganizationName, i => i.Organization.Name },
                { ReportRequestFilterColumn.FacilityName, i => i.Facility.Name },
                { ReportRequestFilterColumn.FormName, i => i.Form.Name },
                { ReportRequestFilterColumn.UserFullName, i => i.User.FullName },
                { ReportRequestFilterColumn.FromDate, i => i.FromDate },
                { ReportRequestFilterColumn.ToDate, i => i.ToDate },
                { ReportRequestFilterColumn.RequestedTime, i => i.RequestedTime },
                { ReportRequestFilterColumn.GeneratedTime, i => i.GeneratedTime },
                { ReportRequestFilterColumn.Status, i => i.Status },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }
    }
}
