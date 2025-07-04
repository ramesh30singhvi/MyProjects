using Microsoft.Extensions.Configuration;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace SHARP.DAL
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly SHARPContext _context;
        private readonly IConfiguration _configuration;
        private IUserRepository _users;
        private ITrustedFEClientRepository _trustedFEClients;

        private IAuditRepository _auditRepository;

        private IOrganizationRepository _organizationRepository;

        private IFacilityRepository _facilityRepository;

        private IFormVersionRepository _formVersionRepository;
        private IFormOrganizationRepository _formOrganizationRepository;

        private IFormRepository _formRepository;

        private IIdentityUserRepository _identityUserRepository;
        private ITwoFATokenRepository _twoFATokenRepository;

        private ITableColumnRepository _tableColumnRepository;

        private IAuditTableColumnValueRepository _auditTableColumnValueRepository;

        private IAuditFieldValueRepository _auditFieldValueRepository;

        private IProgressNoteRepository _progressNoteRepository;

        private IAuditTypeRepository _auditTypeRepository;

        private ITableColumnGroupRepository _tableColumnGroupRepository;

        private IFormFieldItemRepository _formFieldItemRepository;

        private IFieldTypeRepository _fieldTypeRepository;

        private IFormFieldRepository _formFieldRepository;

        private IRecipientRepository _recipientRepository;

        private IFormSectionRepository _formSectionRepository;

        private IFormGroupRepository _formGroupRepository;

        private IFacilityTimeZoneRepository _facilityTimeZoneRepository;
        private IReportRepository _reportRepository;

        private IMemoRepository _memoRepository;

        private IPatientRepository _patientRepository;

        private IUserOrganizationRepository _userOrganizationRepository;

        private IUserFacilityRepository _userFacilityRepository;

        private IReportRequestRepository _reportRequestRepository;
        private IAuditStatusHistoryRepository _auditStatusHistoryRepository;

        private IAuditSettingRepository _auditSettingRepository;

        private IUserActivityRepository _userActivityRepository;

        private IDashboardInputTableRepository _dashboardInputTableRepository;

        private IDashboardInputGroupsRepository _dashboardInputGroupsRepository;

        private IDashboardInputElementRepository _dashboardInputElementRepository;

        private IDashboardInputValuesRepository _dashboardInputValuesRepository;

        private IKeywordTriggerRepository _keywordTriggerRepository;
        private IAuditTriggeredRepository _auditTriggeredRepository;

        private IReportAIContentRepository _reportAIContentRepository;

        private IReportRangeRepository _reportRangeRepository;

        private IReportCategoryRepository _reportCategoryRepository ;

        private IReportTypeRepository _reportTypeRepository ;

        private IPortalReportRepository _portalReportRepository;

        private ISendReportToUserRepository _sendReportToUserRepository;

        private IFacilityRecipientRepository _facilityRecipientRepository;

        private IHighAlertStatusRepository _highAlertStatusRepository;

        private IHighAlertCategoryRepository _highAlertCategoryRepository;

        private IHighAlertAuditValueRepository _highAlertAuditValueRepository;

        private IHighAlertStatusHistoryRepository _highAlertStatusHistoryRepository;
     
        private IPortalFeatureRepository _portalFeatureRepository;

        private IOrganizationPortalFeatureRepository _organizationPortalFeatureRepository;

        private IAuditAIReportRepository _auditAIReportRepository;

        private IHighAlertPotentialAreasRepository _highAlertPotentialAreasRepository;


        private IAuditAIReportV2Repository _auditAIReportV2Repository;

        private IAuditAIPatientPdfNotesRepository _auditAIPatientPdfNotesRepository;

        private IAuditAIKeywordSummaryRepository _auditAIKeywordSummaryRepository;


        private ILoginsTrackingRepository _loginsTrackingRepository;

        private IDownloadsTrackingRepository _downloadsTrackingRepository;


        private IAuditorProductivityDashboardRepository _auditorProductivityDashboardRepository;

        private ITeamRepository _teamRepository;

        private IUserTeamRepository _userTeamRepository;


        public UnitOfWork(SHARPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IUserRepository UserRepository => _users ??= new UserRepository(_context);

        public ITrustedFEClientRepository TrsutedFEClientRespository => _trustedFEClients ??= new TrustedFEClientRespository(_context);

        public IAuditRepository AuditRepository => _auditRepository ??= new AuditRepository(_context);

        public IOrganizationRepository OrganizationRepository => _organizationRepository ??= new OrganizationRepository(_context);

        public IFacilityRepository FacilityRepository => _facilityRepository ??= new FacilityRepository(_context);

        public IFormRepository FormRepository => _formRepository ??= new FormRepository(_context);



        public IIdentityUserRepository AspNetUserRepository => _identityUserRepository ??= new IdentityUserRepository(_context);

        public ITwoFATokenRepository TwoFATokenRepository => _twoFATokenRepository ??= new TwoFATokenRepository(_context);

        public ITableColumnRepository TableColumnRepository => _tableColumnRepository ??= new TableColumnRepository(_context);

        public IAuditTableColumnValueRepository AuditTableColumnValueRepository => 
            _auditTableColumnValueRepository ??= new AuditTableColumnValueRepository(_context);

        public IAuditFieldValueRepository AuditFieldValueRepository => _auditFieldValueRepository ??= new AuditFieldValueRepository(_context);

        public IProgressNoteRepository ProgressNoteRepository => _progressNoteRepository ??= new ProgressNoteRepository(_context);

        public IAuditTypeRepository AuditTypeRepository => _auditTypeRepository ??= new AuditTypeRepository(_context);

        public IFormVersionRepository FormVersionRepository => _formVersionRepository ??= new FormVersionRepository(_context);

        public IFormSectionRepository FormSectionRepository => _formSectionRepository ??= new FormSectionRepository(_context);

        public IFormGroupRepository FormGroupRepository => _formGroupRepository ??= new FormGroupRepository(_context);

        public IFormOrganizationRepository FormOrganizationRepository => _formOrganizationRepository ??= new FormOrganizationRepository(_context);

        public ITableColumnGroupRepository TableColumnGroupRepository => _tableColumnGroupRepository ??= new TableColumnGroupRepository(_context);

        public IFieldTypeRepository FieldTypeRepository => _fieldTypeRepository ??= new FieldTypeRepository(_context);

        public IFormFieldRepository FormFieldRepository => _formFieldRepository ??= new FormFieldRepository(_context);

        public IFormFieldItemRepository FormFieldItemRepository => _formFieldItemRepository ??= new FormFieldItemRepository(_context);

        public IRecipientRepository RecipientRepository => _recipientRepository ??= new RecipientRepository(_context);

        public IFacilityTimeZoneRepository FacilityTimeZoneRepository => _facilityTimeZoneRepository ??= new FacilityTimeZoneRepository(_context);
        public IReportRepository ReportRepository => _reportRepository ??= new ReportRepository(_context);

        public IMemoRepository MemoRepository => _memoRepository ??= new MemoRepository(_context);

        public IPatientRepository PatientRepository => _patientRepository ??= new PatientRepository(_context);

        public IUserOrganizationRepository UserOrganizationRepository => _userOrganizationRepository ??= new UserOrganizationRepository(_context);

        public IUserFacilityRepository UserFacilityRepository => _userFacilityRepository ??= new UserFacilityRepository(_context);

        public IReportRequestRepository ReportRequestRepository => _reportRequestRepository ??= new ReportRequestRepository(_context);

        public IAuditStatusHistoryRepository AuditStatusHistoryRepository => _auditStatusHistoryRepository ??= new AuditStatusHistoryRepository(_context);

        public IAuditSettingRepository AuditSettingRepository => _auditSettingRepository ??= new AuditSettingRepository(_context);

        public IUserActivityRepository UserActivityRepository => _userActivityRepository ??= new UserActivityRepository(_context, _configuration);

        public IDashboardInputTableRepository DashboardInputTableRepository => _dashboardInputTableRepository ??= new DashboardInputTableRepository(_context);

        public IDashboardInputGroupsRepository DashboardInputGroupsRepository => _dashboardInputGroupsRepository ??= new DashboardInputGroupsRepository(_context);

        public IDashboardInputElementRepository DashboardInputElementRepository => _dashboardInputElementRepository ??= new DashboardInputElementRepository(_context);

        public IDashboardInputValuesRepository DashboardInputValuesRepository => _dashboardInputValuesRepository ??= new DashboardInputValuesRepository(_context);

        public IKeywordTriggerRepository KeywordTriggerRepository => _keywordTriggerRepository ??= new KeywordTriggerRepository(_context);

        public IAuditTriggeredRepository AuditTriggeredRepository => _auditTriggeredRepository ??= new AuditTriggeredRepository(_context);

        public IReportAIContentRepository ReportAIContentRepository => _reportAIContentRepository ??= new ReportAIContentRepository(_context);

        public IReportRangeRepository ReportRangeRepository => _reportRangeRepository ??= new ReportRangeRepository(_context);

        public IReportCategoryRepository ReportCategoryRepository => _reportCategoryRepository ??= new ReportCategoryRepository(_context);

        public IReportTypeRepository ReportTypeRepository => _reportTypeRepository ??= new ReportTypeRepository(_context);

        public IPortalReportRepository PortalReportRepository => _portalReportRepository ??= new PortalReportRepository(_context);

        public ISendReportToUserRepository SendReportToUserRepository => _sendReportToUserRepository ??= new SendReportToUserRepository(_context);

        public IFacilityRecipientRepository FacilityRecipientRepository => _facilityRecipientRepository ??= new FacilityRecipientRepository(_context);

        public IHighAlertCategoryRepository HighAlertCategoryRepository => _highAlertCategoryRepository ?? new HighAlertCategoryRepository(_context);

        public IHighAlertStatusRepository HighAlertStatusRepository => _highAlertStatusRepository ?? new HighAlertStatusRepository(_context);

        public IHighAlertAuditValueRepository HighAlertAuditValueRepository => _highAlertAuditValueRepository ?? new HighAlertAuditValueRepository(_context);
       
        public IHighAlertStatusHistoryRepository HighAlertStatusHistoryRepository => _highAlertStatusHistoryRepository ?? new HighAlertStatusHistoryRepository(_context);

        public IPortalFeatureRepository PortalFeatureRepository =>_portalFeatureRepository ?? new PortalFeatureRepository(_context);

        public IOrganizationPortalFeatureRepository OrganizationPortalFeatureRepository => _organizationPortalFeatureRepository ?? new OrganizationPortalFeatureRepository(_context);

        public IAuditAIReportRepository AuditAIReportRepository => _auditAIReportRepository ?? new AuditAIReportRepository(_context);

        public IHighAlertPotentialAreasRepository HighAlertPotentialAreasRepository => _highAlertPotentialAreasRepository??  new HighAlertPotentialAreasRepository (_context);

        public IAuditAIReportV2Repository AuditAIReportV2Repository => _auditAIReportV2Repository ?? new AuditAIReportV2Repository(_context);

        public IAuditAIPatientPdfNotesRepository AuditAIPatientPdfNotesRepository => _auditAIPatientPdfNotesRepository ?? new AuditAIPatientPdfNotesRepository(_context);


        public IAuditAIKeywordSummaryRepository AuditAIKeywordSummaryRepository => _auditAIKeywordSummaryRepository ?? new AuditAIKeywordSummaryRepository(_context);  

        public ILoginsTrackingRepository LoginsTrackingRepository => _loginsTrackingRepository ?? new LoginsTrackingRepository(_context);

        public IDownloadsTrackingRepository DownloadsTrackingRepository => _downloadsTrackingRepository ?? new DownloadsTrackingRepository(_context);


        public IAuditorProductivityDashboardRepository AuditorProductivityDashboardRepository => _auditorProductivityDashboardRepository ?? new AuditorProductivityDashboardRepository(_context);

        public ITeamRepository TeamRepository => _teamRepository ?? new TeamRepository(_context);

        public IUserTeamRepository UserTeamRepository => _userTeamRepository ?? new UserTeamRepository(_context);


        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
