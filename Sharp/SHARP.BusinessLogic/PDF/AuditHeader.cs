using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using SHARP.BusinessLogic.DTO.Audit;
using SHARP.BusinessLogic.Properties;
using SHARP.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.BusinessLogic.PDF
{
    internal class AuditHeaderEventHandler : IEventHandler
    {
        protected const string DATE_FORMAT = "MMM dd, yyyy";
        private readonly PdfFont _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        private readonly Document _document;
        private readonly float _height;
        private readonly Table _deviderTop;
        private readonly Table _deviderBottom;
        private readonly Table _auditHeaders;

        protected readonly Rectangle _pageSize;
        
        private readonly string _headerTitle;

        protected int _recordsCount = 0;        

        public float Height => _height;

        private int _utcOffset = 0;
        private string _timeZoneShortName;

        public int UtcOffset => _utcOffset;

        public string TimeZoneShortName => _timeZoneShortName;
        AuditDto _audit = null;
        protected IReadOnlyCollection<AuditDto> _audits = null;
        public virtual void SetRecordsCount()
        {
            var trackerAudit = this._audit as TrackerAuditDetailsDto;
            _recordsCount = trackerAudit != null ? trackerAudit.PivotAnswerGroups.Count : 0;
        }

        ImageData _logoData;

        internal AuditHeaderEventHandler(Document document, List<AuditDto> audits, string headerTitle)
        {
            _document = document;
            _pageSize = document.GetPdfDocument().GetDefaultPageSize();
            _headerTitle = headerTitle;
            _audit = audits.First();
            _audits = audits;

            _logoData = ImageDataFactory.Create(Resources.Logo);

            SetRecordsCount();

            _utcOffset = _audit.Facility.TimeZoneOffset;
            _timeZoneShortName = _audit.Facility.TimeZoneShortName;

            var title = CreateTitle(1, 1);
            var titleHeight = HeightCalculator.GetHeight(title, _document);

            _deviderTop = CreateDevider()
                .SetMarginTop(10)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);
            var deviderTopHeight = HeightCalculator.GetHeight(_deviderTop, _document);

            _auditHeaders = CreateAudit(_audit);
            var auditHeadersHeight = HeightCalculator.GetHeight(_auditHeaders, _document);

            _deviderBottom = CreateDevider()
                .SetMarginLeft(-20)
                .SetMarginRight(-20)
                .SetMarginBottom(0);
            var deviderBottomHeight = HeightCalculator.GetHeight(_deviderBottom, _document);

            _height = titleHeight + deviderTopHeight + auditHeadersHeight + deviderBottomHeight + 15;
            
        }

        public void HandleEvent(Event @event)
        {
            var documentEvent = (PdfDocumentEvent)@event;
            var pdfDocument = documentEvent.GetDocument();
            var page = documentEvent.GetPage();
            var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDocument);
            var pageSize = pdfDocument.GetDefaultPageSize();
            var coordX = pageSize.GetX() + _document.GetLeftMargin();
            var coordY = pageSize.GetTop() - _document.GetTopMargin();
            var width = pageSize.GetWidth() - _document.GetRightMargin() - _document.GetLeftMargin();
            Rectangle rect = new Rectangle(coordX, coordY, width, _height);

            var pageNumber = pdfDocument.GetPageNumber(page);
            var pageCount = pdfDocument.GetNumberOfPages();
            var title = CreateTitle(pageCount, pageNumber);

            new Canvas(canvas, rect)
                .Add(title)
                .Add(_deviderTop)
                .Add(_auditHeaders)
                .Add(_deviderBottom)
                .Close();

            canvas.Release();
        }

        private Table CreateTitle(int pageCount, int pageNumber)
        {
            var columSizes = UnitValue.CreatePercentArray(new float[] { 20, 60, 20 });
            var table = new Table(columSizes).UseAllAvailableWidth();

            var logoWidth = UnitValue.CreatePercentValue(100);
            var logo = new Image(_logoData).SetWidth(logoWidth);
            var logoCell = CreateCell().Add(logo);
            table.AddCell(logoCell);

            var titleCell = PdfHelper.CreateTextCell(_headerTitle, 18, 38, 50, 55, (float)((_pageSize.GetWidth() - 40) * 0.6))
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.TOP);
            table.AddCell(titleCell);

            var pageMarkerCell = PdfHelper.CreateTextCell($"Page {pageNumber}", 12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.TOP);
            table.AddCell(pageMarkerCell);

            return table;
        }

        private Table CreateDevider()
        {
            var borderColor = new DeviceRgb(100, 100, 100);
            var deviderBorder = new SolidBorder(borderColor, 1);
            return new Table(1).UseAllAvailableWidth().SetBorderBottom(deviderBorder);
        }

        protected virtual Table CreateAudit(AuditDto audit)
        {
            float availableWidth = _pageSize.GetWidth() - 40;
            float[] columnsPercentWidth = new float[] { 15, 20, 23, 13, 25 };

            float orgWidth = availableWidth * columnsPercentWidth[0] / 100;
            float facilityWidth = availableWidth * columnsPercentWidth[1] / 100;
            float datesWidth = availableWidth * columnsPercentWidth[2] / 100;
            float auditorWidth = availableWidth * columnsPercentWidth[3] / 100;
            float auditDateWidth = availableWidth * columnsPercentWidth[4] / 100;

            var percentArray = UnitValue.CreatePercentArray(columnsPercentWidth);
            var table = new Table(percentArray)
                .SetMarginTop(5)
                .SetMarginBottom(5);

            var organizationHeader = CreateAuditHeaderCell("Organization")
                .SetWidth(orgWidth)
                .SetPaddingBottom(0);
            table.AddCell(organizationHeader);

            var facilityHeader = CreateAuditHeaderCell("Facility")
                .SetWidth(facilityWidth)
                .SetPaddingBottom(0);
            table.AddCell(facilityHeader);

            var datesHeader = CreateAuditHeaderCell("Filtered Date")
                .SetWidth(datesWidth)
                .SetPaddingBottom(0);
            table.AddCell(datesHeader);

            var auditorHeader = CreateAuditHeaderCell("Audited by")
                .SetWidth(auditorWidth)
                .SetPaddingBottom(0);
            table.AddCell(auditorHeader);

            var auditDateHeader = CreateAuditHeaderCell("Audit Date")
                .SetWidth(auditDateWidth)
                .SetPaddingBottom(0);
            table.AddCell(auditDateHeader);

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

            var auditor = CreateAuditDataCell(audit.SubmittedByUser.FullName)
                .SetWidth(auditorWidth)
                .SetPaddingTop(0);
            table.AddCell(auditor);

            var auditDate = CreateAuditDataCell($"{audit.SubmittedDate.AddHours(UtcOffset).ToString(DateTimeConstants.MM_DD_YYYY_hh_mm_tt_SLASH)} ({TimeZoneShortName})")
                .SetWidth(auditDateWidth)
               .SetPaddingTop(0);
            table.AddCell(auditDate);

            return table;

            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text.ToUpper());

            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
                .SetFontSize(11);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                PdfHelper.CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private static Cell CreateCell() => new Cell(1, 1).SetBorder(Border.NO_BORDER);
    }
}
