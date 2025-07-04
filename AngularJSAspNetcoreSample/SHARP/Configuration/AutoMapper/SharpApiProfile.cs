using AutoMapper;
using SHARP.Authentication;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.AuditorProductivityDashboard;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.BusinessLogic.DTO.Portal;
using SHARP.BusinessLogic.DTO.ProgressNote;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.BusinessLogic.DTO.Role;
using SHARP.BusinessLogic.DTO.User;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Filtration.Enums;
using SHARP.Configuration.AutoMapper.Converters;
using SHARP.Configuration.AutoMapper.Resolvers;
using SHARP.Configuration.Resolvers;
using SHARP.DAL.Models;
using SHARP.DAL.Models.Views;
using SHARP.Extensions;
using SHARP.ViewModels;
using SHARP.ViewModels.Audit;
using SHARP.ViewModels.AuditorProductivityDashboard;
using SHARP.ViewModels.Base;
using SHARP.ViewModels.Common;
using SHARP.ViewModels.Dashboard;
using SHARP.ViewModels.Facilitty;
using SHARP.ViewModels.Facility;
using SHARP.ViewModels.Form;
using SHARP.ViewModels.Memo;
using SHARP.ViewModels.Organization;
using SHARP.ViewModels.Portal;
using SHARP.ViewModels.ProgressNote;
using SHARP.ViewModels.Report;
using SHARP.ViewModels.ReportRequest;
using SHARP.ViewModels.Role;
using SHARP.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.Configuration.AutoMapper
{
    public class SharpApiProfile : Profile
    {
        public SharpApiProfile()
        {
            MapCommon();
            MapUsers();
            MapReports();
            MapAudits();
            MapForm();
            MapFacilities();
            MapOrganizations();
            MapRoles();
            MapProgressNotes();
            MapFormField();
            MapDashboard();
            MapMemo();
            MapReportRequests();
            MapPortal();
            MapAuditorProductivityDashboard();
        }

        private void MapPortal()
        {
            CreateMap<HighAlertPortalFilterModel, HighAlertPortalFilter>()
                  .ForMember(dest => dest.Date, opt => opt.MapFrom<DateToUtcHighAlertDateFilterModelResolver>())
                                 .AfterMap((src, dest) => {
                                     dest.Date?.FirstCondition.SetDateCondition();
                                     dest.Date?.SecondCondition.SetDateCondition();

                                 });


            CreateMap<HighAlertValueDto, HighAlertGridItemModel>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Audit.Organization.Name))
            .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Audit.Facility.Name))
            .ForMember(dest => dest.FacilityId, opt => opt.MapFrom(src => src.Audit.Facility.Id))
            .ForMember(dest => dest.HighAlertCategoryPotentialAreas, opt => opt.MapFrom(src => string.Join(",",src.HighAlertCategoryPotentialAreas)))
            .ForMember(dest => dest.HighAlertCategoryName, opt => opt.MapFrom(src => src.HighAlertCategory.Name))
            .ForMember(dest => dest.HighAlertDescription, opt => opt.MapFrom(src => src.HighAlertDescription))
            .ForMember(dest => dest.Compliance, opt => opt.MapFrom(src => src.TotalCompliance))
            .ForMember(dest => dest.HighAlertNotes, opt => opt.MapFrom(src => src.HighAlertNotes))          
            .ForMember(dest => dest.HighAlertStatus, opt => opt.MapFrom(src => src.HighAlertStatus.Name))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom<HighAlertDateToLocalTimeResolver<HighAlertGridItemModel>>());

            CreateMap<HighAlertCategoryPotentialAreaDto, HighAlertCategoryViewModel>().
                ForMember(desc => desc.NameWithArea, opt => opt.MapFrom(src => src.Name + " : " + string.Join(",", src.HighAlertCategoryWithPotentialAreas)));
        }

        private void MapCommon()
        {
            CreateMap<string, DateFilterModel>().ConvertUsing<StringToModelConverter<DateFilterModel>>();
            CreateMap<string, NumberFilterModel>().ConvertUsing<StringToModelConverter<NumberFilterModel>>();
            CreateMap<string, AuditStatus>().ConvertUsing<StringToEnumConverter<AuditStatus>>();
            CreateMap<string, ReportAIStatus>().ConvertUsing<StringToEnumConverter<ReportAIStatus>>();
            CreateMap<string, AuditFilterColumn>().ConvertUsing<StringNameToEnumConverter<AuditFilterColumn>>();
            CreateMap<string, FormSettingType>().ConvertUsing<StringToEnumConverter<FormSettingType>>();
            CreateMap<string, DateTime?>().ConvertUsing<StringToDateTimeConverter>();
            CreateMap<string, TimeSpan?>().ConvertUsing<StringToTimeSpanConverter>();

            CreateMap<string, string>().ConvertUsing(str => string.IsNullOrEmpty(str) ? str : str.Trim());

            CreateMap<OptionDto, OptionModel>();
            CreateMap<KeywordOptionDto, KeywordOptionModel>();
            CreateMap<HighAlertValueDto, HighAlertValueModel>();
            CreateMap<KeywordDto, KeywordModel>();

         

            CreateMap<OptionModel, OptionFilter>();

            CreateMap<RearrangeItemsModel, RearrangeItemsDto>();

            CreateMap<RearrangeModel, RearrangeDto>();

            CreateMap<FilterOption, FilterOptionModel>()
                .ReverseMap();
            CreateMap<FacilityAccessModel, FacilityAccessDto>();
        }

        private void MapUsers()
        {
            CreateMap<UserOptionDto, UserOptionModel>();
            
            CreateMap<UserDto, UserModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(
                    dest => dest.Access,
                    opt => opt.MapFrom(src => new OrganizationAccess
                    {
                        Unlimited = src.Unlimited,
                        Organizations = src.Organizations
                    }))
                .ForMember(
                    dest => dest.FacilityAccess,
                    opt => opt.MapFrom(src => new FacilityAccess
                    {
                        Unlimited = src.FacilityUnlimited,
                        Facilities = src.Facilities
                    }
                ))
                ;

            CreateMap<UserFilterModel, UserFilter>();

            CreateMap<UserFilterColumnSourceModel, UserFilterColumnSource<UserColumn>>();

            CreateMap<FilterColumnSourceModel, FilterColumnSource<UserColumn>>();

            CreateMap<CreateUserModel, CreateUserDto>();

            CreateMap<EditUserModel, EditUserDto>();

            CreateMap<UserDetailsDto, UserDetailsModel>()
                .ForMember(
                    dest => dest.TimeZone,
                    opt => opt.MapFrom(
                        src => !string.IsNullOrEmpty(src.TimeZone) ? TimeZoneInfo.FindSystemTimeZoneById(src.TimeZone) : null));

            CreateMap<AddUserActivityModel, AddUserActivityDto>();

            CreateMap<UserOrganizationsDto, UserOrganizationsModel>();

            CreateMap<UserFaciltiesDto, UserFacilitiesModel>();

            CreateMap<TimeZoneInfo, TimeZoneModel>();
            CreateMap<TeamDto, TeamModel>();


            CreateMap<ClientPortalLoginModel,CreatePortalUserDto>();

            CreateMap<CreatePortalUserModel, CreatePortalUserDto>();


            CreateMap<PortalLoginsTrackingViewFilterModel, PortalLoginsTrackingViewFilter>()
                                      .ForMember(dest => dest.Date, opt => opt.MapFrom<DateToUtcLoginsTrackingFilterModelResolver>())
                                      .AfterMap((src, dest) =>
                                      {
                                          dest.Date?.FirstCondition.SetDateCondition();
                                          dest.Date?.SecondCondition.SetDateCondition();
                                      });

            CreateMap<LoginsTracking, LoginsTrackingDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<PortalDownloadsTrackingViewFilterModel, PortalDownloadsTrackingViewFilter>()
                                      .ForMember(dest => dest.Date, opt => opt.MapFrom<DateToUtcDownloadsTrackingFilterModelResolver>())
                                      .AfterMap((src, dest) =>
                                      {
                                          dest.Date?.FirstCondition.SetDateCondition();
                                          dest.Date?.SecondCondition.SetDateCondition();
                                      });

            CreateMap<DownloadsTracking, DownloadsTrackingDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PortalReport.Name))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.PortalReport.CreatedAt));

        }

        private void MapReports()
        {
            CreateMap<ReportDto, ReportModel>();
                
            CreateMap<ReportFilterModel, ReportFilter>();

            CreateMap<ReportFallModel, ReportFallFilterModel>();

            CreateMap<ReportCriteriaModel, ReportCriteriaFilter>();

            CreateMap<UploadFileModel,PdfReportUploadAzureDto>();

            CreateMap<UploadNewReportModal, UploadNewReportDto>();

            CreateMap<PortalReportDto,PortalReportViewModal>();

            CreateMap<PdfReportAnalysisDto, PdfReportAnalysisModel>();

            CreateMap<AIResultViewModel, AzureReportProcessResultDto>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => new OptionDto() { Id = src.Organization.Id, Name = src.Organization.Name }))
                .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => new OptionDto() { Id = src.Facility.Id, Name = src.Facility.Name }));


            CreateMap<PortalReportDto, PortalReportGridItemModel>()
                    .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                    .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name))
                    .ForMember(dest => dest.FacilityId, opt => opt.MapFrom(src => src.Facility.Id))
                    .ForMember(dest => dest.ReportName, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.ReportCategoryName, opt => opt.MapFrom(src => src.ReportCategory.Name))
                    .ForMember(dest => dest.ReportRangeName, opt => opt.MapFrom(src => src.ReportRange.Name))
                    .ForMember(dest => dest.Compliance, opt => opt.MapFrom(src => src.TotalCompliance))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.CreateByUser.FullName))
                    .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.AuditType.Id))
                    .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.ReportType.Name));

            CreateMap<PortalReportFilterModel, PortalReportFilter>()
                       .ForMember(dest => dest.Date, opt => opt.MapFrom<DateToUtcDateFilterModelResolver>())
                                      .AfterMap((src, dest) => {
                                          dest.Date?.FirstCondition.SetDateCondition();
                                          dest.Date?.SecondCondition.SetDateCondition();

                                      });
            CreateMap<PortalReportSelectedModel, SelectedDto>();
            CreateMap<PortalReportFacilityViewFilterModel, PortalReportFacilityViewFilter>()
                                       .ForMember(dest => dest.Date, opt => opt.MapFrom<DateToUtcFacilitytViewFilterModelResolver>())
                                      .AfterMap((src, dest) => {
                                          dest.Date?.FirstCondition.SetDateCondition();
                                          dest.Date?.SecondCondition.SetDateCondition();

                                      });


            CreateMap<ReportAIContentDto, ReportAIContentModel>()
                    .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                    .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name));


            CreateMap<AuditAIReportV2Dto, AIAuditReportV2ViewModel>()
                    .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                    .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name));


            CreateMap<AuditAIPatientPdfNotesViewModel, AuditAIPatientPdfNotesDto>()
               .ForMember(dest => dest.AuditAIReportV2Id, opt => opt.MapFrom(src => src.ReportId));

            CreateMap<AuditAIPatientPdfNotesDto, AuditAIPatientPdfNotesViewModel>()
                 .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.PatientId) ? "1" : src.PatientId))
                 .ForMember(dest => dest.ReportId, opt => opt.MapFrom(src => src.AuditAIReportV2Id));
               //  .ForMember(dest => dest.PatientNotes, opt => opt.ConvertUsing(new BinaryToStringConverter(), src => src));
            CreateMap<AuditAIKeywordSummaryDto, AuditAIKeywordSummaryViewModel>();
            CreateMap<AuditAIKeywordSummaryViewModel, AuditAIKeywordSummaryDto>();
            CreateMap<AIAuditReportV2ViewModel, AuditAIReportV2Dto>();
        }

        private void MapAudits()
        {
            CreateMap<AuditFilterModel, AuditFilter>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.FacilityName))
                .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.FormName))
                .ForMember(dest => dest.Auditor, opt => opt.MapFrom(src => src.AuditorName))
                .ForMember(dest => dest.AuditDate, opt => opt.MapFrom<AuditSubmittedDateToUtcDateFilterModelResolver>())
                .ForMember(dest => dest.LastDeletedDate, opt => opt.MapFrom<AuditLastDeletedDateToUtcDateFilterModelResolver>())
                .ForMember(dest => dest.DeletedByUser, opt => opt.MapFrom(src => src.DeletedByUser))
                 .AfterMap((src, dest) => {
                     dest.AuditDate?.FirstCondition.SetDateCondition();
                     dest.AuditDate?.SecondCondition.SetDateCondition();
                     dest.IncidentDateFrom?.FirstCondition.SetDateCondition();
                     dest.IncidentDateFrom?.SecondCondition.SetDateCondition();
                     dest.IncidentDateTo?.FirstCondition.SetDateCondition();
                     dest.IncidentDateTo?.SecondCondition.SetDateCondition();
                     dest.Compliance?.FirstCondition.SetNumberCondition();
                     dest.Compliance?.SecondCondition.SetNumberCondition();
                     dest.LastDeletedDate?.FirstCondition.SetDateCondition();
                     dest.LastDeletedDate?.SecondCondition.SetDateCondition();
                 });

            CreateMap<AuditFilterColumnSourceModel, AuditFilterColumnSource<AuditFilterColumn>>();

            CreateMap<AuditDto, AuditGridItemModel>()
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name))
                .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.Form.AuditType.Name))
                .ForMember(dest => dest.AuditDate, opt => opt.MapFrom<AuditSubmittedDateToLocalTimeResolver<AuditGridItemModel>>())
                .ForMember(dest => dest.AuditorUserId, opt => opt.MapFrom(src => src.SubmittedByUser.UserId))
                .ForMember(dest => dest.AuditorName, opt => opt.MapFrom(src => src.SubmittedByUser.FullName))
                .ForMember(dest => dest.Compliance, opt => opt.MapFrom(src => src.TotalCompliance))
                .ForMember(dest => dest.LastDeletedDate, opt => opt.MapFrom<AuditLastDeletedDateToLocalTimeResolver<AuditGridItemModel>>())
                .ForMember(dest => dest.DeletedByUser, opt => opt.MapFrom(src => src.DeletedByUser.FullName))
                .ForMember(dest => dest.SentForApprovalDate, opt => opt.MapFrom<AuditSentForApprovalDateToLocalTimeResolver<AuditGridItemModel>>())
                .ForMember(dest => dest.AuditCompletedDate, opt => opt.MapFrom<AuditCompletedDateToLocalTimeResolver<AuditGridItemModel>>());

            CreateMap<AuditDto, AuditModel>()
                .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom<AuditSubmittedDateToLocalTimeResolver<AuditModel>>());

            CreateMap<AuditDto, AuditModel>()
                .ForMember(dest => dest.LastDeletedDate, opt => opt.MapFrom<AuditLastDeletedDateToLocalTimeResolver<AuditModel>>());

            CreateMap<AuditAddEditModel, AuditAddEditDto>()
                .ForMember(dest => dest.CurrentClientDate, opt => opt.MapFrom<CurrentDateResolver<AuditAddEditModel, AuditAddEditDto>>());

            CreateMap<DuplicatedAuditAddEditModel, AuditAddEditDto>();

            CreateMap<CriteriaQuestionDto, CriteriaQuestionModel>()
                .ForMember(dest => dest.Answer, opt => opt.MapFrom(src => src.Answer ?? new CriteriaAnswerDto(src.Id)));

            CreateMap<AuditAddEditValueModel, AuditAddEditValueDto>();

            CreateMap<AuditAddEditSubHeaderValueModel, AuditAddEditSubHeaderValueDto>();

            CreateMap<CriteriaQuestionGroupDto, CriteriaQuestionGroupModel>();

            CreateMap<MdsSectionDto, MdsSectionModel>();

            CreateMap<MdsGroupDto, MdsGroupModel>();

            CreateMap<CriteriaAnswerGroupDto, CriteriaAnswerGroupModel>();

            CreateMap<CriteriaAnswerDto, CriteriaAnswerModel>();


            CreateMap<AddKeywordModel, AddKeywordDto>();

            CreateMap<EditKeywordModel, EditKeywordDto>()
                .IncludeBase<AddKeywordModel, AddKeywordDto>();

            CreateMap<AuditProgressNoteFilterModel, AuditProgressNoteFilter>()
                .ForMember(dest => dest.Keyword, opt => opt.MapFrom(src => new OptionModel() { Id = src.KeywordId, Name = src.KeywordName}))
                .ForMember(dest => dest.DateTo, opt => opt.MapFrom(src => src.DateTo.HasValue ? src.DateTo.Value.AddDays(1) : src.DateFrom.AddDays(1)));

            CreateMap<AddEditTrackerAnswerModel, AddEditTrackerAnswerDto>();

            CreateMap<TrackerAnswerDto, AuditTrackerAnswerModel>();

            CreateMap<TrackerAnswerGroupDto, AuditTrackerAnswerGroupModel>();

            CreateMap<PdfFilterModel, AddReportRequestDto>();

            CreateMap<PdfFilterModel, PdfFilter>();

            CreateMap<AuditDto, AuditDetailsModel>()
                .ForMember(dest => dest.Audit, opt => opt.MapFrom(src => src));

            CreateMap<HourKeywordAuditDetailsDto, HourKeywordAuditDetailsModel>()
                .IncludeBase<AuditDto, AuditDetailsModel>()
                .ForMember(dest => dest.MatchedKeywords, opt => opt.MapFrom(src => src.MatchedKeywords.OrderByDescending(keyword => keyword.Id)));
                   

            CreateMap<CriteriaAuditDetailsDto, CriteriaAuditDetailsModel>()
                .IncludeBase<AuditDto, AuditDetailsModel>();

            CreateMap<TrackerAuditDetailsDto, TrackerAuditDetailsModel>()
                .IncludeBase<AuditDto, AuditDetailsModel>();
            
            CreateMap<MdsAuditDetailsDto, MdsAuditDetailsModel>()
                .IncludeBase<AuditDto, AuditDetailsModel>();

            CreateMap<AuditKPIDto, AuditKPIModel>();

            CreateMap<CreateAIReportViewModel, CreateAIReportDto>();

            CreateMap<AuditAIReportV2Dto, AIAuditViewModel>()
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility.Name));
        }

        private void MapFacilities()
        {
            CreateMap<FacilityOptionDto, FacilityOptionModel>();
            CreateMap<FacilityFilterModel, FacilityFilter>()
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZoneName))
                /*.ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))*/;

            CreateMap<FacilityFilterColumnSourceModel, FacilityFilterColumnSource<FacilityFilterColumn>>();

            CreateMap<FacilityDto, FacilityGridItemModel>();
            CreateMap<TimeZoneOptionDto, TimeZoneOptionModel>();

            CreateMap<FacilityDetailsDto, FacilityDetailsModel>();

            CreateMap<FacilityRecipientDto, FacilityRecipientModel>()
                .ReverseMap();

            CreateMap<AddFacilityModel, AddFacilityDto>();

            CreateMap<AddRecipientsEmail, AddEmailRecipientsDto>();

            CreateMap<EditFacilityModel, EditFacilityDto>();

            CreateMap<FacilityOptionFilterModel, FacilityOptionFilter>();
        }

        private void MapOrganizations()
        {
            CreateMap<OrganizationDto, OrganizationModel>();
            CreateMap<OrganizationDto, OptionModel>();
            CreateMap<OrganizationPortalFeature, OrganizationPortalFeatureDto>()
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PortalFeature != null?  src.PortalFeature.Name : ""));
            CreateMap<OrganizationPortalFeatureDto, OrganizationPortalFeatureModel>();
            CreateMap<OrganizationDetailedDto, OrganizationDetailedModel>();
            CreateMap<OrganizationPortalFeatureModel, OrganizationPortalFeatureDto>();
            CreateMap<AddOrganizationModel, AddOrganizationDto>();
            CreateMap<EditOrganizationModel, EditOrganizationDto>();
            CreateMap<RecipientDto, RecipientModel>().ReverseMap();
        }

        private void MapForm()
        {
            CreateMap<FormOptionDto, FormOptionModel>();

            CreateMap<FormVersionDto, FormVersionGridItemModel>()
                .ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.Form.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.Organizations.Select(org => org.Name)))
                .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.Form.AuditType.Name))
                .ForMember(dest => dest.IsFormActive, opt => opt.MapFrom(src => src.Form.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom<FormManagementCreatedDateToLocalTimeResolver<FormVersionGridItemModel>>());

            CreateMap<FormVersionFilterModel, FormVersionFilter>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom<FormManagementCreatedDateToUtcDateFilterModelResolver>())
                .AfterMap((src, dest) => {
                    dest.CreatedDate?.FirstCondition.SetDateCondition();
                    dest.CreatedDate?.SecondCondition.SetDateCondition();
                });

            CreateMap<FormManagementFilterColumnSourceModel, FormManagementFilterColumnSource<FormVersionColumn>>();

            CreateMap<OrganizationFormFilterColumnSourceModel, OrganizationFormFilterColumnSource<OrganizationFormFilterColumn>>();

            CreateMap<FormOrganizationDto, FormGridItemModel>()
                .ForMember(dest => dest.FormOrganizationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.Form.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.Form.AuditType.Name))
                .ForMember(dest => dest.IsFormActive, opt => opt.MapFrom(src => src.Form.IsActive));

            CreateMap<FormVersionDto, FormVersionModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom<FormManagementCreatedDateToLocalTimeResolver<FormVersionModel>>());

            CreateMap<AddFormModel, AddFormDto>();

            CreateMap<AddFormKeywordModel, AddFormKeywordDto>();
            CreateMap<OptionModel,OptionDto>();
           
            CreateMap<EditFormKeywordModel, EditFormKeywordDto>();

            CreateMap<FormVersionDto, FormDetailsModel>()
               .ForMember(dest => dest.FormVersion, opt => opt.MapFrom(src => src));

            CreateMap<KeywordFormDetailsDto, KeywordFormDetailsModel>()
                .IncludeBase<FormVersionDto, FormDetailsModel>();

            CreateMap<CriteriaFormDetailsDto, CriteriaFormDetailsModel>()
                .IncludeBase<FormVersionDto, FormDetailsModel>();

            CreateMap<MdsFormDetailsDto, MdsFormDetailsModel>()
                .IncludeBase<FormVersionDto, FormDetailsModel>();

            CreateMap<TrackerFormDetailsDto, TrackerFormDetailsModel>()
                .IncludeBase<FormVersionDto, FormDetailsModel>();

            CreateMap<TrackerQuestionDto, TrackerQuestionModel>();

            CreateMap<FormFieldDto, FormFieldModel>();

            CreateMap<FormFieldItemDto, FormFieldItemModel>()
                .ReverseMap();

            CreateMap<CriteriaOptionDto, CriteriaOptionModel>();

            CreateMap<TrackerOptionDto, TrackerOptionModel>();

            CreateMap<RearrangeQuestionsModel, RearrangeQuestionsDto>();

            CreateMap<RearrangeQuestionModel, RearrangeQuestionDto>();

            CreateMap<AddCriteriaQuestionModel, GroupDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName));

            CreateMap<AddCriteriaQuestionModel, AddCriteriaQuestionDto>()
                .ForMember(dest => dest.Group, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.GroupName) ? src : null));

            CreateMap<AddMdsSectionModel, AddMdsSectionDto>();
            CreateMap<AddMdsGroupModel, AddMdsGroupDto>();
            
            CreateMap<EditMdsSectionModel, EditMdsSectionDto>();
            CreateMap<EditMdsGroupModel, EditMdsGroupDto>();

            CreateMap<EditCriteriaQuestionModel, EditCriteriaQuestionDto>()
                .IncludeBase<AddCriteriaQuestionModel, AddCriteriaQuestionDto>();

            CreateMap<EditQuestionGroupsModel, EditQuestionGroupsDto>();

            CreateMap<EditQuestionGroupModel, EditQuestionGroupDto>();

            CreateMap<AddTrackerQuestionModel, AddTrackerQuestionDto>();

            CreateMap<EditTrackerQuestionModel, EditTrackerQuestionDto>()
                .IncludeBase<AddTrackerQuestionModel, AddTrackerQuestionDto>();

            CreateMap<FormSettingModel, FormSettingDto>();

            CreateMap<ScheduleSettingDto, ScheduleSettingModel>()
                .ReverseMap();

            CreateMap<FormFilterModel, FormFilter>();

            CreateMap<FormOptionFilterModel, FormOptionFilter>();

            CreateMap<AuditAIReportFilterModel, AuditAIReportFilter>()
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom<ReportAIContentCreatedDateToUtcDateFilterModelResolver>())
               .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom<ReportAIContentSubmittedDateToUtcDateFilterModelResolver>())
               .AfterMap((src, dest) => {
                   dest.CreatedAt?.FirstCondition.SetDateCondition();
                   dest.CreatedAt?.SecondCondition.SetDateCondition();
                   dest.SubmittedDate?.FirstCondition.SetDateCondition();
                   dest.SubmittedDate?.SecondCondition.SetDateCondition();
                   dest.AuditDate?.FirstCondition.SetDateCondition();
                   dest.AuditDate?.SecondCondition.SetDateCondition();
               });

            CreateMap<AuditAIReportFilterColumnSourceModel, AuditAIReportFilterColumnSource<AuditAIReportFilterColumn>>();
        }

        private void MapFormField()
        {
            CreateMap<AddFormFieldModel, AddFormFieldDto>();

            CreateMap<EditFormFieldModel, EditFormFieldDto>();

            CreateMap<FormFieldValueDto, ControlOptionModel>();

            CreateMap<ControlOptionDto, ControlOptionModel>();
        }

        private void MapRoles()
        {
            CreateMap<RoleDto, RoleModel>();
        }

        private void MapProgressNotes()
        {
            CreateMap<ProgressNoteDto, ProgressNoteModel>();
        }

        private void MapDashboard()
        {
            CreateMap<DashboardFilterModel, DashboardFilter>()
                .ForMember(dest => dest.TimeFrame, opt => opt.MapFrom<DashboardTimeFrameToUtcDateFilterModelResolver>())
                .ForMember(dest => dest.CurrentClientDate, opt => opt.MapFrom<CurrentDateResolver<DashboardFilterModel, DashboardFilter>>())
                .AfterMap((src, dest) => {
                    dest.TimeFrame?.FirstCondition.SetDateCondition();
                    dest.TimeFrame?.SecondCondition.SetDateCondition();
                });

            CreateMap<AuditsDueDateCountDto, AuditsDueDateCountModel>();
        }

        private void MapMemo()
        {
            CreateMap<MemoFilterModel, MemoFilter>()
                .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom<CurrentDateResolver<MemoFilterModel, MemoFilter>>());

            CreateMap<MemoDto, MemoModel>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom<MemoCreatedDateToLocalTimeResolver>());

            CreateMap<AddMemoModel, AddMemoDto>();

            CreateMap<EditMemoModel, EditMemoDto>();
        }

        private void MapReportRequests()
        {
            CreateMap<ReportRequestFilterModel, ReportRequestFilter>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.FacilityName))
                .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.FormName))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.UserFullName))
                .ForMember(dest => dest.RequestedTime, opt => opt.MapFrom<ReportRequestRequestTimeToUtcDateFilterModelResolver>())
                .ForMember(dest => dest.GeneratedTime, opt => opt.MapFrom<ReportRequestGeneratedTimeToUtcDateFilterModelResolver>())
                .AfterMap((src, dest) => {
                    dest.FromDate?.FirstCondition.SetDateCondition();
                    dest.FromDate?.SecondCondition.SetDateCondition();
                    dest.ToDate?.FirstCondition.SetDateCondition();
                    dest.ToDate?.SecondCondition.SetDateCondition();
                    dest.RequestedTime?.FirstCondition.SetDateCondition();
                    dest.RequestedTime?.SecondCondition.SetDateCondition();
                    dest.GeneratedTime?.FirstCondition.SetDateCondition();
                    dest.GeneratedTime?.SecondCondition.SetDateCondition();
                });

            CreateMap<ReportRequestDto, ReportRequestGridModel>()
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                .ForMember(dest => dest.FacilityName, opt => opt.MapFrom(src => src.Facility != null ? src.Facility.Name : null))
                .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.RequestedTime, opt => opt.MapFrom<ReportRequestRequestTimeToLocalTimeResolver>())
                .ForMember(dest => dest.GeneratedTime, opt => opt.MapFrom<ReportRequestGeneratedTimeToLocalTimeResolver>());

            CreateMap<ReportRequestFilterColumnSourceModel, ReportRequestFilterColumnSource<ReportRequestFilterColumn>>();
        }

        private void MapAuditorProductivityDashboard()
        {
            CreateMap<AuditorProductivityInputView, AuditorProductivityInputDto>();

            CreateMap<AuditorProductivityInputFilterModel, AuditorProductivityInputFilter>()
               .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
               .ForMember(dest => dest.CompletionTime, opt => opt.MapFrom(src => src.CompletionTime))
               .AfterMap((src, dest) => {
                   dest.StartTime?.FirstCondition.SetDateCondition();
                   dest.StartTime?.SecondCondition.SetDateCondition();
                   dest.CompletionTime?.FirstCondition.SetDateCondition();
                   dest.CompletionTime?.SecondCondition.SetDateCondition();
               });

            CreateMap<AuditorProductivityInputFilterColumnSourceModel, AuditorProductivityInputFilterColumnSource<AuditorProductivityInputFilterColumn>>();


            CreateMap<AuditorProductivityAHTPerAuditTypeView, AuditorProductivityAHTPerAuditTypeDto>();

            CreateMap<AuditorProductivityAHTPerAuditTypeFilterModel, AuditorProductivityAHTPerAuditTypeFilter>()
               .ForMember(dest => dest.DateProcessed, opt => opt.MapFrom(src => src.DateProcessed))
               .AfterMap((src, dest) => {
                   dest.DateProcessed?.FirstCondition.SetDateCondition();
                   dest.DateProcessed?.SecondCondition.SetDateCondition();
               });

            CreateMap<AuditorProductivityAHTPerAuditTypeFilterColumnSourceModel, AuditorProductivityAHTPerAuditTypeFilterColumnSource<AuditorProductivityAHTPerAuditTypeFilterColumn>>();


            CreateMap<AuditorProductivitySummaryPerAuditorView, AuditorProductivitySummaryPerAuditorDto>();

            CreateMap<AuditorProductivitySummaryPerAuditorFilterModel, AuditorProductivitySummaryPerAuditorFilter>()
               .ForMember(dest => dest.DateProcessed, opt => opt.MapFrom(src => src.DateProcessed))
               .AfterMap((src, dest) => {
                   dest.DateProcessed?.FirstCondition.SetDateCondition();
                   dest.DateProcessed?.SecondCondition.SetDateCondition();
               });
            CreateMap<AuditorProductivitySummaryPerFacilityFilterModel, AuditorProductivitySummaryPerFacilityFilter>()
               .ForMember(dest => dest.DateProcessed, opt => opt.MapFrom(src => src.DateProcessed))
               .AfterMap((src, dest) => {
                   dest.DateProcessed?.FirstCondition.SetDateCondition();
                   dest.DateProcessed?.SecondCondition.SetDateCondition();
               });


            CreateMap<AuditorProductivitySummaryPerAuditorFilterColumnSourceModel, AuditorProductivitySummaryPerAuditorFilterColumnSource<AuditorProductivitySummaryPerAuditorFilterColumn>>();
        }
    }
}
