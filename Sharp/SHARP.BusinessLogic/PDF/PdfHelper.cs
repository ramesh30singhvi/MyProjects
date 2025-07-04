
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Newtonsoft.Json.Linq;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.Helpers;
using SHARP.Common.Constants;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.BusinessLogic.PDF
{
    internal class PdfHelper
    {
        public static Cell CreateCriteriaSummaryCell(SolidBorder deviderBorder, TextAlignment textAlignment)
        {
            return new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(deviderBorder)
                    //.SetBorderBottom(deviderBorder)
                    .SetTextAlignment(textAlignment)
                    .SetFontSize(12)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
        }

        public static Table CreateCriteriaAuditSummaryTable(AuditDto[] audits)
        {
            int auditCounter = 0;
            var borderColor = new DeviceRgb(100, 100, 100);
            var redColor = new DeviceRgb(180, 0, 0);
            var greenColor = new DeviceRgb(0, 120, 30);

            var summaryHeaderColor = new DeviceRgb(38, 50, 55);
            var summaryResidentColor = new DeviceRgb(5, 5, 5);
            var deviderBorder = new SolidBorder(borderColor, 1);

            var summaryPercentArray = UnitValue.CreatePercentArray(new float[] { 70, 30 });
            var summaryTable = new Table(summaryPercentArray)
                .SetMarginTop(10)
                .SetMarginBottom(10)
                .UseAllAvailableWidth();

            summaryTable.AddCell(new Cell()
                .Add(new Paragraph($"RESIDENT ({audits.Count()})"))
                .SetBorder(Border.NO_BORDER)
                .SetFontSize(10)
                .SetFontColor(summaryHeaderColor));

            summaryTable.AddCell(new Cell()
                .Add(new Paragraph("COMPLIANCE SUMMARY"))
                .SetBorder(Border.NO_BORDER)
                .SetFontSize(10)
                .SetFontColor(summaryHeaderColor)
                .SetTextAlignment(TextAlignment.RIGHT));

            foreach (var audit in audits)
            {
                var resident = new Paragraph(audit.Resident ?? string.Empty)
                    .SetFontColor(summaryResidentColor)
                    .SetAction(iText.Kernel.Pdf.Action.PdfAction.CreateGoTo(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter++)));

                var totalComplianceValue = audit.TotalCompliance.HasValue ? Math.Round(audit.TotalCompliance.Value, 2) : 0;

                var totalCompliance = new Paragraph($"{totalComplianceValue}%")
                    .SetFontColor(totalComplianceValue == 100 ? greenColor : redColor);

                var residentCell = PdfHelper.CreateCriteriaSummaryCell(deviderBorder, TextAlignment.LEFT)
                    .Add(resident);
                var totalComplianceCell = PdfHelper.CreateCriteriaSummaryCell(deviderBorder, TextAlignment.RIGHT)
                    .Add(totalCompliance);

                summaryTable.AddCell(residentCell);
                summaryTable.AddCell(totalComplianceCell);
            }

            return summaryTable;
        }

        public static Table CreateMdsAuditSummaryTable(AuditDto[] audits)
        {
            int auditCounter = 0;
            var borderColor = new DeviceRgb(100, 100, 100);
            var redColor = new DeviceRgb(180, 0, 0);
            var greenColor = new DeviceRgb(0, 120, 30);

            var summaryHeaderColor = new DeviceRgb(38, 50, 55);
            var summaryResidentColor = new DeviceRgb(5, 5, 5);
            var deviderBorder = new SolidBorder(borderColor, 1);

            var summaryPercentArray = UnitValue.CreatePercentArray(new float[] { 70, 30 });
            var summaryTable = new Table(summaryPercentArray)
                .SetMarginTop(10)
                .SetMarginBottom(10)
                .UseAllAvailableWidth();

            summaryTable.AddCell(new Cell()
                .Add(new Paragraph($"RESIDENT ({audits.Count()})"))
                .SetBorder(Border.NO_BORDER)
                .SetFontSize(10)
                .SetFontColor(summaryHeaderColor));



            foreach (var audit in audits)
            {
                var resident = new Paragraph(audit.Resident ?? string.Empty)
                    .SetFontColor(summaryResidentColor)
                    .SetAction(iText.Kernel.Pdf.Action.PdfAction.CreateGoTo(string.Format("{0}-{1}", audit.Resident ?? string.Empty, auditCounter++)));

                var residentCell = PdfHelper.CreateCriteriaSummaryCell(deviderBorder, TextAlignment.LEFT)
                    .Add(resident);

                summaryTable.AddCell(residentCell);
            }

            return summaryTable;
        }

        public static Table CreateCriteriaAuditDetailedDummyHeader()
        {
            var detailedHeaderBorder = new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(new float[] { 13, 17, 25, 15, 23 });

            var detailedHeaderTable = new Table(detailedHeaderPercentArray)
                .UseAllAvailableWidth()
                .SetBorderTop(detailedHeaderBorder)
                .SetBorderBottom(detailedHeaderBorder)
                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                .SetMarginTop(0)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            var buildingHeader = CreateAuditHeaderCell("Building")
                .SetPaddingLeft(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(buildingHeader);

            var identifierHeader = CreateAuditHeaderCell("Identifier")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(identifierHeader);

            var datesHeader = CreateAuditHeaderCell("Filtered date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(datesHeader);

            var auditorHeader = CreateAuditHeaderCell("Audited by")
                .SetPaddingRight(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditorHeader);

            var auditDateHeader = CreateAuditHeaderCell("Audit Date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditDateHeader);

            var building = CreateAuditDataCell("Lorem Lipsum Lorem")
                .SetPaddingLeft(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(building);

            var identifier = CreateAuditDataCell("Lorem Lipsum Lorem")
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(identifier);


            var dateText = "Lorem Lipsum Lorem";
            var date = CreateAuditDataCell(dateText)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(date);

            var auditor = CreateAuditDataCell("Lorem Lipsum Lorem")
                .SetPaddingRight(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditor);
            var auditDate = CreateAuditDataCell("Lorem Lipsum Lorem")
            .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditDate);
            return detailedHeaderTable;
            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text.ToUpper());
            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
            .SetFontSize(11);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        public static Table CreateCriteriaAuditDetailedHeader(AuditDto audit)
        {
            var detailedHeaderBorder = new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(new float[] { 13, 17, 25, 15, 23 });

            var detailedHeaderTable = new Table(detailedHeaderPercentArray)
                .UseAllAvailableWidth()
                .SetBorderTop(detailedHeaderBorder)
                .SetBorderBottom(detailedHeaderBorder)
                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                .SetMarginTop(0)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            var buildingHeader = CreateAuditHeaderCell("Building")
                .SetPaddingLeft(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(buildingHeader);

            var identifierHeader = CreateAuditHeaderCell("Identifier")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(identifierHeader);

            var datesHeader = CreateAuditHeaderCell("Filtered date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(datesHeader);

            var auditorHeader = CreateAuditHeaderCell("Audited by")
                .SetPaddingRight(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditorHeader);

            var auditDateHeader = CreateAuditHeaderCell("Audit Date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditDateHeader);

            var building = CreateAuditDataCell(audit.Room)
                .SetPaddingLeft(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(building);

            var identifier = CreateAuditDataCell(audit.Resident)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(identifier);

            var dateFrom = audit.IncidentDateFrom?.ToString(DateTimeConstants.MM_DD_YYYY_SLASH);
            var dateTo = audit.IncidentDateTo?.ToString(DateTimeConstants.MM_DD_YYYY_SLASH);
            var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";
            var date = CreateAuditDataCell(dateText)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(date);

            var auditor = CreateAuditDataCell(audit.SubmittedByUser.FullName)
                .SetPaddingRight(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditor);

            var auditDate = CreateAuditDataCell($"{audit.SubmittedDate.AddHours(audit.Facility.TimeZoneOffset).ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({audit.Facility.TimeZoneShortName})")
               .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditDate);

            return detailedHeaderTable;

            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text.ToUpper());

            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
                .SetFontSize(11);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        public static Table CreateMdsAuditDetailedHeader(AuditDto audit)
        {
            var detailedHeaderBorder = new SolidBorder(new DeviceRgb(100, 100, 100), 1.5f);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(new float[] { 13, 17, 25, 15, 23 });

            var detailedHeaderTable = new Table(detailedHeaderPercentArray)
                .UseAllAvailableWidth()
                .SetBorderTop(detailedHeaderBorder)
                .SetBorderBottom(detailedHeaderBorder)
                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                .SetMarginTop(0)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            var buildingHeader = CreateAuditHeaderCell("Building")
                .SetPaddingLeft(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(buildingHeader);

            var identifierHeader = CreateAuditHeaderCell("Identifier")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(identifierHeader);

            var datesHeader = CreateAuditHeaderCell("Filtered date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(datesHeader);

            var auditorHeader = CreateAuditHeaderCell("Audited by")
                .SetPaddingRight(20)
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditorHeader);

            var auditDateHeader = CreateAuditHeaderCell("Audit Date")
                .SetPaddingBottom(0);
            detailedHeaderTable.AddCell(auditDateHeader);

            var building = CreateAuditDataCell(audit.Room)
                .SetPaddingLeft(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(building);

            var identifier = CreateAuditDataCell(audit.Resident)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(identifier);

            var dateFrom = audit.IncidentDateFrom?.ToString(DateTimeConstants.MM_DD_YYYY_SLASH);
            var dateTo = audit.IncidentDateTo?.ToString(DateTimeConstants.MM_DD_YYYY_SLASH);
            var dateText = dateTo == null ? dateFrom : $"{dateFrom} - {dateTo}";
            var date = CreateAuditDataCell(dateText)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(date);

            var auditor = CreateAuditDataCell(audit.SubmittedByUser.FullName)
                .SetPaddingRight(20)
                .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditor);

            var auditDate = CreateAuditDataCell($"{audit.SubmittedDate.AddHours(audit.Facility.TimeZoneOffset).ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({audit.Facility.TimeZoneShortName})")
               .SetPaddingTop(0);
            detailedHeaderTable.AddCell(auditDate);

            return detailedHeaderTable;

            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text.ToUpper());

            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
                .SetFontSize(11);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        public static Table CreateCriteriaAuditSubheaderData(CriteriaAuditDetailsDto audit)
        {
            var subheaderPercentArray = UnitValue.CreatePercentArray(new float[] { 24, 24, 24, 24 });

            var subheaderDataTable = new Table(subheaderPercentArray)
               .UseAllAvailableWidth();

            foreach (var subheader in audit.FormVersion.FormFields)
            {
                var label = new Paragraph(subheader.LabelName?.ToUpper())
                    .SetFontColor(new DeviceRgb(38, 50, 55));

                var value = new Paragraph(subheader.Value != null ? subheader.Value.FormattedValue : "-");

                var cell = CreateCell()
                    .Add(label)
                    .Add(value)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(10)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetMarginLeft(20);

                subheaderDataTable.AddCell(cell);
            }

            return subheaderDataTable;
        }

        public static Table CreateMdsAuditSubheaderData(MdsAuditDetailsDto audit)
        {
            var subheaderPercentArray = UnitValue.CreatePercentArray(new float[] { 24, 24, 24, 24 });

            var subheaderDataTable = new Table(subheaderPercentArray)
               .UseAllAvailableWidth();

            foreach (var subheader in audit.FormVersion.FormFields)
            {
                var label = new Paragraph(subheader.LabelName?.ToUpper())
                    .SetFontColor(new DeviceRgb(38, 50, 55));

                var value = new Paragraph(subheader.Value != null ? subheader.Value.FormattedValue : "-");

                var cell = CreateCell()
                    .Add(label)
                    .Add(value)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(10)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetMarginLeft(20);

                subheaderDataTable.AddCell(cell);
            }

            return subheaderDataTable;
        }

        public static Table CreateCriteriaAuditDetailedData(CriteriaAuditDetailsDto audit)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);
            var answersHeaderColor = new DeviceRgb(250, 250, 250);

            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var detailedDataBorder = new SolidBorder(detailedDataBorderColor, 1);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(new float[] { 7, 42, 9, 42 });

            var detailedDataTable = new Table(detailedHeaderPercentArray)
                .UseAllAvailableWidth()
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            if (audit.FormVersion.FormFields != null && audit.FormVersion.FormFields.Any())
            {
                detailedDataTable.AddCell(new Cell(1, 4)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder));
            }

            foreach (var questionGroup in audit.FormVersion.QuestionGroups)
            {
                if (questionGroup.Questions == null || !questionGroup.Questions.Any())
                {
                    continue;
                }

                if (questionGroup.Name != null)
                {
                    var groupParagraph = new Paragraph(questionGroup.Name != null ? questionGroup.Name : string.Empty)
                        .SetFontSize(10)
                        .SetFontColor(new DeviceRgb(38, 50, 55));
                    var groupCell = new Cell(1, 4)
                        .SetBorder(Border.NO_BORDER)
                        .Add(groupParagraph)
                        .SetPaddingLeft(20)
                        .SetBackgroundColor(new DeviceRgb(238, 236, 236))
                        .SetBorderBottom(detailedHeaderBorder);
                    detailedDataTable.AddCell(groupCell);
                }

                detailedDataTable.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("#"))
                    .SetFontSize(10)
                    .SetFontColor(headerColor)
                    .SetBackgroundColor(answersHeaderColor)
                    .SetPaddingLeft(20));

                detailedDataTable.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("CRITERIA"))
                    .SetFontSize(10)
                    .SetFontColor(headerColor)
                    .SetBackgroundColor(answersHeaderColor));

                detailedDataTable.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("ANSWER"))
                    .SetFontSize(10)
                    .SetFontColor(headerColor)
                    .SetBackgroundColor(answersHeaderColor));

                detailedDataTable.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("AUDITOR COMMENT"))
                    .SetFontSize(10)
                    .SetFontColor(headerColor)
                    .SetPaddingRight(20)
                    .SetBackgroundColor(answersHeaderColor));

                int index = 0;

                foreach (var question in questionGroup.Questions)
                {
                    var questionValueText = "";

                    if (question.Answer != null)
                    {
                        questionValueText = question.Answer.Value == CommonConstants.NA ? question.Answer.Value?.Insert(1, "/").ToUpper() : question.Answer.Value?.ToUpper();
                    }

                    string questionNumber = (++index).ToString();

                    AddQuestionRow(detailedDataTable, detailedDataBorder, questionNumber, question, questionValueText);

                    int subQuestionIndex = 0;

                    foreach (var subQuestion in question.SubQuestions)
                    {
                        if (subQuestion.Answer == null)
                        {
                            continue;
                        }
                        var subQuestionValueText = "";
                        if (subQuestion.Answer != null)
                        {
                            subQuestionValueText = subQuestion.Answer.Value == CommonConstants.NA ? subQuestion.Answer.Value?.Insert(1, "/").ToUpper() : subQuestion.Answer.Value?.ToUpper();
                        }

                        string subQuestionNumber = $"{questionNumber}.{++subQuestionIndex}";

                        AddQuestionRow(detailedDataTable, detailedDataBorder, subQuestionNumber, subQuestion, subQuestionValueText);
                    }
                }
            }

            return detailedDataTable;
        }

        public static Table CreateMdsAuditDetailedData(MdsAuditDetailsDto audit)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);
            var answersHeaderColor = new DeviceRgb(250, 250, 250);

            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var detailedDataBorder = new SolidBorder(detailedDataBorderColor, 1);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(new float[] { 8, 62, 30 });

            var detailedDataTable = new Table(detailedHeaderPercentArray)
                .UseAllAvailableWidth()
                .SetMarginLeft(-20)
                .SetMarginRight(-20);

            foreach (var section in audit.FormVersion.Sections)
            {
                if (section.Name != null)
                {
                    var sectionParagraph = new Paragraph(section.Name != null ? section.Name : string.Empty)
                        .SetFontSize(15)
                        .SetFontColor(new DeviceRgb(38, 50, 55));

                    var sectionCell = new Cell(1, 3)
                        .SetBorder(Border.NO_BORDER)
                        .Add(sectionParagraph)
                        .SetPaddingLeft(20)
                        .SetBackgroundColor(new DeviceRgb(238, 236, 236))
                        .SetBorderBottom(detailedHeaderBorder);
                    detailedDataTable.AddCell(sectionCell);

                    foreach (var group in section.Groups)
                    {
                        if (group.Name != null)
                        {
                            var groupParagraph = new Paragraph(group.Name != null ? group.Name : string.Empty)
                                .SetFontSize(12)
                                .SetFontColor(new DeviceRgb(38, 50, 55));

                            var groupCell = new Cell(1, 3)
                                .SetBorder(Border.NO_BORDER)
                                .Add(groupParagraph)
                                .SetPaddingLeft(20)
                                .SetBackgroundColor(new DeviceRgb(247, 247, 247))
                                .SetBorderBottom(detailedHeaderBorder);
                            detailedDataTable.AddCell(groupCell);

                            detailedDataTable.AddCell(new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(detailedHeaderBorder)
                                .Add(new Paragraph("#"))
                                .SetFontSize(10)
                                .SetFontColor(headerColor)
                                .SetBackgroundColor(answersHeaderColor)
                                .SetPaddingLeft(20));

                            detailedDataTable.AddCell(new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(detailedHeaderBorder)
                                .Add(new Paragraph("QUESTION"))
                                .SetFontSize(10)
                                .SetFontColor(headerColor)
                                .SetBackgroundColor(answersHeaderColor));

                            detailedDataTable.AddCell(new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .SetBorderBottom(detailedHeaderBorder)
                                .Add(new Paragraph("ANSWER"))
                                .SetFontSize(10)
                                .SetFontColor(headerColor)
                                .SetBackgroundColor(answersHeaderColor));



                            int index = 0;

                            foreach (var field in group.FormFields)
                            {
                                if (field.LabelName != null)
                                {
                                    string questionNumber = (++index).ToString();

                                    detailedDataTable.AddCell(new Cell(1, 1)
                                        .SetBorder(Border.NO_BORDER)
                                        .SetBorderBottom(detailedHeaderBorder)
                                        .Add(new Paragraph(questionNumber))
                                        .SetFontSize(10)
                                        .SetFontColor(headerColor)
                                        .SetPaddingLeft(20));

                                    detailedDataTable.AddCell(new Cell(1, 1)
                                        .SetBorder(Border.NO_BORDER)
                                        .SetBorderBottom(detailedHeaderBorder)
                                        .Add(new Paragraph(field.LabelName))
                                        .SetFontSize(10)
                                        .SetFontColor(headerColor));

                                    var formField = audit.FormVersion.FormFields.First(formField => formField.Id == field.Id);

                                    if (formField.Value != null)
                                    {
                                        detailedDataTable.AddCell(new Cell(1, 1)
                                        .SetBorder(Border.NO_BORDER)
                                        .SetBorderBottom(detailedHeaderBorder)
                                        .Add(new Paragraph(HtmlToPlainTextConverter.ConvertHtmlToPlainTextWithRegex(formField.Value.FormattedValue)))
                                        .SetFontSize(10)
                                        .SetFontColor(headerColor));
                                    }
                                    else
                                    {
                                        detailedDataTable.AddCell(new Cell(1, 1)
                                        .SetBorder(Border.NO_BORDER)
                                        .SetBorderBottom(detailedHeaderBorder)
                                        .Add(new Paragraph("---"))
                                        .SetFontSize(10)
                                        .SetFontColor(headerColor));
                                    }

                                }
                            }
                        }
                    }
                }
            }

            return detailedDataTable;
        }

        public static Cell CreateCell() => new Cell().SetBorder(Border.NO_BORDER);

        public static Cell CreateAuditAnswerCell(string text, bool isCompiliance)
        {
            if (!isCompiliance)
            {
                return CreateAuditGrayCell(text);
            }

            switch (text)
            {
                case CommonConstants.YES:
                    return CreateAuditGreenCell(text);
                case CommonConstants.NO:
                    return CreateAuditRedCell(text);
                default:
                    return CreateAuditGrayCell(text);
            }

            Cell CreateAuditGreenCell(string text) => CreateTextCell(text, 12, red: 0, green: 120, blue: 30)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Cell CreateAuditRedCell(string text) => CreateTextCell(text, 12, red: 180, green: 0, blue: 0)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Cell CreateAuditGrayCell(string text) => CreateTextCell(text, 12, red: 80, green: 80, blue: 80)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        public static Cell CreateTextAreaCell(string html)
        {
            var cell = CreateCell();

            IList<IElement> elements = HtmlConverter.ConvertToElements(html);

            ProcessElements(elements);

            foreach (IElement element in elements)
            {
                if (element is IBlockElement)
                {
                    cell.Add((IBlockElement)element);
                }
                else if (element is Text)
                {
                    cell.Add(new Paragraph(element as Text)
                        .SetPaddingTop(0)
                        .SetPaddingBottom(0)
                        .SetMarginTop(2)
                        .SetMarginBottom(2));
                }
            }

            return cell
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        public static Cell CreateTextCell(string text, float fontSize = 10, int red = 0, int green = 0, int blue = 0, float? width = null)
        {
            var paragraph = new Paragraph(text is null ? string.Empty : text);

            if (width.HasValue)
            {
                paragraph.SetWidth(width.Value);
            }

            var color = new DeviceRgb(red, green, blue);

            var cell = CreateCell()
                .Add(paragraph)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(fontSize)
                .SetFontColor(color);

            return cell;
        }

        private static void ProcessElements(IList<IElement> elements)
        {
            foreach (IElement element in elements)
            {
                if (element is Text)
                {
                    Text text = element as Text;

                    text.SetFontSize(10);

                    if (text.HasProperty(94) && text.HasProperty(95))
                    {
                        text.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLDOBLIQUE));
                    }
                    else if (text.HasProperty(94))
                    {
                        text.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE));
                    }
                    else if (text.HasProperty(95))
                    {
                        text.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD));
                    }
                    else
                    {
                        text.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
                    }
                }
                else if (element is IAbstractElement)
                {
                    IList<IElement> children = ((IAbstractElement)element).GetChildren();

                    ProcessElements(children);
                }

                if (element is Paragraph)
                {
                    Paragraph paragraph = element as Paragraph;

                    paragraph.SetPaddingTop(0)
                            .SetPaddingBottom(0)
                            .SetMarginTop(2)
                            .SetMarginBottom(2);
                }

                if (element is List)
                {
                    List list = element as List;

                    list.SetPaddingTop(0)
                            .SetPaddingBottom(0)
                            .SetPaddingLeft(2)
                            .SetPaddingRight(2)
                            .SetMarginTop(2)
                            .SetMarginBottom(2);
                }
            }
        }

        private static void AddQuestionRow(Table table, SolidBorder detailedDataBorder, string rowNumber, CriteriaQuestionDto question, string valueText)
        {
            var number = CreateAuditCell(rowNumber)
                       .SetPaddingLeft(20)
                       .SetBorderBottom(detailedDataBorder);

            SetFontColor(number, valueText, question.CriteriaOption.Compliance);

            table.AddCell(number);

            var questionName = CreateAuditCell(question.Value)
                .SetPaddingLeft(5)
                .SetPaddingRight(5)
                .SetBorderBottom(detailedDataBorder);

            SetFontColor(questionName, valueText, question.CriteriaOption.Compliance);

            table.AddCell(questionName);

            var value = CreateAuditAnswerCell(valueText, question.CriteriaOption.Compliance)
                .SetPaddingLeft(5)
                .SetPaddingRight(5)
                .SetBorderBottom(detailedDataBorder);

            table.AddCell(value);

            if (question.Answer != null && question.Answer.AuditorComment != null)
            {
                var comment = CreateTextAreaCell(question.Answer.AuditorComment);
                SetFontColor(comment, valueText, question.CriteriaOption.Compliance);

                comment
                    .SetMaxWidth(200f)
                    .SetPaddingLeft(5)
                    .SetPaddingRight(20)
                    .SetBorderBottom(detailedDataBorder);

                table.AddCell(comment);
            }
            else
            {
                var comment = CreateTextAreaCell("");
                SetFontColor(comment, valueText, question.CriteriaOption.Compliance);

                comment
                    .SetMaxWidth(200f)
                    .SetPaddingLeft(5)
                    .SetPaddingRight(20)
                    .SetBorderBottom(detailedDataBorder);

                table.AddCell(comment);
            }




            //Cell CreateAuditGrayCell(string text) => CreateAuditCell(text, 80, 80, 80);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                CreateTextCell(text, 12, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private static void SetFontColor(Cell cell, string value, bool isCompliance)
        {
            if (value == CommonConstants.NA_SLASH || !isCompliance)
            {
                cell.SetFontColor(new DeviceRgb(80, 80, 80));
            }
        }

        internal static Table CreateByMonthData(FallReportDto fallReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            //Type of Fall
            columnPercentages.Add(15);

            // Remaining - 70%

            var eachMonth = 70 / fallReport.ByMonth.Count;

            foreach (var month in fallReport.ByMonth)
            {
                columnPercentages.Add(eachMonth);
            }

            //Total
            columnPercentages.Add(10);

            //Percentage
            columnPercentages.Add(15);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Analysis and Trend

            table.AddCell(new Cell(1, fallReport.ByMonth.Count + 3)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Fall Analysis and Trend"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("FALL"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(month.Name))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Percentage"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );

            //Total Falls

            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Total Fall"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.Total}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }


            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.Total)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );



            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("100%"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            //Falls with Major Injury

            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Falls with Major Injury"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.MajorInjury}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }


            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.MajorInjury)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            var total = Convert.ToDouble(fallReport.ByMonth.Sum(month => month.Total));
            var fallMajorTotal = Convert.ToDouble(fallReport.ByMonth.Sum(month => month.MajorInjury));
            var fallMajorPercentage = Math.Round((fallMajorTotal / total) * 100);



            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallMajorPercentage}%"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            //Sent To Hospital

            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Sent To Hospital"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.SentToHospital}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }


            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.SentToHospital)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );


            var sentToHospitalTotal = Convert.ToDouble(fallReport.ByMonth.Sum(month => month.SentToHospital));
            var sentToHospitalPercentage = Math.Round((sentToHospitalTotal / total) * 100);

            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{sentToHospitalPercentage}%"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );


            return table;
        }

        internal static Table CreateByMonthData(WoundReportDto woundReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            float[] columnPercentages = { 25, 22, 22, 22 };

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages);

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Analysis and Trend

            table.AddCell(new Cell(1, 4)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Analysis and Trend"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Month"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Wound incidents"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("In-House Acquired"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Re-Hospitalization"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            foreach (var month in woundReport.byMonths)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(month.Name))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.Total}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.InHouseAcquired}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ReHospitalization}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{woundReport.byMonths.Sum(month => month.Total)}"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );

            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{woundReport.byMonths.Sum(month => month.InHouseAcquired)}"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{woundReport.byMonths.Sum(month => month.ReHospitalization)}"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );

            return table;
        }

        internal static Table CreateByDayData(FallReportDto fallReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            // Day of Week
            columnPercentages.Add(15);

            // Remaining - 75%
            var eachMonth = 75 / fallReport.ByMonth.Count;

            foreach (var month in fallReport.ByMonth)
            {
                columnPercentages.Add(eachMonth);
            }

            //Total
            columnPercentages.Add(10);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Summary by Day of Week
            table.AddCell(new Cell(1, fallReport.ByMonth.Count + 2)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Fall Summary by Day of Week"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Day of Week"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBold()
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(month.Name))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            // Sunday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Sunday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Sunday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Sunday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            //Monday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Monday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Monday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Monday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            // Tuesday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Tuesday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Tuesday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Tuesday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            // Wednesday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Wednesday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Wednesday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Wednesday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            // Thursday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Thursday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Thursday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Thursday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            // Friday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Friday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Friday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Friday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );

            // Saturday
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph("Saturday"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );
            foreach (var month in fallReport.ByMonth)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{month.ByDay.Saturday}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }
            table.AddCell(new Cell(1, 1)
               .SetBorder(Border.NO_BORDER)
               .SetBorderBottom(detailedHeaderBorder)
               .Add(new Paragraph($"{fallReport.ByMonth.Sum(month => month.ByDay.Saturday)}"))
               .SetFontSize(11)
               .SetFontColor(headerColor)
               );



            return table;
        }

        internal static Table CreateByShiftData(FallReportDto fallReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            // Day of Week
            columnPercentages.Add(25);
            columnPercentages.Add(50);
            columnPercentages.Add(20);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Summary by Day of Week
            table.AddCell(new Cell(1, 3)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Fall Summary by Shift"))
               .SetFontSize(13)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetBold()
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Shift"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );

            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Time of Incident"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total Incidents"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            foreach (var shift in fallReport.ByShift.FindAll(byShift => byShift.ByTime.Count > 0))
            {
                table.AddCell(new Cell(shift.ByTime.Count, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(shift.Name))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                foreach (var time in shift.ByTime)
                {

                    table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph(time.Name))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );

                    table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{time.Count}"))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );
                }
            }

            table.AddCell(
                new Cell(1, 2)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("TOTAL"))
                    .SetBold()
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
            );

            table.AddCell(
                new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{fallReport.ByShift.Sum(byShift => byShift.ByTime.Sum(byTime => byTime.Count))}"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
            );

            return table;
        }

        internal static Table CreateByActivityData(FallReportDto fallReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            // Day of Week
            columnPercentages.Add(50);
            columnPercentages.Add(40);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Summary by Activity Prior to Incident
            table.AddCell(new Cell(1, 2)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Fall Summary by Activity Prior to Incident"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Activity"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );



            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total Incidents"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            foreach (var activity in fallReport.ByActivity)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(activity.Name))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{activity.Count}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }

            table.AddCell(
                new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("TOTAL"))
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
            );

            table.AddCell(
                new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{fallReport.ByActivity.Sum(byActivity => byActivity.Count)}"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
            );

            return table;
        }

        internal static Table CreateByPlaceData(FallReportDto fallReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            // Day of Week
            columnPercentages.Add(50);
            columnPercentages.Add(40);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Fall Summary by Place of Incident
            table.AddCell(new Cell(1, 2)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Fall Summary by Place of Incident"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Place of Incident"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );



            table.AddCell(new Cell(1, 1)
            .SetBorder(Border.NO_BORDER)
            .SetBorderBottom(detailedHeaderBorder)
            .Add(new Paragraph("Total Incidents"))
            .SetFontSize(11)
            .SetBold()
            .SetFontColor(headerColor)
            );

            foreach (var activity in fallReport.ByPlace)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(activity.Name))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );

                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph($"{activity.Count}"))
                .SetFontSize(11)
                .SetFontColor(headerColor)
                );
            }

            table.AddCell(
                new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("TOTAL"))
                    .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
            );

            table.AddCell(
                new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{fallReport.ByPlace.Sum(byPlace => byPlace.Count)}"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
            );

            return table;
        }

        internal static Table CreateByUlcerData(WoundReportDto woundReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            //Ulcer Stage
            columnPercentages.Add(25);

            // Remaining - 60%
            var eachMonth = 60 / woundReport.byMonths.Count;
            foreach (var month in woundReport.byMonths)
            {
                columnPercentages.Add(eachMonth);
            }
            //Percentage
            columnPercentages.Add(15);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Wound Summary by Pressure Ulcer Stage
            table.AddCell(new Cell(1, woundReport.byMonths.Count + 2)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Summary by Pressure Ulcer Stage"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Pressure Ulcer Stage"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            foreach (var month in woundReport.byMonths)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(month.Name))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            }

            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Percentage"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );


            var total = Convert.ToDouble(woundReport.byMonths.Sum(month => month.byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase)).Sum(type => type.Count)));

            foreach (var ulcerType in woundReport.byMonths[0].byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase)).OrderBy(type => type.Name.Replace("Pressure Ulcer", "")))
            {

                //var totalByUlcerType = Convert.ToDouble(woundReport.byMonths.Sum(month => month.byTypes.Find(type => type.Name == ulcerType.Name).Count));

                var totalByUlcerType = 0;

                foreach (var month in woundReport.byMonths)
                {
                    var byTypes = month.byTypes.Find(type => type.Name == ulcerType.Name);
                    if (byTypes != null)
                    {
                        totalByUlcerType += byTypes.Count;
                    }
                }

                var percentage = Math.Round((totalByUlcerType / total) * 100);


                table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph(ulcerType.Name.Replace("Pressure Ulcer", "")))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );

                foreach (var month in woundReport.byMonths)
                {
                    var _ulcerType = month.byTypes.Find(type => type.Name == ulcerType.Name);
                    if (_ulcerType != null)
                    {
                        table.AddCell(new Cell(1, 1)
                        .SetBorder(Border.NO_BORDER)
                        .SetBorderBottom(detailedHeaderBorder)
                        .Add(new Paragraph($"{_ulcerType.Count}"))
                        .SetFontSize(11)
                        .SetFontColor(headerColor)
                        );
                    }

                }

                table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{percentage}%"))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );
            }

            table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("Total"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
                    );


            foreach (var month in woundReport.byMonths)
            {
                var totalByMonth = month.byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase)).Sum(type => type.Count);
                table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{totalByMonth}"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
                    );
            }

            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("100%"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );


            return table;
        }

        internal static Table CreateByNotUlcerData(WoundReportDto woundReport)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);
            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var headerColor = new DeviceRgb(38, 50, 55);
            List<float> columnPercentages = new List<float>();

            //Summary by Skin Types
            columnPercentages.Add(25);

            // Remaining - 75%
            var eachMonth = 75 / woundReport.byMonths.Count;
            foreach (var month in woundReport.byMonths)
            {
                columnPercentages.Add(eachMonth);
            }
            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            // Summary by Skin Types
            table.AddCell(new Cell(1, woundReport.byMonths.Count + 1)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Summary by Skin Types"))
               .SetFontSize(13)
               .SetBold()
               .SetTextAlignment(TextAlignment.CENTER)
               .SetMarginBottom(30)
               .SetFontColor(headerColor)
               );


            // Header
            table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph("Skin Types"))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            foreach (var month in woundReport.byMonths)
            {
                table.AddCell(new Cell(1, 1)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(detailedHeaderBorder)
                .Add(new Paragraph(month.Name))
                .SetFontSize(11)
                .SetBold()
                .SetFontColor(headerColor)
                );
            }

            foreach (var ulcerType in woundReport.byMonths[0].byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase) == false).OrderBy(type => type.Name))
            {


                table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph(ulcerType.Name))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );

                foreach (var month in woundReport.byMonths)
                {
                    var _ulcerType = month.byTypes.Find(type => type.Name == ulcerType.Name);
                    table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{_ulcerType.Count}"))
                    .SetFontSize(11)
                    .SetFontColor(headerColor)
                    );
                }

            }

            table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph("Total"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
                    );


            foreach (var month in woundReport.byMonths)
            {
                var totalByMonth = month.byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase) == false).Sum(type => type.Count);
                table.AddCell(new Cell(1, 1)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderBottom(detailedHeaderBorder)
                    .Add(new Paragraph($"{totalByMonth}"))
                    .SetFontSize(11)
                    .SetBold()
                    .SetFontColor(headerColor)
                    );
            }

            return table;
        }

        internal static void AddByMonthDataBarChart(FallReportDto fallReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);


            string[] groupNames = { "Total falls", "Falls with Major Injury", "Sent to Hospital" };

            List<string> seriesNames = new List<string>();

            List<double[]> valuesBySeries = new List<double[]>();
            List<double[]> errorsBySeries = new List<double[]>();

            foreach (var month in fallReport.ByMonth)
            {
                seriesNames.Add(month.Name);
                double[] values = { month.Total, month.MajorInjury, month.SentToHospital };
                double[] errors = { 0, 0, 0 };
                valuesBySeries.Add(values);
                errorsBySeries.Add(errors);
            }

            if (groupNames.Length > 0)
            {
                var barGroups = plt.AddBarGroups(groupNames, seriesNames.ToArray(), valuesBySeries.ToArray(), errorsBySeries.ToArray());

                foreach (var barGroup in barGroups)
                {
                    barGroup.ShowValuesAboveBars = true;
                }
                plt.Legend(location: ScottPlot.Alignment.UpperRight);
                plt.SetAxisLimits(yMin: 0);
                plt.Title("Fall Analysis and Trend", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }


        }

        internal static void AddByMonthDataBarChart(WoundReportDto woundReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);
            string[] groupNames = { "Wound Incidents", "In-House Acquired", "Re-Hospitalization" };

            List<string> seriesNames = new List<string>();

            List<double[]> valuesBySeries = new List<double[]>();
            List<double[]> errorsBySeries = new List<double[]>();

            foreach (var month in woundReport.byMonths)
            {
                seriesNames.Add(month.Name);
                double[] values = { month.Total, month.InHouseAcquired, month.ReHospitalization };
                double[] errors = { 0, 0, 0 };
                valuesBySeries.Add(values);
                errorsBySeries.Add(errors);
            }

            if (groupNames.Length > 0)
            {
                var barGroups = plt.AddBarGroups(groupNames, seriesNames.ToArray(), valuesBySeries.ToArray(), errorsBySeries.ToArray());
                foreach (var barGroup in barGroups)
                {
                    barGroup.ShowValuesAboveBars = true;
                }
                plt.Legend(location: ScottPlot.Alignment.UpperRight);
                plt.SetAxisLimits(yMin: 0);
                plt.Title("Analysis and Trend", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }


        }

        internal static void AddByWeekDayDataBarChart(FallReportDto fallReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);
            string[] groupNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            List<string> seriesNames = new List<string>();

            List<double[]> valuesBySeries = new List<double[]>();
            List<double[]> errorsBySeries = new List<double[]>();

            foreach (var month in fallReport.ByMonth)
            {
                seriesNames.Add(month.Name);

                double[] values = { month.ByDay.Sunday, month.ByDay.Monday, month.ByDay.Tuesday, month.ByDay.Wednesday, month.ByDay.Thursday, month.ByDay.Friday, month.ByDay.Saturday };
                double[] errors = { 0, 0, 0, 0, 0, 0, 0 };
                valuesBySeries.Add(values);
                errorsBySeries.Add(errors);
            }
            if (groupNames.Length > 0)
            {
                var barGroups = plt.AddBarGroups(groupNames.ToArray(), seriesNames.ToArray(), valuesBySeries.ToArray(), errorsBySeries.ToArray());
                foreach (var barGroup in barGroups)
                {
                    barGroup.ShowValuesAboveBars = true;
                }
                plt.Legend(location: ScottPlot.Alignment.UpperRight);
                plt.SetAxisLimits(yMin: 0);
                plt.Title("Fall Summary by Day of Week", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }

        }

        internal static void AddByShiftDayDataBarChart(FallReportDto fallReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 600);

            List<double> values = new List<double>();
            List<double> positions = new List<double>();
            List<string> labels = new List<string>();

            var i = 0;

            foreach (var shift in fallReport.ByShift)
            {
                foreach (var time in shift.ByTime.FindAll(time => time.Count > 0))
                {
                    values.Add(time.Count);
                    positions.Add(i);
                    labels.Add(time.Name);
                    i++;
                }
            }
            if (values.Count > 0)
            {
                var bar = plt.AddBar(values.ToArray(), positions.ToArray());
                bar.ShowValuesAboveBars = true;
                bar.BarWidth = (1.0 / values.Count) * .8;

                plt.XTicks(positions.ToArray(), labels.ToArray());
                plt.SetAxisLimits(yMin: 0);

                plt.Title("Fall Summary by Shift", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }


        }

        internal static void AddByActivityDayDataBarChart(FallReportDto fallReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);

            List<double> values = new List<double>();
            List<double> positions = new List<double>();
            List<string> labels = new List<string>();

            var i = 0;

            foreach (var activity in fallReport.ByActivity)
            {
                values.Add(activity.Count);
                positions.Add(i);
                labels.Add(activity.Name);
                i++;
            }
            if (values.Count > 0)
            {
                var bar = plt.AddBar(values.ToArray(), positions.ToArray());
                bar.ShowValuesAboveBars = true;
                bar.BarWidth = (1.0 / values.Count) * .8;

                plt.XTicks(positions.ToArray(), labels.ToArray());
                plt.SetAxisLimits(yMin: 0);

                plt.Title("Fall Summary by Activity Prior to Incident", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }

        }

        internal static void AddByPlaceDataBarChart(FallReportDto fallReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);

            List<double> values = new List<double>();
            List<double> positions = new List<double>();
            List<string> labels = new List<string>();

            var i = 0;

            foreach (var place in fallReport.ByPlace)
            {
                values.Add(place.Count);
                positions.Add(i);
                labels.Add(place.Name);
                i++;
            }
            if (values.Count > 0)
            {
                var bar = plt.AddBar(values.ToArray(), positions.ToArray());
                bar.BarWidth = (1.0 / values.Count) * .8;
                bar.ShowValuesAboveBars = true;

                plt.XTicks(positions.ToArray(), labels.ToArray());
                plt.SetAxisLimits(yMin: 0);

                plt.Title("Fall Summary by Place of Incident", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }

        }

        internal static void AddByUlcerDataBarChart(WoundReportDto woundReport, Document document, PdfDocument pdfDocument)
        {
            var pageSize = pdfDocument.GetDefaultPageSize();

            var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()), 400);

            List<string> seriesNames = new List<string>(); // January, February, etc
            List<string> groupNames = new List<string>(); // Stage 1, Stage 2, etc

            List<double[]> valuesBySeries = new List<double[]>();
            List<double[]> errorsBySeries = new List<double[]>();

            foreach (var ulcerType in woundReport.byMonths[0].byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase)).OrderBy(type => type.Name.Replace("Pressure Ulcer", "")))
            {
                groupNames.Add(ulcerType.Name.Replace("Pressure Ulcer", ""));
            }

            foreach (var month in woundReport.byMonths)
            {
                seriesNames.Add(month.Name);

                List<double> values = new List<double>();
                List<double> errors = new List<double>();

                foreach (var ulcerType in month.byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase)).OrderBy(type => type.Name.Replace("Pressure Ulcer", "")))
                {
                    values.Add(ulcerType.Count);
                    errors.Add(0);
                }

                valuesBySeries.Add(values.ToArray());
                errorsBySeries.Add(errors.ToArray());
            }

            if (groupNames.Count > 0)
            {
                var barGroups = plt.AddBarGroups(groupNames.ToArray(), seriesNames.ToArray(), valuesBySeries.ToArray(), errorsBySeries.ToArray());
                foreach (var barGroup in barGroups)
                {
                    barGroup.ShowValuesAboveBars = true;
                }
                plt.Legend(location: ScottPlot.Alignment.UpperRight);
                plt.SetAxisLimits(yMin: 0);
                plt.Title("Summary by Pressure Ulcer Stage", false);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                document.Add(img);
            }


        }

        internal static void AddByNotUlcerDataLineChart(WoundReportDto woundReport, Document document, PdfDocument pdfDocument)
        {

            var pageSize = pdfDocument.GetDefaultPageSize();

            float[] columnPercentages = { 45, 45 };
            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages);
            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .SetBorder(Border.NO_BORDER)
                .UseAllAvailableWidth();


            foreach (var ulcerType in woundReport.byMonths[0].byTypes.FindAll(type => type.Name.Contains("stage", StringComparison.OrdinalIgnoreCase) == false).OrderBy(type => type.Name))
            {
                var plt = new ScottPlot.Plot(Convert.ToInt16(pageSize.GetWidth()) / 2 - 20, 300);

                List<double> values = new List<double>();
                List<double> points = new List<double>();
                List<string> labels = new List<string>();

                var i = 0;
                foreach (var month in woundReport.byMonths)
                {
                    var type = month.byTypes.First(byType => byType.Name == ulcerType.Name);
                    values.Add(type.Count);
                    points.Add(i);
                    labels.Add(month.Name);
                    i++;
                }

                plt.Title(ulcerType.Name, false);
                plt.AddSignal(values.ToArray());
                plt.XAxis.ManualTickPositions(points.ToArray(), labels.ToArray());
                plt.SetAxisLimits(yMin: 0);

                var image = plt.GetImageBytes();

                var data = ImageDataFactory.Create(image, false);
                var img = new Image(data).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                //document.Add(img);
                table.AddCell(new Cell().Add(img).SetBorder(Border.NO_BORDER));
                //table.AddCell(img);
            }

            document.Add(table);

        }

        internal static (Table, string) CreateByReportAIData(string jsonResult)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);
            var answersHeaderColor = new DeviceRgb(250, 250, 250);

            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var detailedDataBorder = new SolidBorder(detailedDataBorderColor, 1);

            var keywordColor = new DeviceRgb(255, 0, 0);
            List<float> columnPercentages = new List<float>();

            //Ulcer Stage
            columnPercentages.Add(100);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            JObject aiRep = JObject.Parse(jsonResult);
            string filteredDate = "";
            try
            {
                foreach (JProperty property in aiRep.Properties())
                {
                    if (property.Value.HasValues)
                    {

                        if (property.Value.Type == JTokenType.Array)
                        {
                            var items = (JArray)property.Value;
                            int acceptable = 1;
                            string ID = string.Empty;
                            string residentName = string.Empty;
                            string aiSuggesion = string.Empty;
                            string userSummary = string.Empty;
                            
                            string date = string.Empty;
                            bool addKeyword = false;
                            foreach (JObject content in items.Children<JObject>())
                            {
                                foreach (JProperty prop in content.Properties())
                                {
                                    if (prop.Name.ToUpper() == "Acceptable".ToUpper())
                                    {
                                        acceptable = (int)prop.Value;
                                        continue;
                                    }

                                    if (prop.Name.ToUpper() == "ID")
                                    {
                                        ID = prop.Value.ToString();
                                        continue;
                                    }

                                    if (prop.Name.ToUpper() == "Name".ToUpper())
                                    {
                                        residentName = prop.Value.ToString();
                                        continue;
                                    }
                                    if (prop.Name.ToUpper() == "Summary".ToUpper())
                                    {
                                        aiSuggesion = prop.Value.ToString();
                                        continue;
                                    }
                                    if (prop.Name.ToUpper() == "UserSummary".ToUpper())
                                    {
                                        userSummary = prop.Value.ToString();
                                        continue;
                                    }
                                    if (prop.Name.ToUpper() == "Date".ToUpper())
                                    {

                                        var dateTIme = DateTime.Parse(prop.Value.ToString());

                                        date = dateTIme.ToShortDateString();
                                        if (string.IsNullOrEmpty(filteredDate))
                                            filteredDate = date;
                                        continue;
                                    }
                                    // This is not allowed 
                                    //here more code in order to save in a database
                                }

                                if (acceptable == 1 && !string.IsNullOrEmpty(residentName) )
                                {
                                    if (!addKeyword)
                                    {
                                        table.AddCell(new Cell(1, 1)
                                         .SetBorder(Border.NO_BORDER)
                                         .SetBorderBottom(detailedHeaderBorder)
                                         .Add(new Paragraph(property.Name.ToUpper()))
                                         .SetFontSize(14)
                                         .SetFontColor(keywordColor)
                                         );
                                        addKeyword = true;
                                    }
                                    string first = $"{residentName} ({ID})    {date}";
                                    table.AddCell(new Cell(1, 1)
                                        .SetBorder(Border.NO_BORDER)
                                        .Add(new Paragraph(first))
                                        .SetFontSize(14)
                                        .SetFontColor(headerColor)
                                        );

                                    var comment = userSummary.Length == 0 ? aiSuggesion : userSummary;
                                    table.AddCell(new Cell(1, 1)
                                         .SetBorder(Border.NO_BORDER)
                                         .SetBorderBottom(detailedHeaderBorder)
                                         .Add(new Paragraph(comment))
                                         .SetFontSize(14)
                                         .SetFontColor(headerColor)
                                         );
                                }
                            }
                            // Proceed as before.
                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }

            return (table, filteredDate);
        }

        internal static Table CreateKeywordsForAIReport(string jsonResult, string jsonKeywords)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);
            var answersHeaderColor = new DeviceRgb(250, 250, 250);

            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var detailedDataBorder = new SolidBorder(detailedDataBorderColor, 1);

            var keywordColor = new DeviceRgb(255, 0, 0);
            float[] columnPercentages = { 20, 20, 20,20 ,20};
            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            IList<string> keywordFound = new List<string>();
            try
            {
                JObject aiRep = JObject.Parse(jsonResult);


                foreach (JProperty property in aiRep.Properties())
                {
                    if (property.Value.HasValues)
                    {
                        if (property.Value.Type == JTokenType.Array)
                        {
                            var items = (JArray)property.Value;
                            foreach (JObject content in items.Children<JObject>())
                            {
                                foreach (JProperty prop in content.Properties())
                                {

                                    if (prop.Name == "Acceptable")
                                    {
                                        if ((int)prop.Value == 1)
                                        {
                                            if (!keywordFound.Contains(property.Name))
                                            {
                                                keywordFound.Add(property.Name.ToUpper());
                                                break;
                                            }

                                        }
                                    }

                                }

                            }
                        }
                    }
                }

                var keywordsFoundDistinct = keywordFound.Distinct();
                List<string> keywordsForReport = jsonKeywords.Replace("\r\n", "").Replace("[", "")
                   .Replace("]", "").Replace("\"", "").Split(",").ToList(); //keywords.Properties().Select(x => x.Name).ToList();
                int column = 0;
                int columnCount = 5;
                int row = 1;
                int col = 1;

                foreach (string keyword in keywordsForReport)
                {
                    var cellColor = headerColor;

                    if (keywordsFoundDistinct.Any(x => x.Equals(keyword.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                        cellColor = keywordColor;

                    var cell = new Cell(row, col)
                      .SetBorder(Border.NO_BORDER)
                      .Add(new Paragraph(keyword.ToUpper()))
                      .SetFontSize(9)
                      .SetFontColor(cellColor);

                    table.AddCell(cell);

                    column = ++column % columnCount;
                    if (column == 0)
                    {
                        row++;
                        col = 1;
                    }

                }


            }
            catch (Exception ex)
            {
                string w = ex.Message;
            }

            return table;
        }

        internal static (Table,IList<Tuple<string, bool>>) CreateByReportAIDataV2(AuditAIReportV2 audit)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);
            var answersHeaderColor = new DeviceRgb(250, 250, 250);

            var detailedHeaderBorder = new SolidBorder(detailedDataBorderColor, 1.5f);
            var detailedDataBorder = new SolidBorder(detailedDataBorderColor, 1);

            var keywordColor = new DeviceRgb(255, 0, 0);
            List<float> columnPercentages = new List<float>();

            //Ulcer Stage
            columnPercentages.Add(100);

            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());
            var keywordList = new List<Tuple<string, bool>>();
            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            try
            {
                var newDoc = new Dictionary<string,IList<AuditAIPatientPdfNotes>>();
                foreach (var patientInfo in audit.Values)
                {
                    if (patientInfo.Summaries.Any())
                    {
                        var keywords = patientInfo.Summaries.Select(x => x.Keyword).Distinct();
                        foreach( var key in keywords)
                        {
                            if(newDoc.ContainsKey(key))
                            {
                                var listExist = newDoc[key];
                                listExist.Add(patientInfo);
                                newDoc[key] = listExist;
                            }
                            else
                            {
                                var listPatients = new List<AuditAIPatientPdfNotes>();
                                listPatients.Add(patientInfo);
                                newDoc.Add(key, listPatients);
                            }
                        }
                    }
                }

               
                foreach(var key in newDoc.Keys)
                {
                    bool addKeyword = false;
                    var values = newDoc[key];

                    if (!values.Any())
                        continue;

                    bool keyAccepted = false;
                    var keywordAdded = false;
                    foreach (var patientInfo in values)
                    {
                        var summaries = patientInfo.Summaries.Where(x => x.Accept == true && x.Keyword.ToLower() == key.ToLower());
                        if( summaries.Any())
                        {
                            if (keywordAdded == false)
                            {
                                table.AddCell(new Cell(1, 1)
                                     .SetBorder(Border.NO_BORDER)
                                     .SetBorderBottom(detailedHeaderBorder)
                                     .Add(new Paragraph(key.ToUpper()))
                                     .SetFontSize(14)
                                     .SetFontColor(keywordColor));
                                keywordAdded = true;
                            }


                            string first = $"{patientInfo.PatientName} ({patientInfo.PatientId})    {patientInfo.DateTime}";
                            table.AddCell(new Cell(1, 1)
                                .SetBorder(Border.NO_BORDER)
                                .Add(new Paragraph(first))
                                .SetFontSize(14)
                                .SetFontColor(headerColor)
                                );

                            keyAccepted = true;
                            foreach (var summary in summaries )
                            {
                                table.AddCell(new Cell(1, 1)
                                     .SetBorder(Border.NO_BORDER)
                                     .SetBorderBottom(detailedHeaderBorder)
                                     .Add(new Paragraph(summary.Summary))
                                     .SetFontSize(14)
                                     .SetFontColor(headerColor)
                                     );
                            }
                        }

                    }


                    keywordList.Add(new Tuple<string, bool>(key, keyAccepted));

                }
                     
                    //    if (property.Value.Type == JTokenType.Array)
                    //    {
                    //        var items = (JArray)property.Value;
                    //        int acceptable = 1;
                    //        string ID = string.Empty;
                    //        string residentName = string.Empty;
                    //        string aiSuggesion = string.Empty;
                    //        string userSummary = string.Empty;

                    //        string date = string.Empty;
                    //        bool addKeyword = false;
                    //        foreach (JObject content in items.Children<JObject>())
                    //        {
                    //            foreach (JProperty prop in content.Properties())
                    //            {
                    //                if (prop.Name.ToUpper() == "Acceptable".ToUpper())
                    //                {
                    //                    acceptable = (int)prop.Value;
                    //                    continue;
                    //                }

                    //                if (prop.Name.ToUpper() == "ID")
                    //                {
                    //                    ID = prop.Value.ToString();
                    //                    continue;
                    //                }

                    //                if (prop.Name.ToUpper() == "Name".ToUpper())
                    //                {
                    //                    residentName = prop.Value.ToString();
                    //                    continue;
                    //                }
                    //                if (prop.Name.ToUpper() == "Summary".ToUpper())
                    //                {
                    //                    aiSuggesion = prop.Value.ToString();
                    //                    continue;
                    //                }
                    //                if (prop.Name.ToUpper() == "UserSummary".ToUpper())
                    //                {
                    //                    userSummary = prop.Value.ToString();
                    //                    continue;
                    //                }
                    //                if (prop.Name.ToUpper() == "Date".ToUpper())
                    //                {

                    //                    var dateTIme = DateTime.Parse(prop.Value.ToString());

                    //                    date = dateTIme.ToShortDateString();
                    //                    if (string.IsNullOrEmpty(filteredDate))
                    //                        filteredDate = date;
                    //                    continue;
                    //                }
                    //                // This is not allowed 
                    //                //here more code in order to save in a database
                    //            }

                    //            if (acceptable == 1 && !string.IsNullOrEmpty(residentName))
                    //            {
                    //                if (!addKeyword)
                    //                {
                    //                    table.AddCell(new Cell(1, 1)
                    //                     .SetBorder(Border.NO_BORDER)
                    //                     .SetBorderBottom(detailedHeaderBorder)
                    //                     .Add(new Paragraph(property.Name.ToUpper()))
                    //                     .SetFontSize(14)
                    //                     .SetFontColor(keywordColor)
                    //                     );
                    //                    addKeyword = true;
                    //                }
                    //                string first = $"{residentName} ({ID})    {date}";
                    //                table.AddCell(new Cell(1, 1)
                    //                    .SetBorder(Border.NO_BORDER)
                    //                    .Add(new Paragraph(first))
                    //                    .SetFontSize(14)
                    //                    .SetFontColor(headerColor)
                    //                    );

                    //                var comment = userSummary.Length == 0 ? aiSuggesion : userSummary;
                    //                table.AddCell(new Cell(1, 1)
                    //                     .SetBorder(Border.NO_BORDER)
                    //                     .SetBorderBottom(detailedHeaderBorder)
                    //                     .Add(new Paragraph(comment))
                    //                     .SetFontSize(14)
                    //                     .SetFontColor(headerColor)
                    //                     );
                    //            }
                    //        }
                    //        // Proceed as before.
                    //    }

                    //}

                //}
            }
            catch (Exception ex)
            {

            }

            return (table, keywordList);
        }

        internal static Table CreateKeywordsForAIReportV2(IList<Tuple<string, bool>> keyList)
        {
            var detailedDataBorderColor = new DeviceRgb(100, 100, 100);

            var headerColor = new DeviceRgb(38, 50, 55);

            var keywordColor = new DeviceRgb(255, 0, 0);
            float[] columnPercentages = { 20, 20, 20, 20, 20 };
            var detailedHeaderPercentArray = UnitValue.CreatePercentArray(columnPercentages.ToArray());

            var table = new Table(detailedHeaderPercentArray)
                .SetMarginTop(20)
                .UseAllAvailableWidth();

            IList<string> keywordFound = new List<string>();
            try
            {
 
                int column = 0;
                int columnCount = 5;
                int row = 1;
                int col = 1;

                foreach (var keyword in keyList)
                {
                    var cellColor = headerColor;

                    if (keyword.Item2 == true)
                        cellColor = keywordColor;

                    var cell = new Cell(row, col)
                      .SetBorder(Border.NO_BORDER)
                      .Add(new Paragraph(keyword.Item1.ToUpper()))
                      .SetFontSize(9)
                      .SetFontColor(cellColor);

                    table.AddCell(cell);

                    column = ++column % columnCount;
                    if (column == 0)
                    {
                        row++;
                        col = 1;
                    }

                }


            }
            catch (Exception ex)
            {
                string w = ex.Message;
            }

            return table;
        }
    }
}