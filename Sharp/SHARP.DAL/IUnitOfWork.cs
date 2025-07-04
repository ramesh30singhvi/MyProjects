using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace SHARP.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        ITrustedFEClientRepository TrsutedFEClientRespository { get; }

        IAuditRepository AuditRepository { get; }
        IAuditStatusHistoryRepository AuditStatusHistoryRepository { get; }

        IOrganizationRepository OrganizationRepository { get; }

        IFacilityRepository FacilityRepository { get; }

        IFormVersionRepository FormVersionRepository { get; }

        IFormOrganizationRepository FormOrganizationRepository { get; }

        IFormRepository FormRepository { get; }

        IIdentityUserRepository AspNetUserRepository { get; }

        ITwoFATokenRepository TwoFATokenRepository { get; }

        ITableColumnRepository TableColumnRepository { get; }

        IFormSectionRepository FormSectionRepository { get; }

        IFormGroupRepository FormGroupRepository { get; }

        IAuditTableColumnValueRepository AuditTableColumnValueRepository { get; }

        IAuditFieldValueRepository AuditFieldValueRepository { get;  }

        IProgressNoteRepository ProgressNoteRepository { get; }

        IAuditTypeRepository AuditTypeRepository { get; }

        ITableColumnGroupRepository TableColumnGroupRepository { get; }

        IFieldTypeRepository FieldTypeRepository { get; }

        IFormFieldRepository FormFieldRepository { get; }

        IFormFieldItemRepository FormFieldItemRepository { get; }

        IRecipientRepository RecipientRepository { get; }

        IFacilityTimeZoneRepository FacilityTimeZoneRepository { get; }
        IReportRepository ReportRepository { get; }

        IMemoRepository MemoRepository { get; }

        IPatientRepository PatientRepository { get; }

        IUserOrganizationRepository UserOrganizationRepository { get; }

        IUserFacilityRepository UserFacilityRepository { get; }

        IReportRequestRepository ReportRequestRepository { get; }

        IAuditSettingRepository AuditSettingRepository { get; }

        IUserActivityRepository UserActivityRepository { get; }

        IDashboardInputTableRepository DashboardInputTableRepository { get; }

        IDashboardInputGroupsRepository DashboardInputGroupsRepository { get; }

        IDashboardInputElementRepository DashboardInputElementRepository { get; }

        IDashboardInputValuesRepository DashboardInputValuesRepository { get; }
        IKeywordTriggerRepository KeywordTriggerRepository { get; }

        IAuditTriggeredRepository AuditTriggeredRepository { get; }

        IReportAIContentRepository ReportAIContentRepository { get; }

        IReportRangeRepository ReportRangeRepository { get; }

        IReportCategoryRepository   ReportCategoryRepository { get; }

        IReportTypeRepository ReportTypeRepository { get; }

        IPortalReportRepository PortalReportRepository { get; }

        ISendReportToUserRepository SendReportToUserRepository { get; }

        IFacilityRecipientRepository FacilityRecipientRepository { get; }

        IHighAlertCategoryRepository HighAlertCategoryRepository { get; }

        IHighAlertStatusRepository HighAlertStatusRepository { get; }

        IHighAlertAuditValueRepository HighAlertAuditValueRepository { get; }

        IHighAlertStatusHistoryRepository HighAlertStatusHistoryRepository { get; }
       
        IPortalFeatureRepository PortalFeatureRepository { get; }

        IOrganizationPortalFeatureRepository OrganizationPortalFeatureRepository { get; }

        IAuditAIReportRepository AuditAIReportRepository { get; }

        IHighAlertPotentialAreasRepository HighAlertPotentialAreasRepository { get; }
        
        IAuditAIReportV2Repository AuditAIReportV2Repository { get; }

        IAuditAIPatientPdfNotesRepository AuditAIPatientPdfNotesRepository { get; }

        IAuditAIKeywordSummaryRepository AuditAIKeywordSummaryRepository { get; }

        IAuditorProductivityDashboardRepository AuditorProductivityDashboardRepository { get; }

        ITeamRepository TeamRepository { get; }

        IUserTeamRepository UserTeamRepository { get; }

        int SaveChanges();

        Task<int> SaveChangesAsync();

        ILoginsTrackingRepository LoginsTrackingRepository { get; }

        IDownloadsTrackingRepository DownloadsTrackingRepository { get; }
    }
}
