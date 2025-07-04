using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Extensions;
using SHARP.DAL.Models.Views;

namespace SHARP.DAL.Models
{
    public partial class SHARPContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>,
    ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public static string ConnectionString { get; set; }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<TrustedFEClient> TrustedFEClient { get; set; }

        public DbSet<Audit> Audit { get; set; }
        public DbSet<AuditStatusHistory> AuditStatusHistory { get; set; }

        public DbSet<UserOrganization> UserOrganization { get; set; }
        public DbSet<Facility> Facility { get; set; }

        public DbSet<Organization> Organization { get; set; }

        public DbSet<FormSection> FormSection { get; set; }

        public DbSet<FormGroup> FormGroup { get; set; }

        public DbSet<DashboardInputTable> DashboardInputTable { get; set; }

        public DbSet<DashboardInputGroups> DashboardInputGroups { get; set; }

        public DbSet<DashboardInputElement> DashboardInputElement { get; set; }

        public DbSet<DashboardInputValues> DashboardInputValues { get; set; }

        public DbSet<Patient> Patient { get; set; }

        public DbSet<AuditType> AuditType { get; set; }

        public DbSet<FormVersion> FormVersion { get; set; }

        public DbSet<Form> Form { get; set; }

        public DbSet<FormOrganization> FormOrganization { get; set; }

        public DbSet<TwoFAToken> TwoFAToken { get; set; }

        public DbSet<TableColumnGroup> TableColumnGroup { get; set; }

        public DbSet<TableColumn> TableColumn { get; set; }

        public DbSet<AuditTableColumnValue> AuditTableColumnValue { get; set; }

        public DbSet<ProgressNote> ProgressNote { get; set; }

        public DbSet<FieldType> FieldType { get; set; }

        public DbSet<FormField> FormField { get; set; }

        public DbSet<CriteriaOption> CriteriaOptions { get; set; }

        public DbSet<TrackerOption> TrackerOptions { get; set; }

        public DbSet<FormFieldItem> FormFieldItems { get; set; }

        public DbSet<TableColumnItem> TableColumnItems { get; set; }

        public DbSet<AuditFieldValue> AuditFieldValue { get; set; }

        public virtual DbSet<KeywordsMatchingCountResult> KeywordsMatchingCountResult { get; set; }

        public DbSet<OrganizationRecipient> OrganizationRecipient { get; set; }

        public DbSet<FacilityTimeZone> FacilityTimeZones { get; set; }

        public DbSet<FacilityRecipient> FacilityRecipients { get; set; }

        public DbSet<FormScheduleSetting> FormScheduleSettings { get; set; }

        public DbSet<Memo> Memos { get; set; }

        public DbSet<OrganizationMemo> OrganizationMemos { get; set; }

        public DbSet<ReportRequest> ReportRequests { get; set; }

        public DbSet<AuditSetting> AuditSettings { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }

        public DbSet<KeywordTrigger> KeywordTrigger { get; set; }

        public DbSet<AuditTriggeredByKeyword> AuditTriggeredByKeyword { get; set; }

        public DbSet<ReportAIContent> ReportAIContent { get; set; }

        public DbSet<PortalReport> PortalReport { get; set; }

        public DbSet<SendReportToUser> SendReportToUser { get; set; }

        public DbSet<HighAlertStatus> HighAlertStatus { get; set; }

        public DbSet<HighAlertCategory> HighAlertCategory { get; set; }


        public DbSet<HighAlertAuditValue> HighAlertAuditValue { get; set; }

        public DbSet<HighAlertStatusHistory> HighAlertStatusHistory { get; set; }

        public DbSet<PortalFeature> PortalFeature { get; set; }

        public DbSet<OrganizationPortalFeature> OrganizationPortalFeature { get; set; }
        public DbSet<AuditAIReport> AuditAIReport { get; internal set; }

        public DbSet<HighAlertPotentialAreas> HighAlertPotentialAreas { get; set; }
        public DbSet<HighAlertCategoryToPotentialAreas> HighAlertCategoryToPotentialAreas { get; set; }
        public DbSet<GetUserActivities> GetUserActivities { get; set; }
        public DbSet<LoginsTracking> LoginsTracking { get; set; }
        public DbSet<DownloadsTracking> DownloadsTracking { get; set; }

        public DbSet<AuditAIReportV2> AuditAIReportV2 { get; internal set; }

        public DbSet<AuditAIPatientPdfNotes> AuditAIPatientPdfNotes { get; set; }

        public DbSet<AuditAIKeywordSummary> AuditAIKeywordSummary { get; set; }

        public DbSet<AuditSummary> AuditSummary { get; set; }

        public DbSet<AuditorProductivityInputView> AuditorProductivityInputView { get; set; }

        public DbSet<AuditorProductivityAHTPerAuditTypeView> AuditorProductivityAHTPerAuditTypeView { get; set; }

        public DbSet<AuditorProductivitySummaryPerAuditorView> AuditorProductivitySummaryPerAuditorView { get; set; }

        public DbSet<Team> Team { get;  set; }

        public DbSet<UserTeam> UserTeam { get; set; }
        public SHARPContext(DbContextOptions<SHARPContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(300)).EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.BuildApplicationUserRoleEntity();

            modelBuilder.BuildUserEntity();
            modelBuilder.BuildAuditEntity();
            modelBuilder.BuildAuditStatusHistoryEntity();
            modelBuilder.BuildTrustedFEClientEntity();
            modelBuilder.BuildFacilityEntity();
            modelBuilder.BuildOrganizationEntity();
            modelBuilder.BuildFormEntity();
            modelBuilder.BuildFormOrganizationEntity();
            modelBuilder.BuildFormVersionEntity();
            modelBuilder.BuildAuditTypeEntity();
            modelBuilder.BuildPatientEntity();
            modelBuilder.BuildTwoFATokenEntity();
            modelBuilder.BuildTableColumnGroupEntity();
            modelBuilder.BuildTableColumnEntity();
            modelBuilder.BuildAuditTableColumnValueEntity();
            modelBuilder.BuildProgressNoteEntity();
            modelBuilder.BuildFieldTypeEntity();
            modelBuilder.BuildFormFieldEntity();
            modelBuilder.BuildCriteriaOptionEntity();
            modelBuilder.BuildTrackerOptionEntity();
            modelBuilder.BuildFormFieldItemEntity();
            modelBuilder.BuildTableColumnItemEntity();
            modelBuilder.BuildFormFieldValueEntity();
            modelBuilder.BuildOrganizationRecipientEntity();
            modelBuilder.BuildFacilityTimeZoneEntity();
            modelBuilder.BuildFacilityRecipientEntity();
            modelBuilder.BuildFormScheduleSettingEntity();
            modelBuilder.BuildReportEntity();
            modelBuilder.BuildMemoEntity();
            modelBuilder.BuildOrganizationMemoEntity();
            modelBuilder.BuildReportRequestEntity();
            modelBuilder.BuildAuditSettingEntity();
            modelBuilder.BuildUserActivityEntity();
            modelBuilder.BuildDashboardInputTable();
            modelBuilder.BuildDashboardInputGroups();
            modelBuilder.BuildDashboardInputElement();
            modelBuilder.BuildDashboardInputValues();
            modelBuilder.BuildKeywordTriggerEntity();
            modelBuilder.BuildAuditTriggeredByKeywordEntity();
            modelBuilder.BuildReportCategoryEntity();
            modelBuilder.BuildReportTypeEntity();
            modelBuilder.BuildReportRangeEntity();
            modelBuilder.BuildPortalReportEntity();
            modelBuilder.BuildSendReportToUserEntity();
            modelBuilder.BuildReportAIContentEntity();
            modelBuilder.BuildHighAlertCategoryEntity();
            modelBuilder.BuildHighAlertStatusEntity();
            modelBuilder.BuildHighAlertAuditValueEntity();
            modelBuilder.BuildOrganizationPortalFeatureEntity();
            modelBuilder.BuildPortalFeatureEntity();
            modelBuilder.BuildAuditAIReportEntity();
            modelBuilder.BuildHighAlertCategoryToPotentialAreasEntity();
            modelBuilder.BuildHighAlertPotentialAreasEntity();
            modelBuilder.BuildAuditAIReportV2Entity();
            modelBuilder.BuildAuditAIPatientPdfNotesEntity();
            modelBuilder.BuildAuditAIKeywordSummaryEntity();
            modelBuilder.BuildLoginsTrackingEntity();
            modelBuilder.BuildDownloadsTrackingEntity();
            modelBuilder.BuildAuditorProductivityInputViewEntity();
            modelBuilder.BuildAuditorProductivityAHTPerAuditTypeViewEntity();
            modelBuilder.BuildAuditorProductivitySummaryPerAuditorViewEntity();
            modelBuilder.BuildTeamEntity();
            modelBuilder.BuildUserTeamEntity();


            var roleOrderValueMethod = typeof(SqlFunctions)
                .GetMethod(nameof(SqlFunctions.RoleOrderValue), new[] { typeof(string) });
            modelBuilder.HasDbFunction(roleOrderValueMethod).HasName("RoleOrderValue");

            var organizationOrderValueMethod = typeof(SqlFunctions)
                .GetMethod(nameof(SqlFunctions.OrganizationOrderValue), new[] { typeof(int) });
            modelBuilder.HasDbFunction(organizationOrderValueMethod).HasName("OrganizationOrderValue");

            var organizationFormVersionOrderValueMethod = typeof(SqlFunctions)
                .GetMethod(nameof(SqlFunctions.OrganizationFormVersionOrderValue), new[] { typeof(int) });
            modelBuilder.HasDbFunction(organizationFormVersionOrderValueMethod).HasName("OrganizationFormVersionOrderValue");

            

            modelBuilder.Entity<KeywordsMatchingCountResult>(e => e.HasNoKey());
            modelBuilder.Entity<AuditSummary>(e => e.HasNoKey());
        }
    }
}