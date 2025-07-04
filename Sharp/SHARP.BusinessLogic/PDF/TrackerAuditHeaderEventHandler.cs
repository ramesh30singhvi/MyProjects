using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.BusinessLogic.PDF
{
    internal class TrackerAuditHeaderEventHandler : AuditHeaderEventHandler
    {
        internal TrackerAuditHeaderEventHandler(Document document, List<AuditDto> audits, string titleText)
            :base(document, audits, titleText)
        {
        }
        public override void SetRecordsCount()
        {
            foreach(var audit in _audits)
            {
                var trackerAudit = audit as TrackerAuditDetailsDto;
                _recordsCount += trackerAudit != null ? trackerAudit.PivotAnswerGroups.Count : 0;
            }
        }
        string GetIncidentDateRangeText()
        {
            DateTime? incidentDateFrom = _audits.Min(audit=>audit.IncidentDateFrom);

            if(incidentDateFrom == null)
            {
                return string.Empty;
            }

            DateTime? incidentDateTo = _audits.Max(audit => audit.IncidentDateTo);
            var dateText = incidentDateTo == null ? 
                                            incidentDateFrom.Value.ToString(DATE_FORMAT) : 
                                            $"{incidentDateFrom.Value.ToString(DATE_FORMAT)} - {incidentDateTo.Value.ToString(DATE_FORMAT)}";
            return dateText;
        }

        string GetSubmittedDateRangeText()
        {
            var orderedAudits = _audits.OrderBy(audit => audit.SubmittedDate).ToList();

            var auditStart = orderedAudits.First();
            var auditEnd = orderedAudits.Last();
            string submittedDateText = string.Empty;

            if(auditStart.SubmittedDate == auditEnd.SubmittedDate && auditStart.Facility.TimeZoneShortName == auditEnd.Facility.TimeZoneShortName)
            {
                submittedDateText = $"{auditStart.SubmittedDate.ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({auditStart.Facility.TimeZoneShortName})";
            }
            else
            {
                submittedDateText = $@"{auditStart.SubmittedDate.ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({auditStart.Facility.TimeZoneShortName}) - {auditEnd.SubmittedDate.ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({auditEnd.Facility.TimeZoneShortName})";
            }

            return submittedDateText;
        }
        protected override Table CreateAudit(AuditDto audit)
        {
            float availableWidth = _pageSize.GetWidth() - 20;

            float[] columnsPercentWidth = new float[] { 18,18, 18, 35, 10 };

            float orgWidth = availableWidth * columnsPercentWidth[0] / 100;
            float facWidth = availableWidth * columnsPercentWidth[1] / 100;
            float datesWidth = availableWidth * columnsPercentWidth[2] / 100;
            float auditDatesWidth = availableWidth * columnsPercentWidth[3] / 100;
            float recordsWidth = availableWidth * columnsPercentWidth[4] / 100;

            var percentArray = UnitValue.CreatePercentArray(columnsPercentWidth);
            var table = new Table(percentArray)
                .SetMarginTop(5)
                .SetMarginBottom(5)
                .SetMinHeight(50)
                .UseAllAvailableWidth();

            var organizationHeader = CreateAuditHeaderCell("Organization")
                .SetWidth(orgWidth)
                .SetPaddingBottom(0);
            table.AddCell(organizationHeader);

            var facilityHeader = CreateAuditHeaderCell("Facility")
                .SetWidth(orgWidth)
                .SetPaddingBottom(0);
            table.AddCell(facilityHeader);

            var datesHeader = CreateAuditHeaderCell("Dates")
                .SetWidth(datesWidth)
                .SetPaddingBottom(0);
            table.AddCell(datesHeader);

            var auditDateHeader = CreateAuditHeaderCell("Audit Date")
                .SetWidth(auditDatesWidth)
                .SetPaddingBottom(0);
            table.AddCell(auditDateHeader);

            var recordsCountHeader = CreateAuditHeaderCell("Records")
                .SetWidth(recordsWidth)
                .SetPaddingBottom(0);
            table.AddCell(recordsCountHeader);

            var organization = CreateAuditDataCell(audit.Organization.Name)
                .SetWidth(orgWidth)
                .SetPaddingTop(0);
            table.AddCell(organization);


            var facility = CreateAuditDataCell(audit.Facility?.Name)
                .SetWidth(orgWidth)
                .SetPaddingTop(0);
            table.AddCell(facility);

            var dateText = GetIncidentDateRangeText();
            var date = CreateAuditDataCell(dateText)
                .SetWidth(datesWidth)
                .SetPaddingTop(0);
            table.AddCell(date);

            string submittedDateText = GetSubmittedDateRangeText();
            var auditDate = CreateAuditDataCell(submittedDateText)
                .SetWidth(auditDatesWidth)
                .SetPaddingTop(0);
            table.AddCell(auditDate);

            var records = CreateAuditDataCell(_recordsCount.ToString())
                .SetWidth(recordsWidth)
                .SetPaddingTop(0);
            table.AddCell(records);

            return table;

            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text?.ToUpper());

            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
                .SetFontSize(12);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                PdfHelper.CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }
    }
}
