using AutoMapper;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Facility;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.BusinessLogic.DTO.Patient;
using SHARP.BusinessLogic.DTO.ProgressNote;
using SHARP.BusinessLogic.DTO.Role;
using SHARP.BusinessLogic.DTO.User;
using SHARP.DAL.Models;
using SHARP.DAL.Extensions;
using System.Collections.Generic;
using System.Linq;
using SHARP.BusinessLogic.DTO.Common;
using SHARP.BusinessLogic.Configuration.AutoMapper.Converters;
using SHARP.Common.Enums;
using SHARP.Common.Constants;
using SHARP.DAL.Models.QueryModels;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.BusinessLogic.DTO.ReportRequest;
using SHARP.Common.Filtration;
using System;
using SHARP.BusinessLogic.DTO.UserActivity;
using SHARP.BusinessLogic.DTO.Portal;

namespace SHARP.BusinessLogic.Configuration.AutoMapper
{
    public class SharpBLLProfile: Profile
    {
        public SharpBLLProfile()
        {
            MapCommon();
            MapUsers();
            MapReports();
            MapAudits();
            MapFacilities();
            MapOrganizations();
            MapForms();
            MapPatients();
            MapRoles();
            MapTableColumn();
            MapTableColumnGroup();
            MapTableColumnValue();
            MapProgressNotes();
            MapFormField();
            MapMemo();
            MapReportRequest();
            MapDashboardInput();
            MapHighAlert();
        }

        private void MapCommon()
        {
            CreateMap<FilterOptionQueryModel, FilterOption>();

            CreateMap<FilterQueryModel, FilterOption>();
        }

        private void MapHighAlert()
        {
            CreateMap<HighAlertCategory, OptionDto>();
            CreateMap<HighAlertStatus, OptionDto>();
            CreateMap<HighAlertPotentialAreas, OptionDto>();
            CreateMap<HighAlertCategory, HighAlertCategoryPotentialAreaDto>()
                .ForMember(desc => desc.HighAlertCategoryWithPotentialAreas, opt => opt.MapFrom(src => src.HighAlertCategoryToPotentialAreas.Select(x => x.HighAlertPotentialAreas.Name).ToArray() ));

        }
        private void MapReports()
        {
            CreateMap<Report, ReportDto>();

            CreateMap<PortalReport, PortalReportDto>()
                 .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => new OptionDto() { Id = src.Organization.Id, Name = src.Organization.Name }))
                 .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => new OptionDto() { Id = src.Facility.Id, Name = src.Facility.Name }))
                 .ForMember(dest => dest.ReportCategory, opt => opt.MapFrom(src => new OptionDto() { Id = src.ReportCategory.Id, Name = src.ReportCategory.ReportCategoryName }))
                 .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => new OptionDto() { Id = src.ReportType.Id, Name = src.ReportType.TypeName }))
                 .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.TotalCompliance, opt =>  opt.MapFrom(src => src.Audit == null ? 0 : src.Audit.TotalCompliance))
                 .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.AuditType == null ? new OptionDto() : new OptionDto() { Id = src.AuditType.Id, Name = src.AuditType.Name }))
                 .ForMember(dest => dest.CreateByUser, opt => opt.MapFrom(src => new UserOptionDto() { Id = src.User.Id, FirstName = src.User.FirstName, LastName = src.User.LastName, FullName = src.User.FullName, UserId = src.User.UserId }));

            CreateMap<ReportType, OptionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TypeName));

            CreateMap<ReportRange, OptionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RangeName));
            CreateMap<ReportCategory, OptionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ReportCategoryName));

            CreateMap<AuditAIReport, ReportAIContentDto>()
                .ForMember(dest => dest.SummaryAI, opt => opt.ConvertUsing(new StringFromBinaryConverter(), src => src))
               .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization))
               .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.Facility));

            CreateMap<AuditAIReport, AuditAIReportNoSummaryDto>()                           
               .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization))
               .ForMember(dest => dest.Facility, opt => opt.MapFrom(src => src.Facility));

            CreateMap<PortalReportFacilityViewFilter, PortalReportFilter>();

            CreateMap<AuditAIPatientPdfNotesDto, AuditAIPatientPdfNotes>();
            

            CreateMap<AuditAIPatientPdfNotes, AuditAIPatientPdfNotesDto>();

            CreateMap<AuditAIKeywordSummary, AuditAIKeywordSummaryDto>();
            CreateMap<AuditAIKeywordSummaryDto, AuditAIKeywordSummary>();

            CreateMap<AuditAIReportV2,AuditAIReportV2Dto>();
            CreateMap<AuditAIReportV2Dto, AuditAIReportV2>();

        }
        private void MapUsers()
        {
            CreateMap<User, UserOptionDto>();
            CreateMap<User, OptionDto>();

            CreateMap<User, UserDto>()
                // user without access to any organization = user with access to any organization
                .ForMember(dest => dest.Unlimited, opt => opt.MapFrom(src => !src.UserOrganizations.Any()))
                .ForMember(dest => dest.FacilityUnlimited, opt => opt.MapFrom(src => !src.UserFacilities.Any()))
                .ForMember(
                    dest => dest.Organizations,
                    opt => opt.MapFrom(
                        src => src.UserOrganizations.Select(userOrganization => userOrganization.Organization.Name)))
                .ForMember(
                    dest => dest.Facilities,
                    opt => opt.MapFrom(
                        src => src.UserFacilities.Select(userFacility => userFacility.Facility.Name)));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(
                    dest => dest.UserOrganizations,
                    opt => opt.MapFrom(src => src.Unlimited
                    ? Enumerable.Empty<UserOrganization>()
                    : src.Organizations.Select(organization => new UserOrganization
                    {
                        OrganizationId = organization
                    })))
                .ForMember(
                    dest => dest.UserFacilities,
                    opt => opt.MapFrom(src => src.FacilityUnlimited
                    ? Enumerable.Empty<UserFacility>()
                    : src.Facilities.Select(facility => new UserFacility
                    {
                        FacilityId = facility
                    })));

            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => src.Roles.Select(role => new ApplicationUserRole { RoleId = role })));

            CreateMap<EditUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(
                    dest => dest.UserOrganizations,
                    opt => opt.MapFrom(src => src.Unlimited
                    ? Enumerable.Empty<UserOrganization>()
                    : src.Organizations.Select(organization => new UserOrganization
                    {
                        OrganizationId = organization
                    })))
                 .ForMember(
                    dest => dest.UserTeams,
                    opt => opt.MapFrom(src => src.Teams == null  && !src.Teams.Any()
                    ? Enumerable.Empty<UserTeam>()
                    : src.Teams.Select(team => new UserTeam
                    {
                        TeamId = team
                    })))
                .ForMember(
                    dest => dest.UserFacilities,
                    opt => opt.MapFrom(src => src.FacilityUnlimited
                    ? Enumerable.Empty<UserFacility>()
                    : src.Facilities.Select(facility => new UserFacility
                    {
                        FacilityId = facility
                    })));

            CreateMap<EditUserDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => src.Roles.Select(role => new ApplicationUserRole { RoleId = role })));

            CreateMap<User, UserDetailsDto>()
                .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(
                        src => src.IdentityUser.UserRoles.Select(userRole => userRole.Role)))
                .ForMember(
                    dest => dest.Organizations,
                    opt => opt.MapFrom(
                        src => src.UserOrganizations.Select(userOrganization => userOrganization.Organization)))
                .ForMember(
                    dest => dest.Teams,
                    opt => opt.MapFrom(
                        src => src.UserTeams.Select(userTeam => userTeam.Team)))
                .ForMember(
                    dest => dest.Facilities,
                    opt => opt.MapFrom(
                        src => src.UserFacilities.Select(userFacility => userFacility.Facility)));


            CreateMap<AddUserActivityDto, UserActivity>();

            CreateMap<Team, TeamDto>();
            CreateMap<Team, OptionDto>();
            CreateMap<UserOrganization, UserOptionDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));
        }

        private void MapAudits()
        {
            CreateMap<Audit, AuditDto>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Facility.Organization))
                .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.FormVersion))
                .ForMember(dest => dest.Resident, opt => opt.MapFrom(src => src.ResidentName))
                .ForMember(dest => dest.IsReadyForNextStatus, opt => opt.MapFrom(src => src.IsFilled.HasValue ? src.IsFilled : true))
                .ForMember(dest => dest.HighAlertCategory, opt => opt.MapFrom(src => src.HighAlertAuditValues != null && src.HighAlertAuditValues.Any() && src.HighAlertAuditValues.FirstOrDefault().HighAlertCategory != null ? new OptionDto() { Id = src.HighAlertAuditValues.FirstOrDefault().HighAlertCategory.Id, Name = src.HighAlertAuditValues.FirstOrDefault().HighAlertCategory.Name } : null))
                .ForMember(dest => dest.HighAlertDescription, opt => opt.MapFrom(src => src.HighAlertAuditValues != null && src.HighAlertAuditValues.Any() ? src.HighAlertAuditValues.FirstOrDefault().HighAlertDescription : string.Empty))
                .ForMember(dest => dest.HighAlertNotes, opt => opt.MapFrom(src => src.HighAlertAuditValues != null && src.HighAlertAuditValues.Any() ? src.HighAlertAuditValues.FirstOrDefault().HighAlertNotes : string.Empty))
                .ForMember(dest => dest.SentForApprovalDate, opt => opt.MapFrom(src => src.AuditStatusHistory != null && src.AuditStatusHistory.FirstOrDefault(w => w.Status == AuditStatus.WaitingForApproval) != null ? (DateTime?)src.AuditStatusHistory.FirstOrDefault(w => w.Status == AuditStatus.WaitingForApproval).Date : (DateTime?)null))
                .ForMember(dest => dest.AuditCompletedDate, opt => opt.MapFrom(src => src.AuditStatusHistory != null && src.AuditStatusHistory.FirstOrDefault(w => w.Status == AuditStatus.Submitted) != null ? (DateTime?)src.AuditStatusHistory.FirstOrDefault(w => w.Status == AuditStatus.Submitted).Date : (DateTime?)null))
                .ForMember(dest => dest.ReportTypeId, opt => opt.MapFrom(src => src.PortalReport != null ? src.PortalReport.ReportTypeId : 0));
 


            CreateMap<AuditType, OptionDto>();

            CreateMap<AuditAddEditDto, Audit>()
                .ForMember(dest => dest.ResidentName, opt => opt.MapFrom(src => src.Resident))
                .ForMember(dest => dest.Values,
                    opt => opt.MapFrom((src, dest, _, context) =>
                        dest.Values.Actualize(src.Values, model => context.Mapper.Map<AuditTableColumnValue>(model), (model, entity) => context.Mapper.Map(model, entity))))
                .ForMember(dest => dest.AuditFieldValues,
                    opt => opt.MapFrom((src, dest, _, context) =>
                        dest.AuditFieldValues.Actualize(src.SubHeaderValues, model => context.Mapper.Map<AuditFieldValue>(model), (model, entity) => context.Mapper.Map(model, entity))));

            CreateMap<Audit, CriteriaAuditDetailsDto>()
                .IncludeBase<Audit, AuditDto>()
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Values))
                .ForMember(dest => dest.SubHeaderValues, opt => opt.MapFrom(src => src.AuditFieldValues));

            CreateMap<Audit, HourKeywordAuditDetailsDto>()
                .IncludeBase<Audit, AuditDto>()
                .ForMember(dest => dest.MatchedKeywords, opt => opt.MapFrom(src => src.Values));

            CreateMap<Audit, TrackerAuditDetailsDto>()
               .IncludeBase<Audit, AuditDto>()
               .ForMember(dest => dest.SortModel, opt => opt.MapFrom(src => MapSortModel(src)))
               .ForMember(dest => dest.PivotAnswerGroups, opt => opt.MapFrom((src, dest, _, context) => MapTrackerGroupAnswers(context, src.Values, dest.SortModel)));

            CreateMap<Audit, MdsAuditDetailsDto>()
                .IncludeBase<Audit, AuditDto>()
                .ForMember(dest => dest.SubHeaderValues, opt => opt.MapFrom(src => src.AuditFieldValues));

            CreateMap<AuditKPIQueryModel, AuditKPIDto>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => new OptionDto() { Id = src.OrganizationId, Name = src.OrganizationName}));

            CreateMap<HighAlertAuditValue, HighAlertValueDto>()
                 .ForMember(dest => dest.HighAlertCategory, opt => opt.MapFrom(src => src.HighAlertCategory != null ?  new OptionDto() { Id = src.HighAlertCategory.Id, Name = src.HighAlertCategory.Name } : new OptionDto() ))
                 .ForMember(dest => dest.HighAlertCategoryPotentialAreas, opt =>   opt.MapFrom(src => src.HighAlertCategory != null && src.HighAlertCategory.HighAlertCategoryToPotentialAreas != null ?  src.HighAlertCategory.HighAlertCategoryToPotentialAreas.Select(x =>x.HighAlertPotentialAreas.Name).ToArray() : null ))
                 .ForMember(dest => dest.HighAlertStatus, opt => opt.MapFrom(src => src.HighAlertStatusHistory != null && src.HighAlertStatusHistory.Any() ? new OptionDto() { Id = src.HighAlertStatusHistory.Last().HighAlertStatus.Id, Name = src.HighAlertStatusHistory.Last().HighAlertStatus.Name } : new OptionDto()))
                  .ForMember(dest => dest.TotalCompliance, opt => opt.MapFrom(src => src.Audit == null ? 0 : src.Audit.TotalCompliance))
                 .ForMember(dest => dest.ReportTypeId, opt => opt.MapFrom(src => src.Audit != null && src.Audit.PortalReport != null ? src.Audit.PortalReport.ReportTypeId : 0))
                 .ForMember(dest => dest.UserNotes, opt => opt.MapFrom(src => src.HighAlertStatusHistory != null && src.HighAlertStatusHistory.Any() ? src.HighAlertStatusHistory.Last().UserNotes : "" ))
                 .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.HighAlertStatusHistory != null && src.HighAlertStatusHistory.Any() ? src.HighAlertStatusHistory.Last().ChangedBy : "" ));

        }

        private void MapFacilities()
        {
            CreateMap<Facility, OptionDto>();

            CreateMap<Facility, FacilityOptionDto>()
                .ForMember(dest => dest.TimeZoneOffset, opt => opt.ConvertUsing(new TimeZoneOffsetConverter(), src => src.TimeZone))
                .ForMember(dest => dest.TimeZoneShortName, opt => opt.MapFrom(src => src.TimeZone.ShortName));

            CreateMap<FacilityTimeZone, TimeZoneOptionDto>()
                .ForMember(dest => dest.TimeZoneOffset, opt => opt.ConvertUsing(new TimeZoneOffsetConverter(), src => src))
                .ForMember(dest => dest.Name, opt => opt.ConvertUsing(new TimeZoneDisplayNameConverter(), src => src))
                .ForMember(dest => dest.OriginalTimeZoneName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TimeZoneShortName, opt => opt.MapFrom(src => src.ShortName));

            CreateMap<Facility, FacilityDto>()
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
                .ForMember(dest => dest.TimeZoneName, opt => opt.ConvertUsing(new TimeZoneDisplayNameConverter(), src => src.TimeZone))
                .ForMember(dest => dest.OriginalTimeZoneName, opt => opt.MapFrom(src => src.TimeZone.Name))
                .ForMember(dest => dest.RecipientsCount, opt => opt.MapFrom(src => src.Recipients.Count));
                
            CreateMap<Facility, FacilityDetailsDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Active))
                .ForMember(dest => dest.PortalFeatures,opt => opt.MapFrom(src => src.Organization.PortalFeatures))
                .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => src.Recipients.OrderBy(recipient => recipient.Email)));

            CreateMap<AddFacilityDto, Facility>()
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.IsActive));
            CreateMap<FacilityOptionDto, FilterOption>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Name));




            CreateMap<EditFacilityDto, Facility>()
              .IncludeBase<AddFacilityDto, Facility>();

            CreateMap<FacilityRecipientDto, FacilityRecipient>()
                .ReverseMap();
        }

        private void MapOrganizations()
        {
            CreateMap<Organization, OptionDto>();
            CreateMap<PortalFeature,OptionDto>();
            CreateMap<Organization, OrganizationDto>();
            CreateMap<Organization, OrganizationDetailedDto>();
            CreateMap<AddOrganizationDto, Organization>()
                .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => src.Recipients.Select(recipient => new OrganizationRecipient
                {
                    Recipient = recipient
                })));
        }

        private void MapDashboardInput()
        {
            CreateMap<DashboardInputValues, DashboardInputValuesDto>();
            CreateMap<DashboardInputElement, DashboardInputElementsDto>();
            CreateMap<DashboardInputGroups, DashboardInputGroupsDto>();
            CreateMap<DashboardInputTable, DashboardInputTableDto>();
            CreateMap<Organization, DashboardInputDto>();
            CreateMap<SaveDashboardInputValuesDto, DashboardInputValues>();
            CreateMap<AddTableDto, DashboardInputTable>();
            CreateMap<AddGroupDto, DashboardInputGroups>();
            CreateMap<AddElementDto, DashboardInputElement>();
        }

        private void MapForms()
        {
            CreateMap<FormVersion, FormVersionDto>()
                .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Form.FormOrganizations.First().Organization))
                .ForMember(
                    dest => dest.Organizations,
                    opt => opt.MapFrom(
                        src => src.Form.FormOrganizations.Select(formOrganization => formOrganization.Organization)))
                /*.ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.Form.AuditType))*/;

            CreateMap<FormVersion, FormOptionDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Form.Name))
                .ForMember(dest => dest.AuditType, opt => opt.MapFrom(src => src.Form.AuditType))
                .ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.Form.Id))
                .ForMember(dest => dest.AllowEmptyComment, opt => opt.MapFrom(src => src.Form.AllowEmptyComment))
                .ForMember(dest => dest.DisableCompliance, opt => opt.MapFrom(src => src.Form.DisableCompliance))
                .ForMember(dest => dest.UseHighAlert, opt => opt.MapFrom(src => src.Form.UseHighAlert))
                .ForMember(dest => dest.AHTime, opt => opt.MapFrom(src => src.Form.AHTime))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Form.IsActive));

            CreateMap<Form, FormOptionDto>();

            CreateMap<Form, OptionDto>();
   
            CreateMap<FormVersion, KeywordFormDetailsDto>()
                .IncludeBase<FormVersion, FormVersionDto>()
                .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Columns.OrderBy(keyword => keyword.Name)));
              

            CreateMap<FormVersion, CriteriaFormDetailsDto>()
               .IncludeBase<FormVersion, FormVersionDto>()
               .ForMember(dest => dest.QuestionGroups, opt => opt.MapFrom((src, dest, _, context) => src.Columns
                    .GroupBy(column => column.Group)
                    .Select(group => new CriteriaQuestionGroupDto
                    {
                        Id = group.Key?.Id,
                        Name = group.Key?.Name,
                        Sequence = group.Key?.Sequence,
                        Questions = context.Mapper.Map<IReadOnlyCollection<CriteriaQuestionDto>>(group
                            .Where(question => !question.ParentId.HasValue)
                            .OrderBy(question => question.Sequence))
                    })
                    .Concat(context.Mapper.Map<IEnumerable<CriteriaQuestionGroupDto>>(src.Groups?.Where(group => group.TableColumns == null || !group.TableColumns.Any()).OrderBy(group => group.Sequence)))
                    .OrderBy(group => group.Sequence)
                    .ToList()
                    ))
               .ForMember(dest => dest.FormFields, opt => opt.MapFrom(src => src.FormFields.OrderBy(field => field.Sequence)));

            CreateMap<FormVersion, TrackerFormDetailsDto>()
               .IncludeBase<FormVersion, FormVersionDto>()
               .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Columns.OrderBy(question => question.Sequence)));

            CreateMap<FormVersion, MdsFormDetailsDto>()
                .IncludeBase<FormVersion, FormVersionDto>()
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections.OrderBy(field => field.Sequence)))
                .ForMember(dest => dest.FormFields, opt => opt.MapFrom(src => src.FormFields.OrderBy(field => field.Sequence)));


            CreateMap<FormField, FormFieldDto>();

            CreateMap<FormSection, MdsSectionDto>()
                .ForMember(section => section.Groups, group => group.MapFrom(src => src.Groups.OrderBy(group => group.Sequence)));

            CreateMap<FormGroup, MdsGroupDto>()
                .ForMember(group => group.FormFields, formField => formField.MapFrom(src => src.FormFields.OrderBy(formField => formField.Sequence)));

       
            CreateMap<AddFormDto, Form>();

            CreateMap<CriteriaOption, CriteriaOptionDto>()
                .ReverseMap();

            CreateMap<TrackerOption, TrackerOptionDto>()
                .ReverseMap();

            CreateMap<Form, FormDto>();

            CreateMap<FormOrganization, FormOrganizationDto>()
                .ForMember(dest => dest.ScheduleSetting, opt => opt.ConvertUsing(new FormSettingFromJsonConverter(), src => src));

            CreateMap<FormSettingDto, FormOrganization>()
                .ForMember(dest => dest.ScheduleSetting, opt => opt.ConvertUsing(new FormSettingToJsonConverter(), src => src));
        }

        private void MapPatients()
        {
            CreateMap<Patient, PatientDto>();
        }

        private void MapRoles()
        {
            CreateMap<ApplicationRole, RoleDto>();

            CreateMap<ApplicationRole, OptionDto>();
        }

        private void MapTableColumn()
        {

            CreateMap<TableColumn, KeywordOptionDto>()
                .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.KeywordTrigger != null && src.KeywordTrigger.Any()))
                .ForMember(dest => dest.FormsTriggeredByKeyword, opt => opt.MapFrom((src, dest, _, context) => src.KeywordTrigger
                    .Select(x => new OptionDto
                    {
                        Id = x.FormTriggerId,
                        Name = x.FormTriggeredByKeyword?.Name,

                    }).ToList()));
    
            CreateMap<TableColumnGroup, OptionDto>();

            CreateMap<TableColumn, CriteriaQuestionDto>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SubQuestions, opt => opt.MapFrom(src => src.SubQuestions.OrderBy(subQuestion => subQuestion.Sequence)))
                .ForMember(dest => dest.CriteriaOption, opt => opt.MapFrom(src => src.CriteriaOption == null ? new CriteriaOption() : src.CriteriaOption));

            CreateMap<TableColumn, TrackerQuestionDto>()
                .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TrackerOption, opt => opt.MapFrom(src => src.TrackerOption == null ? new TrackerOption() : src.TrackerOption));

            CreateMap<AddFormKeywordDto, TableColumn>();

            CreateMap<EditFormKeywordDto, TableColumn>();

            CreateMap<RearrangeQuestionDto, TableColumn>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.LegacyFormColumnId, opt => opt.Ignore())
                .ForMember(dest => dest.LegacyFormRowId, opt => opt.Ignore())
                .ForMember(dest => dest.LegacyRowId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore());

            CreateMap<AddCriteriaQuestionDto, TableColumn>()
                .ForMember(dest => dest.Group, opt => opt.MapFrom((src, dest, _, context) => 
                src.Group != null && src.Group.Id == null && src.Group.Name != null  
                ? new TableColumnGroup() { Name = src.Group.Name, FormVersionId = src.Group.FormVersionId }
                : dest.Group?.Id == src.Group?.Id ? context.Mapper.Map(src.Group, dest.Group) : null))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Group != null ? src.Group.Id : default(int?)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Question))
                .ForMember(dest => dest.CriteriaOption, opt => opt.MapFrom(src => src));

            CreateMap<AddCriteriaQuestionDto, CriteriaOption>();

            CreateMap<EditCriteriaQuestionDto, CriteriaOption>();

            CreateMap<EditCriteriaQuestionDto, TableColumn>()
                .IncludeBase<AddCriteriaQuestionDto, TableColumn>()
                .ForMember(dest => dest.Sequence, opt => opt.Ignore());

            CreateMap<AddTrackerQuestionDto, TableColumn>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Question))
               .ForMember(dest => dest.TrackerOption, opt => opt.MapFrom(src => src));

            CreateMap<AddTrackerQuestionDto, TrackerOption>();

            CreateMap<EditTrackerQuestionDto, TrackerOption>()
                .ForMember(dest => dest.FieldType, opt => opt.Ignore());

            CreateMap<EditTrackerQuestionDto, TableColumn>()
                .IncludeBase<AddTrackerQuestionDto, TableColumn>()
                .ForMember(dest => dest.Sequence, opt => opt.Ignore());
        }

        private void MapTableColumnGroup()
        {
            CreateMap<GroupDto, TableColumnGroup>();

            CreateMap<TableColumnGroup, CriteriaQuestionGroupDto>();

            CreateMap<EditQuestionGroupDto, TableColumnGroup>()
                .ForMember(dest => dest.FormVersionId, opt => opt.Ignore())
                .ForMember(dest => dest.TableColumns, opt => opt.Ignore());
        }

        private void MapTableColumnValue()
        {
            CreateMap<AuditTableColumnValue, KeywordDto>()
                .ForMember(dest => dest.Keyword, opt => opt.MapFrom(src => src.Column))
                .ForMember(dest => dest.ProgressNoteDateTime, opt => opt.MapFrom(src => src.ProgressNoteTime.HasValue
                        ? src.ProgressNoteDate.Value.Add(src.ProgressNoteTime.Value).ToString(DateTimeConstants.MM_DD_YYYY_HH_MM_SLASH)
                        : src.ProgressNoteDate.Value.ToString(DateTimeConstants.MM_DD_YYYY_SLASH)));


            CreateMap<AddKeywordDto, AuditTableColumnValue>()
                .ForMember(dest => dest.TableColumnId, opt => opt.MapFrom(src => src.KeywordId));

            CreateMap<EditKeywordDto, AuditTableColumnValue>()
                .IncludeBase<AddKeywordDto, AuditTableColumnValue>();

            CreateMap<AuditAddEditValueDto, AuditTableColumnValue>();

            CreateMap<AuditTableColumnValue, CriteriaAnswerDto>();

            CreateMap<AddEditTrackerAnswerDto, AuditTableColumnValue>()
                .ForMember(dest => dest.TableColumnId, opt => opt.MapFrom(src => src.QuestionId))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Answer));

            CreateMap<AuditTableColumnValue, TrackerAnswerDto>()
                .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.TableColumnId))
                .ForMember(dest => dest.Answer, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.HighAlertAuditValue, opt => opt.MapFrom(src => src.HighAlertAuditValue))
                .ForMember(dest => dest.FieldType, opt => opt.MapFrom(src => src.Column.TrackerOption.FieldTypeId))
                .ForMember(dest => dest.FormattedAnswer, opt => opt.ConvertUsing(new JsonValueToStringConverter(), src =>
                new FieldValueDto()
                {
                    FieldType = src.Column.TrackerOption != null ? (FieldTypes)src.Column.TrackerOption.FieldTypeId : default(FieldTypes?),
                    Value = src.Value
                }));
        }

        private void MapProgressNotes()
        {
            CreateMap<ProgressNote, ProgressNoteDto>()
                .ForMember(dest => dest.Resident, opt => opt.MapFrom(src => src.Patient.FullName));
        }

        private void MapFormField()
        {
            CreateMap<FieldType, OptionDto>();

            CreateMap<AddFormFieldDto, FormField>();

            CreateMap<EditFormFieldDto, FormField>()
                .IncludeBase<AddFormFieldDto, FormField>()
                .ForMember(dest => dest.Sequence, opt => opt.Ignore());

            CreateMap<FormFieldItem, FormFieldItemDto>()
                .ReverseMap();

            CreateMap<TableColumnItem, FormFieldItemDto>();

            CreateMap<FormFieldItemDto, TableColumnItem>()
                .ForMember(dest => dest.TableColumnId, opt => opt.Ignore());

            CreateMap<RearrangeDto, FormField>()
                .ForMember(dest => dest.FormVersionId, opt => opt.Ignore())
                .ForMember(dest => dest.FieldName, opt => opt.Ignore())
                .ForMember(dest => dest.LabelName, opt => opt.Ignore())
                .ForMember(dest => dest.FieldType, opt => opt.Ignore())
                .ForMember(dest => dest.IsRequired, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            CreateMap<AuditAddEditSubHeaderValueDto, AuditFieldValue>();

            CreateMap<AuditFieldValue, FormFieldValueDto>()
                .ForMember(dest => dest.FormattedValue, opt => opt.ConvertUsing(new JsonValueToStringConverter(), src =>
                new FieldValueDto()
                {
                    FieldType = src.FormField != null ? (FieldTypes)src.FormField.FieldTypeId : default(FieldTypes?),
                    Value = src.Value
                }));
        }

        public void MapMemo()
        {
            CreateMap<Memo, MemoDto>()
                .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.OrganizationMemos.Select(orgMemo => orgMemo.Organization)));

            CreateMap<AddMemoDto, Memo>()
                .ForMember(dest => dest.OrganizationMemos, opt => opt.MapFrom(src => src.OrganizationIds.Select(orgId => 
                new OrganizationMemo
                { 
                    OrganizationId = orgId 
                })));

            CreateMap<EditMemoDto, Memo>()
                .ForMember(dest => dest.UserId, src => src.Ignore())
                .ForMember(dest => dest.CreatedDate, src => src.Ignore())
                .ForMember(dest => dest.OrganizationMemos, opt => opt.MapFrom(src => src.OrganizationIds.Select(orgId =>
                new OrganizationMemo
                {
                    OrganizationId = orgId,
                    MemoId = src.Id
                })));
        }

        private void MapReportRequest()
        {
            CreateMap<AddReportRequestDto, ReportRequest>();

            CreateMap<EditReportRequestDto, ReportRequest>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FacilityId, opt => opt.Ignore())
                .ForMember(dest => dest.FormId, opt => opt.Ignore())
                .ForMember(dest => dest.FromDate, opt => opt.Ignore())
                .ForMember(dest => dest.ToDate, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedTime, opt => opt.Ignore());

            CreateMap<ReportRequest, ReportRequestDto>();

            CreateMap<ReportRequest, PdfRequest>();

            CreateMap<AddReportRequestDto, PdfFilter>();

            CreateMap<PdfRequest, PdfFilter>();

            CreateMap<AzureReportProcessResultDto, ReportAIContent>().
                ForMember(des => des.FacilityId, opt => opt.Ignore()).
                ForMember(des => des.OrganizationId, src => src.MapFrom(src => src.Organization.Id)).
                ForMember(des => des.Organization, src => src.Ignore()).
                ForMember(des => des.Facility, src => src.Ignore()).
            
                ForMember(des => des.AuditTime, opt => opt.Ignore()).
                ForMember(des => des.FilteredDate, opt => opt.Ignore()).
                ForMember(des => des.PdfFileName, src => src.MapFrom(src => src.ReportFileName)).
                ForMember(des => des.ContainerName, src => src.MapFrom(src => src.ContainerName));
        }

        private SortModel MapSortModel(Audit src)
        {
            var setting = src.Settings?.FirstOrDefault(setting => setting.Type == AuditSettingType.TrackerOrder);

            SortModel sortModel = null;

            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                var orderValue = setting.Value.Split(CommonConstants.SLASH);

                sortModel = new SortModel()
                {
                    OrderBy = orderValue[0],
                    SortOrder = orderValue[1]
                };
            }

            return sortModel;
        }

        private IReadOnlyCollection<dynamic> MapTrackerGroupAnswers(ResolutionContext context, IEnumerable<AuditTableColumnValue> values, SortModel sortModel = null)
        {
            var answerGroups = values.GroupBy(value => value.GroupId)
                   .Select(group => MapTrackerAnswers(group, context.Mapper.Map<IReadOnlyCollection<TrackerAnswerDto>>(group))).ToArray();

            if (sortModel?.SortOrder == CommonConstants.ASC_SORT_ORDER)
            {
                answerGroups = answerGroups
                    .OrderBy(ag => GetTrackerValue(ag[sortModel.OrderBy]))
                    .ThenByDescending(ag => ag[CommonConstants.MAX_ID])
                    .ToArray();
            }
            else if(sortModel?.SortOrder == CommonConstants.DESC_SORT_ORDER)
            {
                answerGroups = answerGroups
                    .OrderByDescending(ag => GetTrackerValue(ag[sortModel.OrderBy]))
                    .ThenByDescending(ag => ag[CommonConstants.MAX_ID])
                    .ToArray();
            }
            else
            {
                answerGroups = answerGroups.OrderByDescending(ag => ag[CommonConstants.MAX_ID]).ToArray();
            }

            return answerGroups;
        }

        private IDictionary<string, object> MapTrackerAnswers(IGrouping<string, AuditTableColumnValue> group, IEnumerable<TrackerAnswerDto> answers)
        {
            var answerGroup = new Dictionary<string, object>();
            answerGroup.Add(CommonConstants.GROUP_ID, group.Key);
            answerGroup.Add(CommonConstants.MAX_ID, group.Max(answer => answer.Id));

            foreach (var answer in answers)
            {
                answer.Answer = answer.Answer ?? string.Empty;
                answer.FormattedAnswer = answer.FormattedAnswer ?? string.Empty;
                answerGroup.Add(answer.QuestionId.ToString(), answer);
            }

            return answerGroup;
        }

        private object GetTrackerValue(dynamic answer)
        {
            var trackerAnswer = answer as TrackerAnswerDto;

            if (trackerAnswer == null)
            {
                return null;
            }

            if (trackerAnswer.FieldType != FieldTypes.DatePicker)
            {
                return trackerAnswer?.FormattedAnswer;
            }

            if (!DateTime.TryParse(trackerAnswer.FormattedAnswer, out DateTime date))
            {
                return null;
            }

            return date;
        }
    }
}
