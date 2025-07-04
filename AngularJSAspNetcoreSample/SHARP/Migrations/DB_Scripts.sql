ALTER TABLE dbo.Audit DROP COLUMN SubmittedDate;
ALTER TABLE dbo.Audit ADD SubmittedDate datetime2(0) NOT NULL;

ALTER TABLE dbo.[User] ADD FullName AS (rtrim(ltrim((isnull([FirstName],'')+' ')+isnull([LastName],'')))) PERSISTED

ALTER TABLE dbo.[Patient] ADD FullName AS (RTRIM(LTRIM(RTRIM(ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName,'')) + ' ' + ISNULL(LastName,'')))) PERSISTED

ALTER TABLE dbo.Audit ADD Unit nvarchar(50) NULL;

ALTER TABLE dbo.Audit ADD Room nvarchar(50) NULL;

ALTER TABLE dbo.Audit ADD Identifier nvarchar(200) NULL;

ALTER TABLE dbo.Audit ADD FilterDate datetime2(0) NULL;

ALTER TABLE dbo.Audit ADD Reason nvarchar(1000) NULL;

----

ALTER TABLE dbo.Form ADD LegacyFormID int NULL;

ALTER TABLE dbo.FacilityAuditTemplate ADD LegacyFacilityTemplateID int NULL;

ALTER TABLE dbo.[User] ADD LegacyID int NULL;

ALTER TABLE dbo.TableColumn ADD LegacyFormRowID int NULL;

ALTER TABLE dbo.FacilityAuditTemplate ALTER COLUMN Name nvarchar(100) NULL;

ALTER TABLE dbo.TableColumn ADD LegacyFormRowID int NULL;

ALTER TABLE dbo.TableColumn ADD LegacyRowID int NULL;

ALTER TABLE dbo.Facility ADD TimeZone int NULL;

UPDATE dbo.Facility SET TimeZone = -10 WHERE State in ('HI');
UPDATE dbo.Facility SET TimeZone = -9 WHERE State in ('AK');
UPDATE dbo.Facility SET TimeZone = -8 WHERE State in ('CA', 'NV', 'OR', 'WA');
UPDATE dbo.Facility SET TimeZone = -7 WHERE State in ('AZ', 'CO', 'ID', 'MT', 'NM', 'UT', 'WY');
UPDATE dbo.Facility SET TimeZone = -6 WHERE State in ('AL', 'AR', 'IL', 'IA', 'KS', 'KY', 'LA', 'MN', 'MS', 'MO', 'NE', 'ND', 'OK', 'SD', 'TN', 'TX', 'WI');
UPDATE dbo.Facility SET TimeZone = -5 WHERE State in ('CT', 'DE', 'DC', 'FL', 'GA', 'IN', 'ME', 'MD', 'MA', 'MI', 'NH', 'NJ','NY', 'NC', 'OH', 'PA', 'RI', 'SC', 'VT', 'VA', 'WV');
UPDATE dbo.Facility SET TimeZone = -4 WHERE State in ('PR', 'USVI');
UPDATE dbo.Facility SET TimeZone = 10 WHERE State in ('GU', 'MP');

----

DELETE FROM dbo.Audit;
DELETE FROM dbo.FacilityAuditTemplate;
DELETE FROM dbo.Form;

ALTER TABLE dbo.FacilityAuditTemplate DROP CONSTRAINT FK_FacilityAuditTemplate_Form;
ALTER TABLE dbo.FormField DROP CONSTRAINT FK_FormField_Form;
ALTER TABLE dbo.TableColumn DROP CONSTRAINT FK_TableColumn_Form;

DROP TABLE dbo.Form;

CREATE TABLE dbo.Form (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	AuditTypeID int NOT NULL,
	IsActive bit NOT NULL,
	LegacyFormID int NULL,

	CONSTRAINT FK_Form_AuditType FOREIGN KEY (AuditTypeID) REFERENCES dbo.AuditType(ID)
);

ALTER TABLE dbo.FacilityAuditTemplate ADD CONSTRAINT FK_FacilityAuditTemplate_Form FOREIGN KEY (FormID) REFERENCES dbo.Form(ID);
ALTER TABLE dbo.FormField ADD CONSTRAINT FK_FormField_Form FOREIGN KEY (FormID) REFERENCES dbo.Form(ID);
ALTER TABLE dbo.TableColumn ADD CONSTRAINT FK_TableColumn_Form FOREIGN KEY (FormID) REFERENCES dbo.Form(ID);

----

ALTER TABLE dbo.AuditTableColumnValue DROP CONSTRAINT FK_AuditTableColumnValue_TableColumn;

DROP TABLE dbo.TableColumn;

CREATE TABLE dbo.TableColumn (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(max) NOT NULL,
	[Sequence] int NOT NULL,
	FormID int NOT NULL,
	GroupID int NULL,
	
	CONSTRAINT FK_TableColumn_Form FOREIGN KEY (FormID) REFERENCES dbo.Form(ID),
	CONSTRAINT FK_TableColumn_TableColumnGroup FOREIGN KEY (GroupID) REFERENCES dbo.TableColumnGroup(ID)
);

ALTER TABLE dbo.AuditTableColumnValue ADD CONSTRAINT FK_AuditTableColumnValue_TableColumn FOREIGN KEY (TableColumnID) REFERENCES dbo.TableColumn(ID);

----

INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES(5, 'Facility', 'FACILITY');

----

EXEC sys.sp_rename N'dbo.Audit.FilterDate' , N'IncidentDateFrom', 'COLUMN';

ALTER TABLE dbo.Audit ADD IncidentDateTo datetime2(0) NULL;
----

INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES(6, 'Admin', 'Admin');

ALTER TABLE [User]
ADD FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id);

GO
CREATE FUNCTION RoleOrderValue(@identityUser NVARCHAR(450))
RETURNS NVARCHAR(255)
AS
BEGIN
    RETURN (SELECT STRING_AGG(Roles.Name, ', ')
			FROM AspNetUsers Users
			JOIN AspNetUserRoles UserRoles ON Users.Id = UserRoles.UserId
			JOIN AspNetRoles Roles ON UserRoles.RoleId = Roles.Id
			WHERE Users.Id = @identityUser
			GROUP BY Users.Id);
END

GO
CREATE FUNCTION OrganizationOrderValue(@sharUser INT)
RETURNS NVARCHAR(255)
AS
BEGIN
    RETURN (SELECT CASE
						WHEN (SELECT COUNT(*)
							  FROM UserOrganization
							  WHERE UserOrganization.UserID = @sharUser) = 0
							THEN 'All'
						ELSE STRING_AGG(Organization.Name, ', ')
					END
			FROM [User]
			LEFT JOIN UserOrganization ON [User].Id = UserOrganization.UserId
			LEFT JOIN Organization ON UserOrganization.OrganizationID = Organization.ID
			WHERE [User].Id = @sharUser
			GROUP BY [User].ID);
END

ALTER TABLE [User]
ADD [Status] INT NOT NULL DEFAULT 1;

CREATE TABLE TwoFAToken(
	Id NVARCHAR(450) PRIMARY KEY FOREIGN KEY REFERENCES AspNetUsers(Id),
	Token NVARCHAR(70) NOT NULL,
	CreatedAt DATETIME2 NOT NULL
);

----

ALTER TABLE dbo.TableColumn ADD LegacyFormColumnID int NULL;

---

ALTER TABLE [Audit]
ADD ResidentName NVARCHAR(100) NULL;

ALTER TABLE [Audit]
ALTER COLUMN TotalNA INT NULL;

EXEC sp_rename 'dbo.Audit.TotalComplience', 'TotalCompliance', 'COLUMN';

ALTER TABLE [Audit]
ALTER COLUMN TotalCompliance FLOAT NULL;

---

ALTER TABLE [FacilityAuditTemplate]
DROP COLUMN IsActive;

ALTER TABLE dbo.TableColumn ADD LegacyFormColumnID int NULL;

---- SHAR-13

CREATE FUNCTION [dbo].[GetCountKeywordsMatching] (@facilityId int, @keyword nvarchar(100), @dateFrom datetime, @dateTo datetime)
RETURNS int
AS
BEGIN
	DECLARE @result int;

    WITH keyword_count_cte ([Id], [Keyword_index])
	AS
	(
	    SELECT pn.ID , CHARINDEX(@keyword, pn.ProgressNoteText COLLATE Latin1_General_CI_AS, 0)+1 AS [Keyword_index]
		FROM ProgressNote pn
		JOIN Patient p ON pn.PatientID = p.ID 
		AND p.FacilityID = @facilityId 
		AND pn.CreatedDate BETWEEN @dateFrom AND @dateTo 
		AND CHARINDEX(@keyword, pn.ProgressNoteText COLLATE Latin1_General_CI_AS, 0) != 0
		UNION ALL
		SELECT pn2.ID, CHARINDEX(@keyword, pn2.ProgressNoteText COLLATE Latin1_General_CI_AS, [Keyword_index])+1 AS [Keyword_index]
		FROM keyword_count_cte cte
		JOIN ProgressNote pn2 ON cte.Id = pn2.ID 
		AND pn2.CreatedDate BETWEEN @dateFrom AND @dateTo 
		AND CHARINDEX(@keyword, pn2.ProgressNoteText COLLATE Latin1_General_CI_AS, [Keyword_index]) > 0
	)

	SELECT @result = count(*) FROM keyword_count_cte;
	
	RETURN @result;
END
GO

---- SHAR-13

ALTER TABLE dbo.AuditTableColumnValue ADD Resident nvarchar(300) NULL;

ALTER TABLE dbo.AuditTableColumnValue ADD ProgressNoteTime datetime2(0) NULL;

ALTER TABLE dbo.AuditTableColumnValue ADD Description nvarchar(max) NULL;

--- SHAR-22

ALTER TABLE AuditTableColumnValue 
ADD GroupId nvarchar(50) NULL;

--- SHAR-14

ALTER TABLE [Audit]
ALTER COLUMN IncidentDateFrom datetime2(0) NOT NULL;

------- SHAR-104

ALTER TABLE dbo.Form ADD OrganizationId int NULL;
ALTER TABLE dbo.Form ADD CONSTRAINT FK_Form_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organization(ID);

ALTER TABLE	dbo.Audit DROP CONSTRAINT FK_Audit_FacilityAuditTemplate
ALTER TABLE	dbo.FacilityAuditTemplate DROP CONSTRAINT FK_FacilityAuditTemplate_Form
ALTER TABLE	dbo.FacilityAuditTemplate DROP CONSTRAINT FK_FacilityAuditTemplate_Facility

ALTER TABLE dbo.Audit ADD FormId int NULL;
ALTER TABLE dbo.Audit ADD CONSTRAINT FK_Audit_Form FOREIGN KEY (FormId) REFERENCES dbo.Form(ID);

ALTER TABLE dbo.Audit ADD FacilityId int NULL;
ALTER TABLE dbo.Audit ADD CONSTRAINT FK_Audit_Facility FOREIGN KEY (FacilityId) REFERENCES dbo.Facility(ID);

-- SHAR-104 after migrate
--ALTER TABLE dbo.Form ALTER COLUMN OrganizationId int NOT NULL;
ALTER TABLE dbo.Audit ALTER COLUMN FormId int NOT NULL;
ALTER TABLE dbo.Audit ALTER COLUMN FacilityId int NOT NULL;

ALTER TABLE dbo.Audit DROP COLUMN FacilityAuditTemplateID;

------ SHAR-75

ALTER TABLE dbo.Form ADD Status int NOT NULL DEFAULT(2);
ALTER TABLE dbo.Form ADD CreatedDate datetime2(0) NULL;
ALTER TABLE dbo.Form ADD CreatedByUserId int NULL;
ALTER TABLE dbo.Form ADD CONSTRAINT FK_Form_User FOREIGN KEY (CreatedByUserId) REFERENCES dbo.[User](ID);

----- SHAR-81

CREATE TABLE FormVersion(
	Id int IDENTITY(1,1) PRIMARY KEY,
	FormId int NOT NULL,
	Version int NOT NULL,
	Status int NOT NULL,
	CreatedDate datetime2(0) NULL,	
	CreatedByUserId int NULL,
	ActivationDate datetime2(0) NULL,
	
	CONSTRAINT FK_FormVersion_Form FOREIGN KEY (FormId) REFERENCES dbo.Form(ID),
	CONSTRAINT FK_FormVersion_User FOREIGN KEY (CreatedByUserId) REFERENCES dbo.[User](ID)
);

ALTER TABLE dbo.Audit ADD FormVersionId int NULL;
ALTER TABLE dbo.Audit ADD CONSTRAINT FK_Audit_FormVersion FOREIGN KEY (FormVersionId) REFERENCES dbo.FormVersion(Id);

ALTER TABLE dbo.TableColumn ADD FormVersionId int NULL;
ALTER TABLE dbo.TableColumn ADD CONSTRAINT FK_TableColumn_FormVersion FOREIGN KEY (FormVersionId) REFERENCES dbo.FormVersion(Id) ON DELETE CASCADE;

ALTER TABLE dbo.FormField ADD FormVersionId int NULL;
ALTER TABLE dbo.FormField ADD CONSTRAINT FK_FormField_FormVersion FOREIGN KEY (FormVersionId) REFERENCES dbo.FormVersion(Id);

INSERT INTO dbo.FormVersion 
SELECT f.ID FormId, 1 Version, f.Status, f.CreatedDate, f.CreatedByUserId, NULL 
FROM Form f;


ALTER TABLE dbo.Form DROP CONSTRAINT DF__Form__Status__503BEA1C;
ALTER TABLE dbo.Form DROP COLUMN [Status];

ALTER TABLE dbo.Form DROP COLUMN CreatedDate;

ALTER TABLE dbo.Form DROP CONSTRAINT FK_Form_User;
ALTER TABLE dbo.Form DROP COLUMN CreatedByUserId;

ALTER TABLE dbo.FormField DROP CONSTRAINT FK_FormField_Form;
ALTER TABLE dbo.FormField DROP COLUMN FormId;
ALTER TABLE dbo.FormField  ALTER COLUMN FormVersionId int NOT NULL;

UPDATE Audit SET FormVersionId = (SELECT fv.Id FROM FormVersion fv WHERE fv.FormId = Audit.FormId);

ALTER TABLE dbo.Audit DROP CONSTRAINT FK_Audit_Form;
ALTER TABLE dbo.Audit  DROP COLUMN FormId;
ALTER TABLE dbo.Audit ALTER COLUMN FormVersionId int NOT NULL;

UPDATE TableColumn SET FormVersionId = (SELECT fv.Id FROM FormVersion fv WHERE fv.FormId = TableColumn.FormId);

ALTER TABLE dbo.TableColumn DROP CONSTRAINT FK_TableColumn_Form;
ALTER TABLE dbo.TableColumn  DROP COLUMN FormId;
ALTER TABLE dbo.TableColumn  ALTER COLUMN FormVersionId int NOT NULL;


---- SHAR-122


CREATE TABLE [dbo].[FormOrganization](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FormId] [int] NOT NULL,
	[OrganizationId] [int] NOT NULL,
 CONSTRAINT [PK_FormOrganization] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[FormOrganization]  WITH CHECK ADD  CONSTRAINT [FK_FormOrganization_Form] FOREIGN KEY([FormId])
REFERENCES [dbo].[Form] ([ID])
GO

ALTER TABLE [dbo].[FormOrganization]  WITH CHECK ADD  CONSTRAINT [FK_FormOrganization_Organization] FOREIGN KEY([OrganizationId])
REFERENCES [dbo].[Organization] ([ID])

---- SHAR-110

UPDATE AspNetRoles SET Name = 'Admin', NormalizedName = 'ADMIN' WHERE Name = 'IT User'

---- SHAR-78

CREATE TABLE [dbo].[FormFieldItems](
	Id int IDENTITY(1,1) PRIMARY KEY,
	FormFieldId int NOT NULL,
	Value nvarchar(50) NOT NULL,
	[Sequence] int NOT NULL,

	CONSTRAINT FK_FormFieldItems_FormField FOREIGN KEY (FormFieldId) REFERENCES dbo.FormField(ID) ON DELETE CASCADE
);

ALTER TABLE [dbo].[TableColumn] ADD ParentId int NULL;
ALTER TABLE [dbo].[TableColumn]  WITH CHECK ADD  CONSTRAINT [FK_TableColumn_Parent_TableColumn] FOREIGN KEY([ParentId])
REFERENCES [dbo].[TableColumn] ([ID])

CREATE TABLE [dbo].[CriteriaOptions](
	TableColumnId int PRIMARY KEY,
	ShowNA bit NOT NULL,
	Compliance bit NOT NULL,
	Quality bit NOT NULL,
	Priority bit NOT NULL,

	CONSTRAINT FK_CriteriaOptions_TableColumn FOREIGN KEY (TableColumnId) REFERENCES dbo.TableColumn(ID) ON DELETE CASCADE
);


ALTER TABLE [dbo].[FormField] ADD IsRequired bit NOT NULL;


ALTER TABLE dbo.FormField DROP CONSTRAINT FK_FormField_FieldType;

DROP TABLE dbo.FieldType;

CREATE TABLE dbo.FieldType (
	ID int PRIMARY KEY,
	Name nvarchar(100)  NOT NULL
);

ALTER TABLE dbo.FormField ADD CONSTRAINT FK_FormField_FieldType FOREIGN KEY (FieldTypeID) REFERENCES dbo.FieldType(ID)

INSERT INTO dbo.FieldType
VALUES
(1, 'Checkbox'),
(2, 'Date picker'),
(3, 'Dropdown Single Select'),
(4, 'Text'),
(5, 'Toggle Single Select'),
(6, 'Dropdown Multiselect'),
(7, 'Toggle Multiselect');

ALTER TABLE [dbo].[TableColumnGroup] ADD FormVersionId int NULL;
ALTER TABLE [dbo].[TableColumnGroup] ADD CONSTRAINT FK_TableColumnGroup_FormVersion FOREIGN KEY (FormVersionId) REFERENCES dbo.FormVersion(Id) ON DELETE CASCADE

UPDATE [dbo].[TableColumnGroup] SET FormVersionId = (SELECT TOP(1) tc.FormVersionId FROM dbo.TableColumn tc WHERE tc.GroupID = TableColumnGroup.ID ORDER BY tc.ID DESC)

ALTER TABLE [dbo].[TableColumnGroup] ADD [Sequence] int NULL;
UPDATE [dbo].[TableColumnGroup] SET [Sequence] = 0
ALTER TABLE [dbo].[TableColumnGroup] ALTER COLUMN [Sequence] int NOT NULL;

ALTER TABLE dbo.FormField DROP CONSTRAINT FK_FormField_FormVersion;
ALTER TABLE dbo.FormField ADD CONSTRAINT FK_FormField_FormVersion FOREIGN KEY (FormVersionId) REFERENCES dbo.FormVersion(Id) ON DELETE CASCADE

---- shar-80

/*UPDATE dbo.FieldType SET Name = 'Toggle Single Select' WHERE ID = 5;
INSERT INTO dbo.FieldType
VALUES
(7, 'Toggle Multiselect');*/

CREATE TABLE [dbo].[TrackerOptions](
	TableColumnId int PRIMARY KEY,
	FieldTypeId int NOT NULL,
	IsRequired bit NOT NULL,
	Compliance bit NOT NULL,
	Quality bit NOT NULL,
	Priority bit NOT NULL,

	CONSTRAINT FK_TrackerOptions_TableColumn FOREIGN KEY (TableColumnId) REFERENCES dbo.TableColumn(ID) ON DELETE CASCADE,
	CONSTRAINT FK_TrackerOptions_FieldType FOREIGN KEY (FieldTypeId) REFERENCES dbo.FieldType(ID)
);

CREATE TABLE [dbo].[TableColumnItems](
	Id int IDENTITY(1,1) PRIMARY KEY,
	TableColumnId int NOT NULL,
	Value nvarchar(50) NOT NULL,
	[Sequence] int NOT NULL,

	CONSTRAINT FK_TableColumnItems_TrackerOptions FOREIGN KEY (TableColumnId) REFERENCES dbo.TrackerOptions(TableColumnId) ON DELETE CASCADE
);

-- SHAR-124
CREATE TABLE OrganizationRecipient(
	Id INT IDENTITY PRIMARY KEY,
	Recipient NVARCHAR(254) NOT NULL,
	OrganizationId INT NOT NULL FOREIGN KEY REFERENCES Organization(Id)
);

---- SHAR-98

CREATE TABLE [dbo].[FacilityTimeZones](
	Id int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(200) NOT NULL,
	DisplayName nvarchar(200) NOT NULL,
	ShortName nvarchar(10) NOT NULL,
);

ALTER TABLE dbo.Facility 
ADD TimeZoneId int NULL;

ALTER TABLE dbo.Facility 
ADD CONSTRAINT FK_Facilities_TimeZones FOREIGN KEY (TimeZoneId) REFERENCES dbo.FacilityTimeZones(Id);

INSERT INTO dbo.FacilityTimeZones (Name, DisplayName, ShortName)
VALUES 
('Hawaiian Standard Time', 'Hawaii-Aleutian Standard Time', 'HST'),
('Alaskan Standard Time', 'Alaska Standard Time', 'AKST'),
('Pacific Standard Time', 'Pacific Standard Time', 'PST'),
('Mountain Standard Time', 'Mountain Standard Time', 'MST'),
('Central Standard Time', 'Central Standard Time', 'CST'),
('Eastern Standard Time', 'Eastern Standard Time', 'EST'),
('Atlantic Standard Time', 'Atlantic Standard Time', 'AST'),
('West Pacific Standard Time', 'Chamorro Standard Time', 'ChST');

UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Hawaiian Standard Time') WHERE State in ('HI');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Alaskan Standard Time') WHERE State in ('AK');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Pacific Standard Time') WHERE State in ('CA', 'NV', 'OR', 'WA');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Mountain Standard Time') WHERE State in ('AZ', 'CO', 'ID', 'MT', 'NM', 'UT', 'WY');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Central Standard Time') WHERE State in ('AL', 'AR', 'IL', 'IA', 'KS', 'KY', 'LA', 'MN', 'MS', 'MO', 'NE', 'ND', 'OK', 'SD', 'TN', 'TX', 'WI');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Eastern Standard Time') WHERE State in ('CT', 'DE', 'DC', 'FL', 'GA', 'IN', 'ME', 'MD', 'MA', 'MI', 'NH', 'NJ','NY', 'NC', 'OH', 'PA', 'RI', 'SC', 'VT', 'VA', 'WV');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'Atlantic Standard Time') WHERE State in ('PR', 'USVI');
UPDATE dbo.Facility SET TimeZoneId = (SELECT Id FROM dbo.FacilityTimeZones WHERE Name = 'West Pacific Standard Time') WHERE State in ('GU', 'MP');

ALTER TABLE dbo.Facility DROP COLUMN TimeZone

---- SHAR-85

CREATE TABLE dbo.FacilityRecipients(
	Id INT IDENTITY PRIMARY KEY,
	Email NVARCHAR(254) NOT NULL,
	FacilityId INT NOT NULL,
	
	CONSTRAINT FK_FacilityRecipients_Facility FOREIGN KEY (FacilityId) REFERENCES dbo.Facility(ID) ON DELETE CASCADE
);

ALTER TABLE dbo.Facility ALTER COLUMN [State] nvarchar(MAX) NULL;
ALTER TABLE dbo.Facility ALTER COLUMN [Active] bit not NULL; 
ALTER TABLE dbo.Facility ALTER COLUMN [TimeZoneId] int not NULL;

---- SHAR-146

ALTER TABLE dbo.AuditTableColumnValue ADD ProgressNoteDate date NULL;

UPDATE dbo.AuditTableColumnValue SET ProgressNoteDate = convert(date, ProgressNoteTime) WHERE ProgressNoteTime IS NOT NULL;
ALTER TABLE dbo.AuditTableColumnValue ALTER COLUMN ProgressNoteTime time NULL;

-----Release 3
-----SHAR-135
DELETE FROM TrustedFEClient

ALTER TABLE TrustedFEClient
ADD UserID INT not null

ALTER TABLE [dbo].[TrustedFEClient]  WITH CHECK ADD  CONSTRAINT [FK_TrustedFEClient_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])

ALTER TABLE [dbo].[TrustedFEClient] CHECK CONSTRAINT [FK_TrustedFEClient_User]

-----SHAR-138
ALTER TABLE Facility
ADD SNFFacilityID INT null

ALTER TABLE Patient
ADD SNFPatientID nvarchar(255) null

ALTER TABLE Patient
ALTER COLUMN FacId int null

ALTER TABLE Patient
ALTER COLUMN PatientId int null

ALTER TABLE ProgressNote
ALTER COLUMN FacId INT NULL

ALTER TABLE ProgressNote
ALTER COLUMN ProgressNoteId INT NULL

ALTER TABLE ProgressNote
ADD SNFProgressNoteID BIGINT NULL

--SHAR-170
ALTER TABLE ProgressNote
ALTER COLUMN EffectiveDate DATETIME NOT NULL
ALTER TABLE [dbo].[TrustedFEClient] CHECK CONSTRAINT [FK_TrustedFEClient_User]


------SHAR-131

CREATE TABLE [dbo].[FormScheduleSettings](
	FormOrganizationId int PRIMARY KEY,
	ScheduleType int NOT NULL,
	Days nvarchar(max) NOT NULL,
	
	CONSTRAINT FK_FormScheduleSettings_FormOrganization FOREIGN KEY (FormOrganizationId) REFERENCES dbo.FormOrganization(ID) ON DELETE CASCADE,
);


ALTER TABLE dbo.FormOrganization ADD SettingType int NULL;

UPDATE dbo.FormOrganization SET SettingType = 1;

ALTER TABLE dbo.FormOrganization ALTER COLUMN SettingType int NOT NULL;


------SHAR-148

ALTER TABLE dbo.Audit ALTER COLUMN IncidentDateFrom datetime2(0) NULL;


--------Release 4
-----SHAR-149

UPDATE TrackerOptions SET FieldTypeId = 5 WHERE FieldTypeId = 7;

UPDATE FormField  SET FieldTypeID = 5 WHERE FieldTypeID = 7;

UPDATE FieldType SET Name = 'Toggle' WHERE ID = 5;

DELETE FROM FieldType WHERE ID = 7;


-----SHAR-173

INSERT INTO dbo.FieldType (ID, Name)
VALUES(8, 'Text area')


---- SHAR-231

ALTER TABLE dbo.Audit Add IsFilled bit NULL;

UPDATE dbo.Audit SET IsFilled = 1
WHERE ID IN (
SELECT a.ID 
FROM dbo.Audit a 
JOIN dbo.FormVersion fv ON a.FormVersionId = fv.Id AND a.Status > 1
JOIN dbo.Form f ON fv.FormId = f.ID AND f.AuditTypeID = 2) -- Criteria



--------Release 5
-----SHAR-84

ALTER TABLE dbo.Audit ADD DueDate datetime2(0) NULL;



--------Release 6

-------SHAR-183----------
CREATE TABLE [dbo].[Report](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[TableauUrl] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Report] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

INSERT INTO dbo.Report ([Name], TableauUrl)
VALUES ('Daily Fall Report','DailyFallReportACSPro/DailyFallReport')

INSERT INTO dbo.Report ([Name], TableauUrl)
VALUES ('Facilities monthly report - YTD','Facilitiesmonthlyreport-YTD_16554585234390/YTD')

INSERT INTO dbo.Report ([Name], TableauUrl)
VALUES ('Weekly Audit Compliance','WeeklyreportACSPro_16554578065180/AuditCompliance')


-----SHAR-185

CREATE TABLE [dbo].[Memos](
	Id int IDENTITY(1,1) PRIMARY KEY,
	UserId int NOT NULL,
	[Text] nvarchar(max) NOT NULL,
	CreatedDate datetime2(0) NOT NULL,
	ValidityDate datetime2(0) NULL,
	
	CONSTRAINT FK_Memos_User FOREIGN KEY (UserId) REFERENCES dbo.[User](ID) ON DELETE CASCADE,
);

CREATE TABLE [dbo].[OrganizationMemos](
	OrganizationId int NOT NULL,
	MemoId int NOT NULL,
	
	Constraint PK_Organizations_Memos Primary Key (OrganizationId, MemoId),
	
	CONSTRAINT FK_OrganizationMemos_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organization(ID) ON DELETE CASCADE,
	CONSTRAINT FK_OrganizationMemos_Memo FOREIGN KEY (MemoId) REFERENCES dbo.Memos(Id) ON DELETE CASCADE,
);

--------- SS-6

CREATE TABLE [dbo].[ReportRequests](
	Id int IDENTITY(1,1) PRIMARY KEY,
	UserId int NOT NULL,
	AuditType nvarchar(50) NOT NULL,
	OrganizationId int NOT NULL,
	FacilityId int NULL,
	FormId int NOT NULL,
	FromDate datetime2(0) NOT NULL,
	ToDate datetime2(0) NULL,
	RequestedTime datetime2(0) NOT NULL,
	GeneratedTime datetime2(0) NULL,
	[Report] nvarchar(500) NULL,
	[Exception] nvarchar(max) NULL,
	
	CONSTRAINT FK_ReportRequests_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organization(ID),
	CONSTRAINT FK_ReportRequests_User FOREIGN KEY (UserId) REFERENCES dbo.[User](ID) ON DELETE CASCADE,
	CONSTRAINT FK_ReportRequests_Facility FOREIGN KEY (FacilityId) REFERENCES dbo.Facility(ID) ON DELETE CASCADE,
	CONSTRAINT FK_ReportRequests_Form FOREIGN KEY (FormId) REFERENCES dbo.Form(ID) ON DELETE CASCADE,
);


CREATE NONCLUSTERED INDEX [idx_AuditID] ON [dbo].[AuditTableColumnValue]
(
	[AuditID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [idx_AuditFieldValue_AuditID] ON [dbo].[AuditFieldValue]
(
	[AuditID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
GO

--Sprint 16
--SHAR-257
CREATE TABLE [dbo].[AuditStatusHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AuditID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[Date] [datetime2](0) NOT NULL,
	[UserID] [int] NOT NULL,
 CONSTRAINT [PK_AuditStatusHistory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AuditStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_AuditStatusHistory_Audit] FOREIGN KEY([AuditID])
REFERENCES [dbo].[Audit] ([ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AuditStatusHistory] CHECK CONSTRAINT [FK_AuditStatusHistory_Audit]
GO

ALTER TABLE [dbo].[AuditStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_AuditStatusHistory_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])
GO

ALTER TABLE [dbo].[AuditStatusHistory] CHECK CONSTRAINT [FK_AuditStatusHistory_User]


--------- SHAR-141

CREATE TABLE [dbo].[AuditSettings](
	Id int IDENTITY(1,1) PRIMARY KEY,
	AuditId int NOT NULL,
	[Type] int NOT NULL,
	Value nvarchar(max) NULL,
	
	CONSTRAINT FK_AuditSettings_Audit FOREIGN KEY (AuditId) REFERENCES dbo.[Audit](ID) ON DELETE CASCADE,
);

ALTER TABLE [dbo].[AuditSettings] ADD CONSTRAINT UNX_AuditSettings UNIQUE(AuditId, [Type]);


---------- Sprint-17
---------- SHAR-296

CREATE TABLE [dbo].[UserActivities](
	Id int IDENTITY(1,1) PRIMARY KEY,
	UserId int NOT NULL,
	ActionType int NOT NULL,
	ActionTime [datetime2](0) NOT NULL,
	UserAgent nvarchar(max) NOT NULL,
	IP nvarchar(22) NOT NULL,
	
	CONSTRAINT FK_UserActivities_User FOREIGN KEY (UserId) REFERENCES dbo.[User](ID) ON DELETE CASCADE,
);


DELETE FROM dbo.Report
INSERT INTO dbo.Report ([Name], TableauUrl)
VALUES ('Daily Fall Report','DailyFallReportACSPro/DailyFallReport'),
('Facilities monthly report - YTD','Facilitiesmonthlyreport-YTD/YTD'),
('Weekly Audit Compliance','WeeklyreportACSPro/AuditCompliance'),
('EMR Report','EMRTrackerReportACSPro/EMRReport'),
('Compliance Percentage','WeeklyreportACSPro/CompliancePercentage'),
('Weekly Wound Report','WeeklyWoundReportACSPro/WeeklyWoundReport')



ALTER TABLE dbo.[Audit]
ADD State INT NULL

ALTER TABLE dbo.[Audit]
ADD LastUnarchivedDate DATETIME2(0) NULL

UPDATE dbo.[Audit]
SET State=1

ALTER TABLE dbo.[Audit]
ALTER COLUMN State INT NOT NULL

GO
CREATE PROCEDURE [dbo].[SetAuditStateToArchived]
AS
BEGIN
    UPDATE dbo.[Audit]
	SET
		[State]=2
	WHERE
		([Status]=6 AND [State]=1 AND DATEDIFF(DAY, SubmittedDate, GETDATE()) >= 120 AND LastUnarchivedDate IS NULL)
		OR
		([State]=1 AND LastUnarchivedDate IS NOT NULL AND DATEDIFF(DAY, LastUnarchivedDate, GETDATE()) >= 120)
END
GO
ALTER TABLE dbo.[Audit]
ADD LastUnarchivedDate DATETIME2(0) NULL
('Weekly Wound Report','WeeklyWoundReportACSPro/WeeklyWoundReport')



---------- Sprint-18
---------- SHAR-290

ALTER TABLE dbo.ReportRequests ADD Status int NULL;

UPDATE dbo.ReportRequests SET Status = 0 WHERE [Exception] IS NOT NULL;
UPDATE dbo.ReportRequests SET Status = 1 WHERE GeneratedTime IS NULL AND [Exception] IS NULL;
UPDATE dbo.ReportRequests SET Status = 2 WHERE GeneratedTime IS NOT NULL AND Report IS NOT NULL AND [Exception] IS NULL;

ALTER TABLE dbo.ReportRequests ALTER COLUMN Status int NOT NULL;


-------- SHAR-314

ALTER TABLE dbo.[User] ADD TimeZone nvarchar(50) NULL;

UPDATE dbo.[User] SET TimeZone = 'Singapore Standard Time';

ALTER TABLE dbo.[User] ALTER COLUMN TimeZone nvarchar(50) NOT NULL;



---------- Sprint-19
---------- SHAR-156

ALTER TABLE dbo.[Audit]
ADD LastDeletedDate DATETIME2(0) NULL


GO
CREATE PROCEDURE [dbo].[PurgeDeletedAudits] @NumberOfDays INT
AS
BEGIN
    DELETE FROM dbo.[Audit]
	WHERE
		[State]=3 AND DATEDIFF(DAY, [LastDeletedDate], GETDATE()) >= @NumberOfdays
END
GO

---------- SHARP-22 User Productivity
ALTER TABLE [dbo].[UserActivities]
    ADD [AuditId] INT CONSTRAINT [DEFAULT_UserActivities_AuditId] DEFAULT NULL NULL;


--------- SHARP-33 MDS

CREATE TABLE [dbo].[FormSection] (
    [ID]            INT           IDENTITY (1, 1) NOT NULL,
    [FormVersionId] INT           NOT NULL,
    [Name]          VARCHAR (MAX) NOT NULL,
    [Sequence]      INT           CONSTRAINT [DEFAULT_FormSection_Sequence] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_FormSection] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_FormSection_FormVersion] FOREIGN KEY ([FormVersionId]) REFERENCES [dbo].[FormVersion] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[FormGroup] (
    [ID]            INT           IDENTITY (1, 1) NOT NULL,
    [FormSectionId] INT           NOT NULL,
    [Name]          VARCHAR (MAX) NOT NULL,
    [Sequence]      INT           CONSTRAINT [DEFAULT_FormGroup_Sequence] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_FormGroup] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_FormGroup_FormSection] FOREIGN KEY ([FormSectionId]) REFERENCES [dbo].[FormSection] ([ID]) ON DELETE CASCADE
);

---------- SHARP-17: Dashboard Input
CREATE TABLE [dbo].[DashboardInputTable] (
    [ID]             INT           IDENTITY (1, 1) NOT NULL,
    [OrganizationId] INT           NOT NULL,
    [Name]           NVARCHAR (50) NULL,
    CONSTRAINT [PK_DashboardInputTable] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DashboardInputTable_Organization] FOREIGN KEY ([OrganizationId]) REFERENCES [dbo].[Organization] ([ID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[DashboardInputGroups] (
    [ID]      INT            IDENTITY (1, 1) NOT NULL,
    [TableID] INT            NOT NULL,
    [Name]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_DashboardInputGroups] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DashboardInputGroups_DashboardInputTable] FOREIGN KEY ([TableID]) REFERENCES [dbo].[DashboardInputTable] ([ID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[DashboardInputElement] (
    [ID]      INT            IDENTITY (1, 1) NOT NULL,
    [GroupID] INT            NULL,
    [Name]    NVARCHAR (50)  NULL,
    [Keyword] NVARCHAR (100) NULL,
    [FormID]  INT            NULL,
    CONSTRAINT [PK_DashboardInputElement] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DashboardInputElement_DashboardInputGroups] FOREIGN KEY ([GroupID]) REFERENCES [dbo].[DashboardInputGroups] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_DashboardInputElement_Form] FOREIGN KEY ([FormID]) REFERENCES [dbo].[Form] ([ID]) ON DELETE SET NULL
);

CREATE TABLE [dbo].[DashboardInputValues] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [ElementID]  INT      NOT NULL,
    [FacilityID] INT      NOT NULL,
    [Date]       DATETIME NOT NULL,
    [Value]      INT      CONSTRAINT [DEFAULT_DashboardInputValues_Value] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DashboardInputValues] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_DashboardInputValues_DashboardInputElement] FOREIGN KEY ([ElementID]) REFERENCES [dbo].[DashboardInputElement] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_DashboardInputValues_Facility] FOREIGN KEY ([FacilityID]) REFERENCES [dbo].[Facility] ([ID])
);

---------- SHARP-38 MDS Clinical Reviews
ALTER TABLE [dbo].[Form]
    ADD [DisableCompliance] INT CONSTRAINT [DEFAULT_Form_DisableCompliance] DEFAULT 0 NOT NULL,
        [AllowEmptyComment] INT CONSTRAINT [DEFAULT_Form_AllowEmptyComment] DEFAULT 0 NOT NULL;


--------- SHARP-9 Facility User Access
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFacility](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[FacilityID] [int] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserFacility] ADD  CONSTRAINT [PK_UserFacility] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserFacility]  WITH CHECK ADD  CONSTRAINT [FK_UserFacility_Facility] FOREIGN KEY([FacilityID])
REFERENCES [dbo].[Facility] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserFacility] CHECK CONSTRAINT [FK_UserFacility_Facility]
GO
ALTER TABLE [dbo].[UserFacility]  WITH CHECK ADD  CONSTRAINT [FK_UserFacility_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserFacility] CHECK CONSTRAINT [FK_UserFacility_User]
GO


------- SHARP-14 - Custom Keywords -----------
ALTER TABLE [dbo].[TableColumn] ADD [Hidden] INT NULL;


CREATE TABLE dbo.[KeywordTrigger] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	FormKeywordID int NOT NULL,
	KeywordID int NOT NULL,
	FormTriggerID int NOT NULL
	CONSTRAINT [FK_KeywordTrigger_FormKeywordID] FOREIGN KEY (FormKeywordID) REFERENCES [dbo].[Form] ([ID]) ON DELETE  NO ACTION,
	CONSTRAINT [FK_KeywordTrigger_TableColumn] FOREIGN KEY (KeywordID) REFERENCES [dbo].[TableColumn] ([ID]) ON DELETE  NO ACTION,
	CONSTRAINT [FK_KeywordTrigger_Form] FOREIGN KEY (FormTriggerID) REFERENCES [dbo].[Form] ([ID]) ON DELETE  NO ACTION,
);

CREATE TABLE dbo.[AuditTriggeredByKeyword] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	AuditID int NOT NULL,
	AuditTableColumnValueID int NOT NULL,
	CreatedAuditID int NOT NULL
	CONSTRAINT [FK_AuditTiggeredByKeyword_AuditID] FOREIGN KEY (AuditID) REFERENCES [dbo].[Audit] ([ID]) ON DELETE NO ACTION,
	CONSTRAINT [FK_AuditTiggeredByKeyword_AuditTableColumnValueID] FOREIGN KEY (AuditTableColumnValueID) REFERENCES [dbo].AuditTableColumnValue ([ID]) ON DELETE NO ACTION,
	CONSTRAINT [FK_AuditTiggeredByKeyword_CreatedAuditID] FOREIGN KEY (CreatedAuditID) REFERENCES [dbo].[Audit] ([ID]) ON DELETE NO ACTION,
);

ALTER TABLE <table> NOCHECK CONSTRAINT <constraint>



CREATE TABLE [dbo].[ReportAIContent] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [OrganizationID]  INT      NOT NULL,
    [FacilityID] INT      NULL,
	[SummaryAI]  nvarchar(max) NOT NULL,
	[Keywords] nvarchar(max) NOT NULL,
	[PdfFileName] nvarchar(max) NOT NULL,
	[ContainerName] nvarchar(50) NOT NULL,
	[AuditorName] nvarchar(255) NOT NULL,
	[AuditTime] nvarchar(50) NOT NULL,
	[AuditDate] nvarchar(50) NOT NULL,
	[FilteredDate] nvarchar(50) NOT NULL,
	[CreatedAt] datetime2(0) NOT NULL,
    CONSTRAINT [PK_ReportAIContent] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_ReportAIContent_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [dbo].[Organization] ([ID]) ,
    CONSTRAINT [FK_ReportAIContent_Facility] FOREIGN KEY ([FacilityID]) REFERENCES [dbo].[Facility] ([ID])
);

CREATE TABLE dbo.[ReportCategory] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	ReportCategoryName nvarchar(50) NOT NULL
);

INSERT INTO [dbo].[ReportCategory] ([ReportCategoryName]) VALUES ('Clinical')
INSERT INTO [dbo].[ReportCategory] ([ReportCategoryName]) VALUES ('PDPM')
INSERT INTO [dbo].[ReportCategory] ([ReportCategoryName]) VALUES ('Quality Measure')
INSERT INTO [dbo].[ReportCategory] ([ReportCategoryName]) VALUES ('QIP Reports')
INSERT INTO [dbo].[ReportCategory] ([ReportCategoryName]) VALUES ('MDS Reports')

CREATE TABLE dbo.[ReportRange] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	RangeName nvarchar(50) NOT NULL
);

INSERT INTO [dbo].[ReportRange] ([RangeName]) VALUES ('Daily')
INSERT INTO [dbo].[ReportRange] ([RangeName]) VALUES ('Weekly')
INSERT INTO [dbo].[ReportRange] ([RangeName]) VALUES ('Bi-weekly')
INSERT INTO [dbo].[ReportRange] ([RangeName]) VALUES ('Monthly')

CREATE TABLE dbo.[ReportType] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	TypeName nvarchar(50) NOT NULL
);

INSERT INTO [dbo].[ReportType] ([TypeName]) VALUES ('PDF')
INSERT INTO [dbo].[ReportType] ([TypeName]) VALUES ('Excel')


CREATE TABLE dbo.[PortalReport] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(max) NOT NULL,
	ReportType int NOT NULL,
	ReportCategory int NOT NULL,
	OrganizationID int NOT NULL,
	FacilityID int NOT NULL,
	AuditID int ,
	AuditTypeID int ,
	StorageContainerName nvarchar(max),
	StorageReportName nvarchar(max),
	StorageURL nvarchar(max),
	[CreatedAt] datetime2(0) NOT NULL,
	[CreatedByUserID] int NOT NULL,

    CONSTRAINT [FK_PortalReport_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [dbo].[Organization] ([ID]) ,
    CONSTRAINT [FK_PortalReport_FacilityID] FOREIGN KEY ([FacilityID]) REFERENCES [dbo].[Facility] ([ID]),
	CONSTRAINT [FK_PortalReport_AuditID] FOREIGN KEY ([AuditID]) REFERENCES [dbo].[Audit] ([ID]),
	CONSTRAINT [FK_PortalReport_ReportType] FOREIGN KEY ([ReportType]) REFERENCES [dbo].[ReportType] ([ID]),
	CONSTRAINT [FK_PortalReport_ReportCategory] FOREIGN KEY ([ReportCategory]) REFERENCES [dbo].[ReportCategory] ([ID]),
	CONSTRAINT [FK_PortalReport_CreatedByUserID] FOREIGN KEY ([CreatedByUserID]) REFERENCES [dbo].[User] ([ID]),
	CONSTRAINT [FK_PortalReport_AuditTypeID] FOREIGN KEY ([AuditTypeID]) REFERENCES [dbo].[AuditType] ([ID])
);

CREATE TABLE dbo.[SendReportToUser] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	UserID int ,
	UserEmail  nvarchar(max) NOT NULL,
	PortalReportID int NOT NULL,
	CreatedAt datetime2(0) NOT NULL,
	Token NVARCHAR(70),
	FacilityID int NOT NULL,
	Status bit,
	SendByUserID int NOT NULL

    CONSTRAINT [FK_SendReportToUser_SendByUserID] FOREIGN KEY (SendByUserID) REFERENCES [dbo].[User] ([ID]),

	CONSTRAINT [FK_SendReportToUser_PortalReportID] FOREIGN KEY (PortalReportID) REFERENCES [dbo].[PortalReport] ([ID]),

	CONSTRAINT [FK_SendReportToUser_PortalFacilityID] FOREIGN KEY (FacilityID) REFERENCES [dbo].[Facility] ([ID])
);



ALTER TABLE [dbo].[PortalReport] ADD [ReportRequestID] INT;

--RS 08/27/2024 Added index to improve performance of UserACtivity table related to ticket ACS-163
CREATE NONCLUSTERED INDEX  idx_UserActivity_UserIdActionTime
ON [dbo].[UserActivities] ([UserId],[ActionTime])
INCLUDE ([ActionType],[UserAgent],[IP],[AuditId])

----Ramessh 08/22/2024 Added DeletedByUserID in Audit ----

ALTER TABLE dbo.[Audit] ADD DeletedByUserID INT NULL;
GO
With tmp
as
(
	Select * from [Audit] a where  a.[State] = 3 and a.DeletedByUserID is null
)

UPDATE tmp
SET DeletedByUserID = ua.UserId
FROM tmp
join (
	
	Select * from (
	Select UserId,  AuditId, RANK() over(partition by AuditId order by ActionTime DESC) as RowNo
	From UserActivities(nolock)
	where ActionType = 7 
	) t where t.RowNo=1
) ua on ua.AuditId = tmp.ID;



ALTER TABLE [dbo].[Form] ADD [UseHighAlert] BIT NOT NULL DEFAULT 0;


CREATE TABLE dbo.[HighAlertCategory] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(50) NOT NULL
);
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('New or worsening wound')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Falls with Major Injury')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Death')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Elopement Episode')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Abuse')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Medication Error')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Medication Unavailable/Awaiting Medications')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Others')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('911')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Against medical Advice (AMA)')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('High risk for elopement')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('High risk for wandering')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Risk for Elopement')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Risk for Wandering')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Unknown origin')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Altercationy')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Behavior')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Choking')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Wandering Episode')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Smoking')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Emergency Department Visitr')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('New Indwelling Catheter')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('New Antianxiety/Hypnotic Use')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('New Physical Restraint')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('Significant Weight Loss')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('UTI')
INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('New Antipsychotic medication')

INSERT INTO [dbo].[HighAlertCategory] ([Name]) VALUES ('ER Transfer')
---- Add Status in ReportAIContent ----

ALTER TABLE dbo.[ReportAIContent] ADD [Status] INT NULL;
GO

UPDATE dbo.[ReportAIContent] SET [Status] = 6

CREATE TABLE dbo.[HighAlertStatus] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(50) NOT NULL
);
INSERT INTO [dbo].[HighAlertStatus] ([Name]) VALUES ('Open')
INSERT INTO [dbo].[HighAlertStatus] ([Name]) VALUES ('Closed')

CREATE TABLE dbo.[HighAlertAuditValue]  (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Description  nvarchar(max) NOT NULL,
	Notes nvarchar(max) ,
	AuditID  int ,
	AuditTableColumnValueID int ,
	ReportAiID int,
	ReportName nvarchar(max) NOT NULL,
	HighAlertCategoryID int NOT NULL,
	CreatedByUserId int NOT NULL,
	CreatedAt datetime2(0) NOT NULL,	
	CONSTRAINT  [FK_HighAlertAuditValue_AuditID]   FOREIGN KEY (AuditID)  REFERENCES [dbo].[Audit] ([ID]) ,
	CONSTRAINT [FK_HighAlertAuditValue_HighAlertCategoryID] FOREIGN KEY (HighAlertCategoryID) REFERENCES [dbo].[HighAlertCategory] ([ID])
);
ALTER TABLE [dbo].[AuditTableColumnValue] ADD [HighAlertID] int;
ALTER TABLE [dbo].[HighAlertAuditValue] ADD [ReportName] nvarchar(max);
CREATE TABLE dbo.[HighAlertStatusHistory]  (
	ID int IDENTITY(1,1) PRIMARY KEY,
	HighAlertAuditValueID int NOT NULL,
	HighAlertStatusID int NOT NULL,
	ChangedBy nvarchar(max),
	UserNotes nvarchar(max),
	CreatedAt datetime2(0) NOT NULL,
	CONSTRAINT [FK_HighAlertStatusHistory_HighAlertStatusID] FOREIGN KEY (HighAlertStatusID) REFERENCES [dbo].[HighAlertStatus] ([ID]),
	CONSTRAINT [FK_HighAlertStatusHistory_HighAlertAuditValueID] FOREIGN KEY (HighAlertAuditValueID) REFERENCES [dbo].[HighAlertAuditValue] ([ID]),
);


CREATE TABLE dbo.[PortalFeature]  (

	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(max) NOT NULL,
);
INSERT INTO [dbo].[PortalFeature] ([Name]) VALUES ('High Alerts')


CREATE TABLE dbo.[OrganizationPortalFeature]  (

	ID int IDENTITY(1,1) PRIMARY KEY,
	OrganizationID int NOT NULL,
	PortalFeatureID int NOT NULL,
	Available  BIT NOT NULL DEFAULT 0,
	CONSTRAINT [FK_OrganizationPortalFeature_OrganizationID] FOREIGN KEY (OrganizationID) REFERENCES [dbo].[Organization] ([ID]),
	CONSTRAINT [FK_OrganizationPortalFeature_PortalFeatureID] FOREIGN KEY (PortalFeatureID) REFERENCES [dbo].[PortalFeature] ([ID]),

);
---- 09/26/2024 ACS-175 Added SubmittedDate and change datatype of AuditDate in ReportAIContent ----

ALTER TABLE dbo.[ReportAIContent] ADD [SubmittedDate] DATETIME2 NULL;
GO

UPDATE dbo.[ReportAIContent] SET SubmittedDate = CreatedAt WHERE SubmittedDate IS NULL and Status=6
GO

ALTER TABLE dbo.[ReportAIContent] ALTER COLUMN [AuditDate] DATE;
GO


---- 10/09/2024 ACS-224 Added SentForApprovalDate in ReportAIContent ----

ALTER TABLE dbo.[ReportAIContent] ADD [SentForApprovalDate] DATETIME2(0) NULL;
GO
Update ReportAIContent
set SentForApprovalDate = (case when SubmittedDate is not null and SubmittedDate > CreatedAt then SubmittedDate else CreatedAt end)
where SentForApprovalDate is null and Status >=2

GO
---- 10/10/2024 ACS-224 Added SentForApprovalDate in Audit ----

ALTER TABLE dbo.[Audit] ADD [SentForApprovalDate] DATETIME2(0) NULL;
GO
Update Audit
Set SentForApprovalDate  = t.[Date]
From Audit a
INNER JOIN (Select * from (
Select AuditId, Date, ROW_NUMBER( ) over (Partition by AuditId order by [Date] Desc)  as RowNum from AuditStatusHistory where Status=2
)
where RowNum =1) t on t.AuditId = a.Id
where Status >=2 and Year(a.[SubmittedDate]) = 2024

---- 10/10/2024 ACS-228 Added State in ReportAIContent ----

ALTER TABLE dbo.[ReportAIContent] ADD [State] INT NULL;
GO

UPDATE dbo.[ReportAIContent] SET [State] = 1 WHERE [State] IS NULL
GO

--------------------------------------

CREATE TABLE [dbo].[AuditAIReport] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [OrganizationID]  INT      NOT NULL,
    [FacilityID] INT      NULL,
	[SummaryAI]  varbinary(max) NOT NULL,
	[Keywords] nvarchar(max) NOT NULL,
	[PdfFileName] nvarchar(max) NOT NULL,
	[AuditorName] nvarchar(255) NOT NULL,
	[AuditTime] nvarchar(50) NOT NULL,
	[AuditDate] nvarchar(50) NOT NULL,
	[State] INT ,
	[Status] INT ,
	[SubmittedDate] DATETIME2 NULL,
	[FilteredDate] nvarchar(50) NOT NULL,
	[CreatedAt] datetime2(0) NOT NULL,
	[SentForApprovalDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_AuditAIReport] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AuditAIReport_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [dbo].[Organization] ([ID]) ,
    CONSTRAINT [FK_AuditAIReport_Facility] FOREIGN KEY ([FacilityID]) REFERENCES [dbo].[Facility] ([ID])
);
---------------[HighAlertPotentialAreas]-----------------------
ALTER TABLE [dbo].[HighAlertCategory] ADD [Active] BIT NOT NULL DEFAULT 1;


CREATE TABLE [dbo].[HighAlertPotentialAreas] (
	[ID]         INT      IDENTITY (1, 1) NOT NULL,
	Name nvarchar(50) NOT NULL,
	CONSTRAINT [PK_HighAlertPotentialAreas] PRIMARY KEY CLUSTERED ([ID] ASC)
);

INSERT INTO [dbo].[HighAlertPotentialAreas] ([Name]) VALUES ('Five-Star')
INSERT INTO [dbo].[HighAlertPotentialAreas] ([Name]) VALUES ('QIP')
INSERT INTO [dbo].[HighAlertPotentialAreas] ([Name]) VALUES ('QM')
INSERT INTO [dbo].[HighAlertPotentialAreas] ([Name]) VALUES ('Survey')

CREATE TABLE [dbo].[HighAlertCategoryToPotentialAreas] (
	[ID]         INT      IDENTITY (1, 1) NOT NULL,
	[HighAlertCategoryID]  INT NOT NULL,
	[HighAlertPotentialAreasID] INT NOT NULL,
	CONSTRAINT [PK_HighAlertCategoryToPotentialAreas] PRIMARY KEY CLUSTERED ([ID] ASC),
	CONSTRAINT [FK_HighAlertCategoryToPotentialAreas_HighAlertPotentialAreasID] FOREIGN KEY ([HighAlertPotentialAreasID]) REFERENCES [dbo].[HighAlertPotentialAreas] ([ID]) ,
);
---------------New or Worsening Wound

INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (1,1)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (1,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (1,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (1,4)

---------------Falls with Major Injury

INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (2,1)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (2,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (2,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (2,4)


---------------death
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (3,4)
---------------Elopement Episode
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (4,4)
----------------------Abuse
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (5,4)
---------------------Medication Error
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (6,4)
------------Medication Unavailable/Awaiting Medications
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (7,4)

------------911
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (9,4)
-------------------Against medical Advice (AMA)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (10,4)
-------------------------Risk for Elopement
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (13,4)
------------------------------------Risk for Wandering
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (14,4)
---------------------Unknown origin
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (15,4)
-----------------------------Altercation
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (16,4)

-----------------------------Behavior
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (17,4)

----------------------------Choking
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (18,4)

----------------------------Wandering Episode
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (19,4)
--------------Smoking

INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (20,4)

----------------------Emergency Department Visit
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (21,1)

-------------------New Indwelling Catheter
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (23,1)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (23,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (23,4)

---------------- New Antianxiety/Hypnotic Use

INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (25,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (25,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (25,4)

---------------- New Physical Restraint

INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (26,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (26,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (26,4)

----Significant Weight Loss
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (27,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (27,4)

-----------------UTI
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (28,1)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (28,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (28,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (28,4)

--------------New Antipsychotic medication
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (29,1)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (29,2)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (29,3)
INSERT INTO [dbo].[HighAlertCategoryToPotentialAreas] ([HighAlertCategoryID],[HighAlertPotentialAreasID]) VALUES (29,4)


---------------------------- portal user management -----------------------
ALTER TABLE dbo.[AspNetRoles] ADD [SiteId] INT NOT NULL DEFAULT 1
INSERT INTO AspNetRoles (Id, Name, NormalizedName,SiteId) VALUES(5, 'User', 'USER',2);
INSERT INTO AspNetRoles (Id, Name, NormalizedName,SiteId) VALUES(6, 'Super User', 'SUPERUSER',2);
CREATE TABLE [dbo].[Sites] (
	[ID]         INT      IDENTITY (1, 1) NOT NULL PRIMARY KEY,
	[Name]  varchar(50) NOT NULL,
)
INSERT INTO [Sites] (ID,[Name]) Values (1,'Sharp')
INSERT INTO [Sites] (ID,[Name]) Values (2,'Client Portal')

---- ACS-300 Added LegalName in Facility ----
ALTER TABLE [dbo].[Facility] ADD [LegalName] nvarchar(max) NULL;
go
UPDATE [dbo].[Facility] SET [LegalName] = [Name]

ALTER TABLE dbo.[User] ADD [SiteID] INT NOT NULL DEFAULT 1;
ALTER TABLE dbo.[User] ADD [Position] nvarchar(255);


ALTER TABLE dbo.[Organization] ADD [OperatorName] nvarchar(255);
ALTER TABLE dbo.[Organization] ADD [OperatorEmail] nvarchar(255);


--ACS-311
---- Added tabs in Productivity report ----
ALTER TABLE dbo.[UserActivities] ADD [UpdatedUserId] INT;

--ACS-342 - Log for failed login attempts require 'UserId' nullable
ALTER TABLE [dbo].[UserActivities] ALTER COLUMN [UserId] INT NULL;
GO
ALTER TABLE [dbo].[UserActivities] ADD [LoginUsername] NVARCHAR(1022) NULL;
GO

---- ACS-314 - check issue with productivity report on production ----
IF EXISTS (SELECT 	* FROM sysobjects WHERE name = 'GetUserActivities')
	DROP PROCEDURE dbo.GetUserActivities
GO

CREATE PROCEDURE [dbo].[GetUserActivities]
@userIds VARCHAR(MAX),
@fromDate DATE,
@toDate DATE
AS
BEGIN

	--User Activities
	SELECT 
		ua.[Id],
		ua.[UserId],
		ua.[AuditId],
		ua.[ActionType],
		ua.[ActionTime],
		ua.[UserAgent],
		ua.[IP],
		ua.[LoginUsername],
		u.[FullName] AS UserName,
		[a0].[Name] AS AuditType,
		f.[Name] AS AuditName,
		uu.[Id] AS UpdatedUserId,
		uu.[FullName] AS UpdatedUserName
	FROM [UserActivities] AS ua(nolock)
	JOIN [User] AS u(nolock) ON ua.[UserId] = u.[ID]
	LEFT JOIN [Audit] AS a(nolock) ON ua.[AuditId] = a.[Id]
	LEFT JOIN [FormVersion](nolock) AS fv ON a.[FormVersionId] = fv.[Id]
	LEFT JOIN [Form] AS f(nolock) ON fv.[FormId] = f.[ID]
	LEFT JOIN [AuditType] AS [a0](nolock) ON f.[AuditTypeID] = [a0].[ID]
	LEFT JOIN [User] AS uu(nolock) ON ua.[UpdatedUserId] = uu.[ID]
	WHERE (@userIds = 'All' OR EXISTS(SELECT 1 FROM string_split(@userIds, ',') AS ss WHERE ss.[value] = ua.[UserId]))
	AND Cast(ua.[ActionTime]  as DATE) between @fromDate AND @toDate
	UNION ALL
	SELECT
		ua.[Id],
		NULL,
		NULL,
		ua.[ActionType],
		ua.[ActionTime],
		ua.[UserAgent],
		ua.[IP],
		ua.[LoginUsername],
		'' AS UserName,
		'' AS AuditType,
		'' AS AuditName,
		NULL AS UpdatedUserId,
		'' AS UpdatedUserName
	FROM [UserActivities] AS ua(nolock)
	WHERE Cast(ua.[ActionTime]  as DATE) between @fromDate AND @toDate
	AND ua.[ActionType] = 22 AND ua.UserId IS NULL
	Order by [ActionTime]
	
	--Active Users
	SELECT
		u.ID,
		u.FirstName,
		u.LastName,
		u.Email,
		u.FullName,
		u.TimeZone
	FROM [User] u(nolock)
	WHERE u.[Status] = 1 and u.[SiteId] = 1

	-------- Change related to ticket ACS-332 -------------
	--User Audit Log
	SELECT 
		u.FullName AS SubmittedByUser,
		a.ID,
		f.[Name],
		a.[Status],
		[dbo].GetMinuteToDays(a.SubmittedDate, SFAHistory.[Date]) AS 'AuditorsTime',
		[dbo].GetMinuteToDays(a.SubmittedDate, SubHistory.[Date]) AS 'Duration'
	FROM [Audit] AS a(nolock)
	JOIN [User] AS u(nolock) ON a.SubmittedByUserID = u.ID
	JOIN FormVersion AS fv(nolock) ON a.FormVersionId = fv.Id
	JOIN Form AS f(nolock) ON fv.FormId = f.Id
	OUTER APPLY(SELECT TOP 1 ashSFA.[Date] FROM AuditStatusHistory AS ashSFA(nolock)
									  WHERE a.ID = ashSFA.AuditID AND ashSFA.[Status] = 2
									  ORDER BY ashSFA.[Date] asc) SFAHistory
	OUTER APPLY(SELECT TOP 1 ashSub.[Date] FROM AuditStatusHistory AS ashSub(nolock) 
									  WHERE a.ID = ashSub.AuditID AND ashSub.[Status] = 6
									  ORDER BY ashSub.[Date] asc) SubHistory
	WHERE (@userIds = 'All' OR EXISTS(SELECT 1 FROM string_split(@userIds, ',') AS ss WHERE ss.[value] = a.SubmittedByUserID))
	AND (CONVERT(date, a.SubmittedDate) BETWEEN @fromDate AND @toDate)


	--Audit Summary Count
	SELECT u.FullName, COUNT(1) AS 'SentForApprovalCount'
	FROM AuditStatusHistory AS ashSFA(nolock)
	JOIN [User] AS u(nolock) ON ashSFA.UserID = u.ID
	WHERE ashSFA.[Status] = 2
	AND (@userIds = 'All' OR EXISTS(SELECT 1 FROM string_split(@userIds, ',') AS ss WHERE ss.[value] = ashSFA.UserID))
	AND (CONVERT(date, ashSFA.[Date]) BETWEEN @fromDate AND @toDate)
	GROUP BY u.FullName


END
GO

-------- Change related to ticket ACS-332 -------------
--Indexes for optimizing Stored Procedure - GetUserActivities
CREATE NONCLUSTERED INDEX idx_AuditStatusHistory_AuditIDStatusDate
ON [dbo].[AuditStatusHistory] ([AuditID],[Status])
INCLUDE ([Date])
GO
CREATE NONCLUSTERED INDEX idx_AuditStatusHistory_StatusDateUserID
ON [dbo].[AuditStatusHistory] ([Status],[Date])
INCLUDE ([UserID])
GO

--Function to use in Stored Procedure - GetUserActivities
IF EXISTS (SELECT 	* FROM sysobjects WHERE name = 'GetMinuteToDays')
	DROP FUNCTION dbo.GetMinuteToDays
GO

CREATE FUNCTION [dbo].[GetMinuteToDays] (@startDate DATETIME, @endDate DATETIME)
RETURNS VARCHAR(100)
AS
BEGIN

	DECLARE @result VARCHAR(100) = '';

	IF (@startDate IS NULL OR @endDate IS NULL)
	BEGIN
		SET @result = '';
	END
	ELSE
	BEGIN

		DECLARE @milliseconds BIGINT = ABS(DATEDIFF_BIG(MILLISECOND, @startDate, @endDate));
		DECLARE @minutes BIGINT = ROUND((@milliseconds / 1000) / 60, 0);

		DECLARE @days BIGINT = Floor(@minutes / 1440),
		@hours BIGINT = Floor((@minutes % 1440) / 60),
		@remainingMin BIGINT = Floor(@minutes % 60);

		IF (@days = 0 AND @hours = 0 AND @remainingMin = 0)
		BEGIN
			SET @result = '0 minute(s)';
		END
		ELSE IF (@days = 0 AND @hours = 0 AND @remainingMin != 0)
		BEGIN
			SET @result = CONVERT(VARCHAR(50), @remainingMin) + ' minute(s)';
		END
		ELSE IF (@days = 0 AND @hours != 0)
		BEGIN
			IF (@remainingMin = 0)
			BEGIN
			  SET @result = CONVERT(VARCHAR(50), @hours) + ' hour(s)';
			END
			SET @result = CONVERT(VARCHAR(50), @hours) + ' hour(s) and ' + CONVERT(VARCHAR(50), @remainingMin) + ' minute(s)';
		END
		ELSE IF (@days != 0)
		BEGIN

		  IF (@hours = 0 AND @remainingMin = 0)
		  BEGIN
			SET @result = CONVERT(VARCHAR(50), @days) + ' day(s)';
		  END
		  ELSE IF (@hours != 0 AND @remainingMin = 0)
		  BEGIN
			SET @result = CONVERT(VARCHAR(50), @days) + ' day(s) and ' + CONVERT(VARCHAR(50), @hours) + ' hour(s)';
		  END
		  ELSE IF (@hours = 0 AND @remainingMin != 0)
		  BEGIN
			SET @result = CONVERT(VARCHAR(50), @days) + ' day(s) and ' + CONVERT(VARCHAR(50), @remainingMin) + ' minute(s)';
		  END
		  SET @result = CONVERT(VARCHAR(50), @days) + ' day(s), ' + CONVERT(VARCHAR(50), @hours) + ' hour(s) and ' + CONVERT(VARCHAR(50), @remainingMin) + ' minute(s)';

		END
	END

	RETURN @result;

END


------------- improve [InsertAuditAIReport]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertAuditAIReport]
    @OrganizationId INT,
    @FacilityId INT = NULL,
    @SummaryAI VARBINARY(MAX),
    @Keywords NVARCHAR(MAX),
    @PdfFileName NVARCHAR(255),
    @AuditorName NVARCHAR(255),
    @AuditTime NVARCHAR(50),
    @AuditDate NVARCHAR(50),
    @FilteredDate NVARCHAR(50),
    @CreatedAt DATETIME,
    @Status INT,
    @SubmittedDate DATETIME = NULL,
    @SentForApprovalDate DATETIME = NULL,
    @State INT,
    @InsertedId INT OUTPUT  -- Add OUTPUT parameter
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert data without compression
    INSERT INTO AuditAIReport (
        OrganizationId, FacilityId, SummaryAI, Keywords, PdfFileName, 
        AuditorName, AuditTime, AuditDate, FilteredDate, CreatedAt, Status, 
        SubmittedDate, SentForApprovalDate, State
    )
    VALUES (
        @OrganizationId, @FacilityId, @SummaryAI, @Keywords, @PdfFileName, 
        @AuditorName, @AuditTime, @AuditDate, @FilteredDate, @CreatedAt, @Status, 
        @SubmittedDate, @SentForApprovalDate, @State
    );

    -- Return the inserted ID
    SET @InsertedId = SCOPE_IDENTITY();
END;
GO



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateAuditAIReport]
    @Id INT,
    @OrganizationId INT = NULL,
    @FacilityId INT = NULL,
    @SummaryAI VARBINARY(MAX) = NULL,
    @Keywords NVARCHAR(MAX) = NULL,
    @PdfFileName NVARCHAR(255) = NULL,
    @AuditorName NVARCHAR(255) = NULL,
    @AuditTime NVARCHAR(50) = NULL,
    @AuditDate NVARCHAR(50) = NULL,
    @FilteredDate NVARCHAR(50) = NULL,
    @CreatedAt DATETIME = NULL,
    @Status INT = NULL,
    @SubmittedDate DATETIME = NULL,
    @SentForApprovalDate DATETIME = NULL,
    @State INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Update the record, only modifying columns that are not NULL
    UPDATE AuditAIReport
    SET 
        OrganizationId = ISNULL(@OrganizationId, OrganizationId),
        FacilityId = ISNULL(@FacilityId, FacilityId),
        SummaryAI = CASE WHEN @SummaryAI IS NOT NULL THEN @SummaryAI ELSE SummaryAI END,
        Keywords = ISNULL(@Keywords, Keywords),
        PdfFileName = ISNULL(@PdfFileName, PdfFileName),
        AuditorName = ISNULL(@AuditorName, AuditorName),
        AuditTime = ISNULL(@AuditTime, AuditTime),
        AuditDate = ISNULL(@AuditDate, AuditDate),
        FilteredDate = ISNULL(@FilteredDate, FilteredDate),
        CreatedAt = ISNULL(@CreatedAt, CreatedAt),
        Status = ISNULL(@Status, Status),
        SubmittedDate = ISNULL(@SubmittedDate, SubmittedDate),
        SentForApprovalDate = ISNULL(@SentForApprovalDate, SentForApprovalDate),
        State = ISNULL(@State, State)
    WHERE Id = @Id;
END;

GO


ALTER TABLE dbo.[Organization] ADD [OperatorName] nvarchar(255);
ALTER TABLE dbo.[Organization] ADD [OperatorEmail] nvarchar(255);


GO


CREATE TABLE [dbo].[AuditAIReportV2] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [OrganizationID]  INT      NOT NULL,
    [FacilityID] INT      NULL,
	[PdfFileName] nvarchar(max) NOT NULL,
	[AuditorName] nvarchar(255) NOT NULL,
	[AuditTime] nvarchar(50) NOT NULL,
	[AuditDate] nvarchar(50) NOT NULL,
	[State] INT ,
	[Status] INT ,
	[SubmittedDate] DATETIME2 NULL,
	[FilteredDate] nvarchar(50) NOT NULL,
	[CreatedAt] datetime2(0) NOT NULL,
	[SentForApprovalDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_AuditAIReportV2] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AuditAIReportV2_OrganizationID] FOREIGN KEY ([OrganizationID]) REFERENCES [dbo].[Organization] ([ID]) ,
    CONSTRAINT [FK_AuditAIReportV2_FacilityID] FOREIGN KEY ([FacilityID]) REFERENCES [dbo].[Facility] ([ID])
);

CREATE TABLE [dbo].[AuditAIPatientPdfNotes] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [AuditAIReportV2ID]  INT      NOT NULL,
	[PatientName] nvarchar(max)  NULL,
	[PatientId] nvarchar(max) ,
	[PdfNotes] varbinary(max)  NULL,
	[DateTime] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditAIPatientPdfNotes] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AuditAIPatientPdfNotes_AuditAIReportV2ID] FOREIGN KEY ([AuditAIReportV2ID]) REFERENCES [dbo].[AuditAIReportV2] ([ID]) ,

);

CREATE TABLE [dbo].[AuditAIKeywordSummary] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [AuditAIPatientPdfNotesID]  INT      NOT NULL,
	[Keyword] nvarchar(max)  NULL,
	[Summary] nvarchar(max)  NULL,
	[Accept] bit default 0
    CONSTRAINT [PK_AuditAIKeywordSummary] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AuditAIKeywordSummary_AuditAIPatientPdfNotesID] FOREIGN KEY ([AuditAIPatientPdfNotesID]) REFERENCES [dbo].[AuditAIPatientPdfNotes] ([ID]) ,
);



---- ACS-302 - create daily audit summary report in client portal ----
IF EXISTS (SELECT
			*
		FROM sysobjects
		WHERE Name = 'GetAuditSummary')
	DROP PROCEDURE dbo.GetAuditSummary
GO
CREATE PROCEDURE [dbo].GetAuditSummary
@facilityId INT,
@fromDate DATE = NULL,
@toDate DATE = NULL
AS
BEGIN

IF((@fromDate IS NULL OR YEAR(@fromDate) = 1900) AND (@toDate IS NULL OR YEAR(@toDate) = 1900))
BEGIN
	set @fromDate = CAST(GETDATE() AS DATE)
	set @toDate = CAST(GETDATE() AS DATE)
END
ELSE IF ((@fromDate IS NULL OR YEAR(@fromDate) = 1900) AND (@toDate IS NOT NULL AND YEAR(@toDate) <> 1900))
BEGIN
	set @fromDate = @toDate
END
ELSE IF ((@toDate IS NULL OR YEAR(@toDate) = 1900) AND (@fromDate IS NOT NULL AND YEAR(@fromDate) <> 1900))
BEGIN
	set @toDate = @fromDate
END


CREATE TABLE #tempTable(
	audit_name VARCHAR(MAX),
	NumberOfAudits INT,
	TotalCompliance FLOAT,
	NonCompliantQuestion VARCHAR(MAX),
	NonCompliantResident VARCHAR(MAX),
)
	

insert into #tempTable
    SELECT
        f.name as audit_name,
		COUNT(f.name) OVER (PARTITION BY f.name) AS NumberOfAudits,
        a.TotalCompliance,
		CASE 
			WHEN c.[Value] IN ('NO', 'NP', 'NON', 'NOI', 'No0', 'NO`', 'no', 'NIO', 'n/o', 'N', 'MO', '`NO')
				 AND NOT (
					((f.name LIKE '%Admission Audit%' OR f.name LIKE '%ADMISSION/READMISSION AUDIT%') 
					 AND (tc.name LIKE '%Nursing Admission/Readmission assessment indicate that resident has wounds on admission%' 
						  OR tc.name LIKE '%Nursing Admission/Readmission evaluation indicate that resident has wounds on admission%'))
					OR 
					((f.name LIKE '%Fall Audit%' OR f.name LIKE '%SUPPLEMENTAL AUDIT (Fall)%' OR f.name LIKE '%Supplemental Audit - Fall%') 
					 AND (tc.name LIKE '%If Fall resulted in Minor Injury%' 
						  OR tc.name LIKE '%If Fall resulted in Major Injury%'))
				 )
			THEN tc.name
			ELSE NULL 
		END AS NonCompliantQuestion,
		CASE 
			WHEN c.[Value] IN ('NO', 'NP', 'NON', 'NOI', 'No0', 'NO`', 'no', 'NIO', 'n/o', 'N', 'MO', '`NO')
				 AND NOT (
					((f.name LIKE '%Admission Audit%' OR f.name LIKE '%ADMISSION/READMISSION AUDIT%') 
					 AND (tc.name LIKE '%Nursing Admission/Readmission assessment indicate that resident has wounds on admission%' 
						  OR tc.name LIKE '%Nursing Admission/Readmission evaluation indicate that resident has wounds on admission%'))
					OR 
					((f.name LIKE '%Fall Audit%' OR f.name LIKE '%SUPPLEMENTAL AUDIT (Fall)%') 
					 AND (tc.name LIKE '%If Fall resulted in Minor Injury%' 
						  OR tc.name LIKE '%If Fall resulted in Major Injury%'))
				 )
			THEN a.ResidentName
			ELSE NULL
		END AS NonCompliantResident
    FROM audit a
    LEFT JOIN FormVersion fv ON a.FormVersionId = fv.id
    LEFT JOIN form f ON fv.formid = f.id
    LEFT JOIN facility fac ON fac.id = a.facilityid
    LEFT JOIN organization org ON org.id = fac.organizationid
    LEFT JOIN AuditTableColumnValue c ON a.id = c.AuditID
    LEFT JOIN TableColumn tc ON c.TableColumnID = tc.ID
    WHERE 
		a.FacilityId = @facilityId
		AND CAST(a.SubmittedDate AS DATE) >= @fromDate    
		AND CAST(a.SubmittedDate AS DATE) <= @toDate  
        AND
		(f.audittypeid = 2 OR (f.AuditTypeID = 3 AND f.id IN (
            SELECT DISTINCT f.id
            FROM Audit a
            LEFT JOIN FormVersion fv ON a.FormVersionId = fv.id
            LEFT JOIN form f ON fv.formid = f.id
            LEFT JOIN AuditTableColumnValue c ON a.id = c.AuditID
            LEFT JOIN TableColumn tc ON a.FormVersionId = tc.FormVersionId
            INNER JOIN TrackerOptions tro ON tro.TableColumnId = tc.ID
            WHERE tro.Compliance = 1
        )))
        AND f.IsActive = 1
        AND a.STATUS = 6
        AND a.SubmittedDate >= '1 January 2023'

	

SELECT
    Convert(varchar(max), a.audit_name) AS TypeOfAudit,
	Convert(varchar(max), NumberOfAudits) AS NumberOfAudits,
    ISNULL(Convert(varchar(max), b.AvgCompliance), '') AS CompliancePercentage,
	ISNULL(Convert(varchar(max), a.NonCompliantQuestion), '') AS NonCompliantQuestion,
	ISNULL(Convert(varchar(max), a.NonCompliantResident), '') AS NonCompliantResident
FROM #tempTable a
JOIN (
	SELECT audit_name, Convert(INT, SUM(TotalCompliance) / NumberOfAudits) AS AvgCompliance
	FROM #tempTable
	group by audit_name, NumberOfAudits
) b ON a.audit_name = b.audit_name


---- ACS-103 Develop tracking tools for user activity (logins, downloads)
CREATE TABLE [dbo].[LoginsTracking](
	Id [int] IDENTITY(1,1) PRIMARY KEY,
	UserId [int] NOT NULL,
	[Login] [datetime2](0) NOT NULL,
	Duration [nvarchar](50) NULL,
	Logout [datetime2](0) NULL
	
	CONSTRAINT FK_LoginsTracking_User FOREIGN KEY (UserId) REFERENCES dbo.[User](ID) ON DELETE CASCADE,
);

DROP TABLE IF EXISTS DownloadsTracking;

CREATE TABLE [dbo].[DownloadsTracking](
	Id [int] IDENTITY(1,1) PRIMARY KEY,
	UserId [int] NOT NULL,
	PortalReportId [int] NOT NULL,
	DateAndTime [datetime2](0) NULL

	CONSTRAINT FK_DownloadsTracking_User FOREIGN KEY (UserId) REFERENCES dbo.[User](ID) ON DELETE CASCADE,
	CONSTRAINT FK_DownloadsTracking_PortalReport FOREIGN KEY (PortalReportId) REFERENCES dbo.[PortalReport](Id) ON DELETE CASCADE,
);


ALTER TABLE [dbo].[Organization] ADD [AttachPortalReport] BIT  DEFAULT 0;


--Indexes
IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='idx_Audit_FormVersionId' 
AND object_id = OBJECT_ID('Audit'))
begin
	DROP INDEX idx_Audit_FormVersionId ON  [dbo].[Audit];
end
GO
CREATE NONCLUSTERED INDEX [idx_Audit_FormVersionId]
ON [dbo].[Audit] ([FormVersionId])
INCLUDE ([SubmittedByUserID])
GO

IF EXISTS (SELECT *  FROM sys.indexes  WHERE name='idx_AuditStatusHistory_Status' 
AND object_id = OBJECT_ID('AuditStatusHistory'))
begin
	DROP INDEX idx_AuditStatusHistory_Status ON  [dbo].[AuditStatusHistory];
end
GO
CREATE NONCLUSTERED INDEX idx_AuditStatusHistory_Status
ON [dbo].[AuditStatusHistory] ([Status])
INCLUDE ([AuditID],[Date])
GO

---- 368 AHT ->

ALTER TABLE [dbo].[Form] ADD [AHTime] INT  DEFAULT 0;

CREATE TABLE dbo.[Team] (
	ID int IDENTITY(1,1) PRIMARY KEY,
	Name nvarchar(MAX) NOT NULL
);
---- team ------
INSERT INTO [dbo].[Team] ([Name]) VALUES ('GENESIS (CLINICAL)')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('SOUTHERN QIP, GENESIS QM, ASTON MDS 3-2-1')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('SOUTHERN (CLINICAL)')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Aston Team 1')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Aston Team 2')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Genesis team 1')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Genesis team 2')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Genesis team 3')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Genesis team 4')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Genesis team 5')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('CITADEL NY/NJ')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Champion')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Lionstone')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('PACIFICARE')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('CLEARVIEW')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('LIVEWELL')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Heritage team 1')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Heritage team 2')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Heritage team 3')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Heritage team 4')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('CAMBRIDGE')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('PDPM (ASTON, SOUTHERN, PACIFICARE, PHILOSOPHY, LEGACY, CARDON)')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('CONSULATE (PDPM)')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Mitigate  (P&G)')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('IV DRIPT ')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('SYNERGY Team 1')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Synergy Team 2')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('ALLURE')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('McKinley')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Sunview')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('LEGACY')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('GENERATIONS')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('ORCHID COVE')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('PROVIDENT') 
INSERT INTO [dbo].[Team] ([Name]) VALUES ('AHAVA')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Highland')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('STERLING')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('IVY')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('White Oak')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('VOLARE')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Casper')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Renew Wound Care')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('NUTRACO')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('HighNif')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('USRN')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('DATA ENTRY')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('TEAM LEADERS')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('Q.A. TEAM')
INSERT INTO [dbo].[Team] ([Name]) VALUES ('NURSE LEADERS')




CREATE TABLE [dbo].[UserTeam](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[TeamID] [int] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserTeam] ADD  CONSTRAINT [PK_UserTeam] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserTeam]  WITH CHECK ADD  CONSTRAINT [FK_UserTeam_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserTeam] CHECK CONSTRAINT [FK_UserTeam_Team]
GO
ALTER TABLE [dbo].[UserTeam]  WITH CHECK ADD  CONSTRAINT [FK_UserTeam_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserTeam] CHECK CONSTRAINT [FK_UserTeam_User]
GO

----------    362 - 368

IF EXISTS (SELECT 	* FROM sysobjects WHERE name = 'GetAuditorProductivityInput')
	DROP PROCEDURE dbo.GetAuditorProductivityInput
GO

--EXEC GetAuditorProductivityInput '2024-04-24', '2024-04-28'
CREATE PROCEDURE  [dbo].[GetAuditorProductivityInput]
    @fromDateStartTime DATE = NULL,
    @toDateStartTime DATE = NULL,
    @fromDateCompletionTime DATE = NULL,
    @toDateCompletionTime DATE = NULL,
    @TeamId INT = NULL
AS
BEGIN
    IF @fromDateStartTime IS NOT NULL AND (@toDateStartTime IS NULL OR @toDateStartTime = @fromDateStartTime)
    BEGIN
        SET @toDateStartTime = DATEADD(DAY, 1, @fromDateStartTime)
    END
    ELSE IF @fromDateCompletionTime IS NOT NULL AND (@toDateCompletionTime IS NULL OR @toDateCompletionTime = @fromDateCompletionTime)
    BEGIN
        SET @toDateStartTime = DATEADD(DAY, 1, @toDateCompletionTime)
    END;

    WITH SFAHistory AS (
        SELECT 
            AuditID, 
            MIN([Date]) AS minDate, 
            COUNT(*) AS TotalCount 
        FROM AuditStatusHistory WITH (NOLOCK)
        WHERE [Status] = 2 
          AND [Date] IS NOT NULL
        GROUP BY AuditID
    ),
    ResidentCount AS (
        SELECT 
            atcv.AuditID,
            tc.FormVersionId,
            COUNT(tc.ID) AS ResidentCount
        FROM AuditTableColumnValue atcv WITH (NOLOCK)
        INNER JOIN TableColumn tc WITH (NOLOCK) ON atcv.TableColumnID = tc.ID AND tc.[Sequence] = 1
        GROUP BY atcv.AuditID, tc.FormVersionId
    ),
    AuditData AS (
        SELECT 
            a.ID,
            CAST(a.SubmittedDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
            CAST(h.minDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS CompletionTime,
            u.ID AS UserId,
            u.FullName AS UserFullName,
            u.TimeZone AS UserTimezone,
            fac.ID AS FacilityId,
            fac.[Name] AS FacilityName,
            f.ID AS AuditTypeId,
            f.[Name] AS TypeOfAudit,
            h.TotalCount AS NoOfFilteredAuditsAllType,
            DATEDIFF(SECOND, a.SubmittedDate, h.minDate) AS HandlingTimeSeconds,
            h.TotalCount AS NoOfFilteredAudits,
            DATENAME(MONTH, h.minDate) AS [Month],
            CASE WHEN adt.[Name] = 'Tracker' THEN rc.ResidentCount ELSE 1 END AS NoOfResidents,
            f.AHTime as TargetAHTPerResident
        FROM [Audit] a WITH (NOLOCK)
        INNER JOIN SFAHistory h ON a.Id = h.AuditID
        INNER JOIN [Facility] fac WITH (NOLOCK) ON a.FacilityId = fac.ID
        INNER JOIN [FormVersion] fv WITH (NOLOCK) ON a.FormVersionId = fv.Id
        INNER JOIN [Form] f WITH (NOLOCK) ON fv.FormId = f.ID
        INNER JOIN [User] u WITH (NOLOCK) ON a.SubmittedByUserID = u.ID
        INNER JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
        INNER JOIN [AuditType] adt WITH (NOLOCK) ON f.AuditTypeID = adt.ID
        INNER JOIN ResidentCount rc WITH (NOLOCK) ON a.ID = rc.AuditID AND a.FormVersionId = rc.FormVersionId
        WHERE (@fromDateStartTime IS NULL OR CAST(a.SubmittedDate AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)
          AND (@fromDateCompletionTime IS NULL OR CAST(h.minDate AS DATE) BETWEEN @fromDateCompletionTime AND @toDateCompletionTime)
          AND (@TeamId IS NULL OR t.TeamID = @TeamId)

        UNION ALL

        SELECT 
            ar.ID,
            CAST(ar.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
            CAST(ar.SentForApprovalDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS CompletionTime,
            u.ID AS UserId,
            u.FullName AS UserFullName,
            u.TimeZone AS UserTimezone,
            fac.ID AS FacilityId,
            fac.[Name] AS FacilityName,
            -1 AS AuditTypeId,
            'AI 24 hour keyword' AS TypeOfAudit,
            1 AS NoOfFilteredAuditsAllType,
            DATEDIFF(SECOND, ar.CreatedAt, ar.SentForApprovalDate) AS HandlingTimeSeconds,
            1 AS NoOfFilteredAudits,
            DATENAME(MONTH, ar.SentForApprovalDate) AS Month,
            NULL AS NoOfResidents,
            0 AS TargetAHTPerResident
        FROM [AuditAIReport] ar WITH (NOLOCK)
        INNER JOIN [Facility] fac WITH (NOLOCK) ON ar.FacilityId = fac.ID
        INNER JOIN [User] u WITH (NOLOCK) ON ar.AuditorName = u.FullName
        INNER JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
        WHERE ar.SentForApprovalDate IS NOT NULL
          AND (@fromDateStartTime IS NULL OR CAST(ar.CreatedAt AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)
          AND (@fromDateCompletionTime IS NULL OR CAST(ar.SentForApprovalDate AS DATE) BETWEEN @fromDateCompletionTime AND @toDateCompletionTime)
          AND (@TeamId IS NULL OR t.TeamID = @TeamId)

        UNION ALL

        SELECT 
            ar2.ID,
            CAST(ar2.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
            CAST(ar2.SentForApprovalDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS CompletionTime,
            u.ID AS UserId,
            u.FullName AS UserFullName,
            u.TimeZone AS UserTimezone,
            fac.ID AS FacilityId,
            fac.[Name] AS FacilityName,
            -1 AS AuditTypeId,
            'AI 24 hour keyword' AS TypeOfAudit,
            1 AS NoOfFilteredAuditsAllType,
            DATEDIFF(SECOND, ar2.CreatedAt, ar2.SentForApprovalDate) AS HandlingTimeSeconds,
            1 AS NoOfFilteredAudits,
            DATENAME(MONTH, ar2.SentForApprovalDate) AS Month,
            NULL AS NoOfResidents,
            0 AS TargetAHTPerResident

        FROM [AuditAIReportV2] ar2 WITH (NOLOCK)
        INNER JOIN [Facility] fac WITH (NOLOCK) ON ar2.FacilityId = fac.ID
        INNER JOIN [User] u WITH (NOLOCK) ON ar2.AuditorName = u.FullName
        INNER JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
        WHERE ar2.SentForApprovalDate IS NOT NULL
          AND (@fromDateStartTime IS NULL OR CAST(ar2.CreatedAt AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)
          AND (@fromDateCompletionTime IS NULL OR CAST(ar2.SentForApprovalDate AS DATE) BETWEEN @fromDateCompletionTime AND @toDateCompletionTime)
          AND (@TeamId IS NULL OR t.TeamID = @TeamId)
    )

    SELECT 
        ID, StartTime, CompletionTime, UserId, UserFullName, UserTimezone, FacilityId, FacilityName, 
        AuditTypeId, TypeOfAudit, NoOfFilteredAuditsAllType,
        FORMAT(DATEADD(SECOND, HandlingTimeSeconds, '1900-01-01'), 'HH:mm:ss') AS HandlingTime,
        FORMAT(DATEADD(SECOND, HandlingTimeSeconds / NULLIF(NoOfFilteredAuditsAllType, 0), '1900-01-01'), 'HH:mm:ss') AS AHTPerAudit,
        NoOfFilteredAudits,
        FORMAT(DATEADD(SECOND, HandlingTimeSeconds, '1900-01-01'), 'HH:mm:ss') AS FinalAHT,
        [Month],
        NoOfResidents,
        TargetAHTPerResident
    FROM AuditData
END
GO




IF EXISTS (SELECT 	* FROM sysobjects WHERE name = 'GetAuditorProductivitySummaryPerAuditor')
	DROP PROCEDURE dbo.GetAuditorProductivitySummaryPerAuditor
GO

--EXEC GetAuditorProductivitySummaryPerAuditor '2025-04-29', '2025-04-30'
CREATE PROCEDURE [dbo].[GetAuditorProductivitySummaryPerAuditor]
@fromDateStartTime DATE = NULL,
@toDateStartTime DATE = NULL,
 @TeamId INT = NULL
AS
BEGIN
    SET @fromDateStartTime = ISNULL(@fromDateStartTime, CAST(GETDATE() AS DATE))
    
    IF @toDateStartTime IS NULL OR @toDateStartTime = @fromDateStartTime
    BEGIN
        SET @toDateStartTime = DATEADD(DAY, 1, @fromDateStartTime)
    END;

    WITH SFAHistory AS (
        SELECT 
            AuditID, 
            MIN([Date]) AS minDate, 
            COUNT(*) AS TotalCount 
        FROM AuditStatusHistory WITH (NOLOCK)
        WHERE [Status] = 2 
        AND [Date] IS NOT NULL
        GROUP BY AuditID
    ),
    ResidentCount AS (
        SELECT 
            atcv.AuditID,
            tc.FormVersionId,
            COUNT(tc.ID) AS ResidentCount
        FROM AuditTableColumnValue atcv WITH (NOLOCK)
        INNER JOIN TableColumn tc WITH (NOLOCK) ON atcv.TableColumnID = tc.ID AND tc.[Sequence] = 1
        GROUP BY atcv.AuditID, tc.FormVersionId
    )

    SELECT
        CAST(a.SubmittedDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        u.TimeZone as UserTimezone,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        f.ID AS AuditTypeId,
        f.[Name] AS TypeOfAudit,
        DATEDIFF(SECOND, a.SubmittedDate, h.minDate) AS FinalAHT,
        h.TotalCount AS NoOfFilteredAudits,
        CASE WHEN adt.[Name] = 'Tracker' THEN rc.ResidentCount ELSE 1 END AS NoOfResidents,
        f.AHTime as TargetAHTPerResident
    FROM [Audit] a WITH (NOLOCK)
    INNER JOIN SFAHistory h ON a.Id = h.AuditID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON a.FacilityId = fac.ID
    INNER JOIN [FormVersion] fv WITH (NOLOCK) ON a.FormVersionId = fv.Id
    INNER JOIN [Form] f WITH (NOLOCK) ON fv.FormId = f.ID
    INNER JOIN [AuditType] adt WITH (NOLOCK) ON f.AuditTypeID = adt.ID
    INNER JOIN [User] u WITH (NOLOCK) ON a.SubmittedByUserID = u.ID
    LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    LEFT JOIN ResidentCount rc ON a.ID = rc.AuditID AND a.FormVersionId = rc.FormVersionId
    WHERE (@fromDateStartTime IS NULL OR CAST(a.SubmittedDate AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)
         AND (@TeamId IS NULL OR t.TeamID = @TeamId)

    UNION ALL

    SELECT 
        CAST(ar.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        u.TimeZone as UserTimezone,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        -1 AS AuditTypeId,
        CAST('AI 24 hour keyword' AS NVARCHAR(100))  AS TypeOfAudit,
        DATEDIFF(SECOND, ar.CreatedAt, ar.SentForApprovalDate) AS FinalAHT,
        1 AS NoOfFilteredAudits,
        NULL AS NoOfResidents,
        NULL AS TargetAHTPerResident
    FROM [AuditAIReport] ar WITH (NOLOCK)
    INNER JOIN [User] u WITH (NOLOCK) ON ar.AuditorName = u.FullName
    LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON ar.FacilityID = fac.ID
    WHERE ar.SentForApprovalDate IS NOT NULL
    AND (@fromDateStartTime IS NULL OR CAST(ar.CreatedAt AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)    AND (@TeamId IS NULL OR t.TeamID = @TeamId)

    UNION ALL

    SELECT 
        CAST(ar2.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        u.TimeZone as UserTimezone,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        -1 AS AuditTypeId,
        CAST('AI 24 hour keyword V2' AS NVARCHAR(100))  AS TypeOfAudit,
        DATEDIFF(SECOND, ar2.CreatedAt, ar2.SentForApprovalDate) AS FinalAHT,
        1 AS NoOfFilteredAudits,
        NULL AS NoOfResidents,
        NULL AS TargetAHTPerResident
    FROM [AuditAIReportV2] ar2 WITH (NOLOCK)
    INNER JOIN [User] u WITH (NOLOCK) ON ar2.AuditorName = u.FullName
    LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON ar2.FacilityID = fac.ID
    WHERE ar2.SentForApprovalDate IS NOT NULL
    AND (@fromDateStartTime IS NULL OR CAST(ar2.CreatedAt AS DATE) BETWEEN @fromDateStartTime AND @toDateStartTime)    AND (@TeamId IS NULL OR t.TeamID = @TeamId)
END
GO

IF EXISTS (SELECT 	* FROM sysobjects WHERE name = 'GetAuditorProductivityAHTPerAuditType')
	DROP PROCEDURE dbo.GetAuditorProductivityAHTPerAuditType
GO

--EXEC GetAuditorProductivityAHTPerAuditType '2025-04-24', '2025-04-28'
CREATE PROCEDURE [dbo].[GetAuditorProductivityAHTPerAuditType]
@fromDateStartTime DATE = NULL,
@toDateStartTime DATE = NULL,
@TeamId INT = NULL
AS
BEGIN
    SET @fromDateStartTime = ISNULL(@fromDateStartTime, CAST(GETDATE() AS DATE))
    IF @toDateStartTime IS NULL OR @toDateStartTime = @fromDateStartTime
    BEGIN
        SET @toDateStartTime = DATEADD(DAY, 1, @fromDateStartTime)
    END;

    WITH SFAHistory AS (
        SELECT 
            AuditID, 
            MIN([Date]) AS minDate, 
            COUNT(*) AS TotalCount 
        FROM AuditStatusHistory WITH (NOLOCK)
        WHERE [Status] = 2 
        AND [Date] IS NOT NULL
        GROUP BY AuditID
    ),
    ResidentCount AS (
        SELECT 
            atcv.AuditID,
            tc.FormVersionId,
            COUNT(tc.ID) AS ResidentCount
        FROM AuditTableColumnValue atcv WITH (NOLOCK)
        INNER JOIN TableColumn tc WITH (NOLOCK) ON atcv.TableColumnID = tc.ID AND tc.[Sequence] = 1
        GROUP BY atcv.AuditID, tc.FormVersionId
    )
    SELECT
        CAST(a.SubmittedDate AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        f.ID AS AuditTypeId,
        f.[Name] AS TypeOfAudit,
        DATEDIFF(SECOND, a.SubmittedDate, h.minDate) AS FinalAHT,
        h.TotalCount AS NoOfFilteredAudits,
        CASE WHEN adt.[Name] = 'Tracker' THEN rc.ResidentCount ELSE 1 END AS NoOfResidents,
        f.AHTime AS TargetAHTPerResident
    FROM [Audit] a WITH (NOLOCK)
    INNER JOIN SFAHistory h ON a.Id = h.AuditID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON a.FacilityId = fac.ID
    INNER JOIN [FormVersion] fv WITH (NOLOCK) ON a.FormVersionId = fv.Id
    INNER JOIN [Form] f WITH (NOLOCK) ON fv.FormId = f.ID
    INNER JOIN [AuditType] adt WITH (NOLOCK) ON f.AuditTypeID = adt.ID
    INNER JOIN [User] u WITH (NOLOCK) ON a.SubmittedByUserID = u.ID
    LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    LEFT JOIN ResidentCount rc ON a.ID = rc.AuditID AND a.FormVersionId = rc.FormVersionId
    WHERE (@fromDateStartTime IS NULL OR a.SubmittedDate BETWEEN @fromDateStartTime AND @toDateStartTime)
           AND (@TeamId IS NULL OR t.TeamID = @TeamId)
    UNION ALL

    SELECT 
        CAST(ar.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        -1 AS AuditTypeId,
         CAST('AI 24 hour keyword' AS NVARCHAR(100))  AS TypeOfAudit,
        DATEDIFF(SECOND, ar.CreatedAt, ar.SentForApprovalDate) AS FinalAHT,
        1 AS NoOfFilteredAudits,
        NULL AS NoOfResidents,
        NULL AS  TargetAHTPerResident
    FROM [AuditAIReport] ar WITH (NOLOCK)
    INNER JOIN [User] u WITH (NOLOCK) ON ar.AuditorName = u.FullName
     LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON ar.FacilityID = fac.ID
    WHERE ar.SentForApprovalDate IS NOT NULL
    AND (@fromDateStartTime IS NULL OR ar.CreatedAt BETWEEN @fromDateStartTime AND @toDateStartTime)
         AND (@TeamId IS NULL OR t.TeamID = @TeamId)
    UNION ALL

    SELECT 
        CAST(ar2.CreatedAt AT TIME ZONE 'UTC' AT TIME ZONE u.TimeZone AS datetime2) AS StartTime,
        u.ID AS UserId,
        u.FullName AS UserFullName,
        fac.ID AS FacilityId,
        fac.[Name] AS FacilityName,
        -1 AS AuditTypeId,
        CAST('AI 24 hour keyword V2' AS NVARCHAR(100))  AS TypeOfAudit,
        DATEDIFF(SECOND, ar2.CreatedAt, ar2.SentForApprovalDate) AS FinalAHT,
        1 AS NoOfFilteredAudits,
        NULL AS NoOfResidents,
         NULL AS  TargetAHTPerResident
    FROM [AuditAIReportV2] ar2 WITH (NOLOCK)
    INNER JOIN [User] u WITH (NOLOCK) ON ar2.AuditorName = u.FullName
     LEFT JOIN [UserTeam] t WITH (NOLOCK) ON t.UserID = u.ID
    INNER JOIN [Facility] fac WITH (NOLOCK) ON ar2.FacilityID = fac.ID
    WHERE ar2.SentForApprovalDate IS NOT NULL
    AND (@fromDateStartTime IS NULL OR ar2.CreatedAt BETWEEN @fromDateStartTime AND @toDateStartTime)  AND (@TeamId IS NULL OR t.TeamID = @TeamId)
END
GO



