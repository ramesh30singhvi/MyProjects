using System;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.Common.Constants;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.PDF
{
    internal class CriteriaAuditHeaderEventHandler : AuditHeaderEventHandler
    {
        internal CriteriaAuditHeaderEventHandler(Document document, System.Collections.Generic.List<AuditDto> audits, string titleText)
                : base(document, audits, titleText)
        {
        }

        protected override Table CreateAudit(AuditDto audit)
        {
            float availableWidth = _pageSize.GetWidth() - 40;
            float[] columnsPercentWidth = new float[] { 30, 45, 25 };

            float orgWidth = availableWidth * columnsPercentWidth[0] / 100;
            float facilityWidth = availableWidth * columnsPercentWidth[1] / 100;
            float datesWidth = availableWidth * columnsPercentWidth[2] / 100;

            var percentArray = UnitValue.CreatePercentArray(columnsPercentWidth);
            var table = new Table(percentArray)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(5)
                .SetMarginBottom(5)
                .UseAllAvailableWidth();

            var organizationHeader = CreateAuditHeaderCell("Organization")
                .SetWidth(orgWidth)
                .SetPaddingBottom(0);
            table.AddCell(organizationHeader);

            var facilityHeader = CreateAuditHeaderCell("Facility")
                .SetWidth(facilityWidth)
                .SetPaddingBottom(0);
            table.AddCell(facilityHeader);

            var datesHeader = CreateAuditHeaderCell("Dates")
                .SetWidth(datesWidth)
                .SetPaddingBottom(0);
            table.AddCell(datesHeader);

            var organization = CreateAuditDataCell(audit.Organization.Name)
                .SetWidth(orgWidth)
                .SetPaddingTop(0);
            table.AddCell(organization);

            var facility = CreateAuditDataCell(audit.Facility.Name)
                .SetWidth(facilityWidth)
                .SetPaddingTop(0);
            table.AddCell(facility);

            var dateFrom = audit.IncidentDateFrom?.ToString(DATE_FORMAT);
            var dateTo = audit.IncidentDateTo?.ToString(DATE_FORMAT);
            var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";
            var date = CreateAuditDataCell(dateText)
                .SetWidth(datesWidth)
                .SetPaddingTop(0);
            table.AddCell(date);


            // Add Summary with Resident Name and Compliance Summary
            var borderColor = new DeviceRgb(100, 100, 100);
            var deviderBorder = new SolidBorder(borderColor, 1);
            var summaryHeaderColor = new DeviceRgb(38, 50, 55);
            var summaryResidentColor = new DeviceRgb(5, 5, 5);
            var redColor = new DeviceRgb(180, 0, 0);
            var greenColor = new DeviceRgb(0, 120, 30);

            table.AddCell(new Cell(1, 3).SetWidth(_pageSize.GetWidth()).SetBorder(Border.NO_BORDER).SetBorderTop(deviderBorder).SetMarginLeft(-20).SetMarginRight(-20));
            if (audit.Form.DisableCompliance == 1 || audit.Form.AuditType.Name == CommonConstants.MDS)
            {
                table.AddCell(new Cell(1, 3).SetBorder(Border.NO_BORDER).Add(new Paragraph("RESIDENT")).SetFontSize(10).SetFontColor(summaryHeaderColor));
            }
            else
            {
                table.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).Add(new Paragraph("RESIDENT")).SetFontSize(10).SetFontColor(summaryHeaderColor));

                table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).Add(new Paragraph("COMPLIANCE SUMMARY")).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT).SetFontColor(summaryHeaderColor));
            }
            

           // table.AddCell(new Cell(1, 3).SetWidth(_pageSize.GetWidth()).SetBorder(Border.NO_BORDER).SetBorderTop(deviderBorder));
            int auditCounter = 0;

            foreach (var _audit in _audits)
            {
                var resident = new Paragraph()
                                     .SetFontColor(summaryResidentColor)
                                     .SetAction(iText.Kernel.Pdf.Action.PdfAction.CreateGoTo(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter++)));
                
                resident.Add(audit.Resident ?? string.Empty);

                if (_audit.HighAlertCategory != null)
                {                
                    Text redText = new Text(" HIGH ALERT").SetFontColor(redColor);
                    resident.Add(redText);
                }
 
                var totalComplianceValue = audit.TotalCompliance.HasValue ? Math.Round(audit.TotalCompliance.Value, 2) : 0;

                var totalCompliance = new Paragraph($"{totalComplianceValue}%")
                    .SetFontColor(totalComplianceValue == 100 ? greenColor : redColor);

                var residentCell = PdfHelper.CreateCriteriaSummaryCell(deviderBorder, TextAlignment.LEFT)
                    .Add(resident);

                var totalComplianceCell = PdfHelper.CreateCriteriaSummaryCell(deviderBorder, TextAlignment.RIGHT)
                    .Add(totalCompliance);
                if (audit.Form.DisableCompliance == 1 || audit.Form.AuditType.Name == CommonConstants.MDS)
                {
                    table.AddCell(new Cell(1, 3).SetBorder(Border.NO_BORDER).SetBorderTop(deviderBorder).Add(resident).SetTextAlignment(TextAlignment.LEFT));
                }
                else
                {
                    table.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER).SetBorderTop(deviderBorder).Add(resident).SetTextAlignment(TextAlignment.LEFT));                   
                    table.AddCell(new Cell(1, 1).SetBorder(Border.NO_BORDER).SetBorderTop(deviderBorder).Add(totalCompliance).SetTextAlignment(TextAlignment.RIGHT));
                }
                    
                
            }

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
