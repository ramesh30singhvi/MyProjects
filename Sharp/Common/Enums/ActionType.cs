namespace SHARP.Common.Enums
{
    public enum ActionType
    {
        Login = 1,
        Logout,
        IdleLogout,
        InitAudit,
        CreateAudit,
        SaveAudit,
        DeleteAudit,
        DuplicateAudit,
        ArchiveAudit,
        SendForApprovalAudit,
        ReopenAudit,
        DissapproveAudit,
        SubmitAudit,
        DownloadPDFAudit,
        UndeleteAudit,
        UnarchiveAudit,
        ApproveAudit,
        NewAccount,
        RoleChange,
        OrganizationChange,
        InactiveChange,
        FailedLogin,
    }
}
