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
using SHARP.BusinessLogic.Properties;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.PDF
{
    internal class ManualKeywordReportAIEventHandler : IEventHandler
    {
        protected const string DATE_FORMAT = "MMM dd, yyyy";
        private readonly PdfFont _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        private readonly Document _document;
        private readonly float _height;
        private readonly Table _deviderTop;
        private readonly Table _deviderBottom;
        private readonly Table _reportHeaders;
        protected readonly Rectangle _pageSize;

        private readonly string _organization;
        private readonly string _facility;
        private readonly string _date;
        private readonly string _time;
        private readonly string _user;
        private readonly string _filteredDate;

        ImageData _logoData;

        public float Height => _height;
        private readonly string _headerTitle;

        internal ManualKeywordReportAIEventHandler(Document document, string organization, string facility,
            string date , string time , string user,string filteredDate)
        {
            _document = document;
            _pageSize = document.GetPdfDocument().GetDefaultPageSize();
            _logoData = ImageDataFactory.Create(Resources.Logo);
            _organization = organization;
            _facility = facility;
            _date = date;
            _time = time;
            _user = user;
            _filteredDate = filteredDate;

            var title = CreateTitle(1, 1);
            var titleHeight = HeightCalculator.GetHeight(title, _document);

            _deviderTop = CreateDevider()
                .SetMarginTop(10)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);
            var deviderTopHeight = HeightCalculator.GetHeight(_deviderTop, _document);

            _reportHeaders = CreateReportHeaders();
            var reportHeadersHeight = HeightCalculator.GetHeight(_reportHeaders, _document);

            _deviderBottom = CreateDevider()
                .SetMarginLeft(-20)
                .SetMarginRight(-20)
                .SetMarginBottom(0);
            var deviderBottomHeight = HeightCalculator.GetHeight(_deviderBottom, _document);

            _height = titleHeight + deviderTopHeight + reportHeadersHeight + deviderBottomHeight;
        }



        protected virtual Table CreateReportHeaders()
        {
            float availableWidth = _pageSize.GetWidth() - 40;
            float[] columnsPercentWidth = new float[] { 25, 20, 20, 20,25 };

            float orgWidth = availableWidth * columnsPercentWidth[0] / 100;
            float facilityWidth = availableWidth * columnsPercentWidth[1] / 100;
            float datesWidth = availableWidth * columnsPercentWidth[2] / 100;
            float userWidth = availableWidth * columnsPercentWidth[3] / 100;
            float dateWidth = availableWidth * columnsPercentWidth[4] / 100;

            var percentArray = UnitValue.CreatePercentArray(columnsPercentWidth);
            var table = new Table(percentArray)
                .SetMarginTop(5)
                .SetMarginBottom(5);

            var organizationHeader = CreateHeaderCell("Organization")
                .SetWidth(orgWidth)
                .SetPaddingBottom(0);
            table.AddCell(organizationHeader);

            var facilityHeader = CreateHeaderCell("Facility")
                .SetWidth(facilityWidth)
                .SetPaddingBottom(0);
            table.AddCell(facilityHeader);

            var datesHeader = CreateHeaderCell("FILTERED DATE")
                .SetWidth(datesWidth)
                .SetPaddingBottom(0);
            table.AddCell(datesHeader);

            var userHeader = CreateHeaderCell("AUDITED BY")
               .SetWidth(datesWidth)
               .SetPaddingBottom(0);
            table.AddCell(userHeader);

            var dateHeader = CreateHeaderCell("AUDIT DATE")
               .SetWidth(datesWidth)
               .SetPaddingBottom(0);
            table.AddCell(dateHeader);

            var organization = CreateDataCell(_organization)
                .SetWidth(orgWidth)
                .SetPaddingTop(0);
            table.AddCell(organization);

            var facility = CreateDataCell(_facility)
                .SetWidth(facilityWidth)
                .SetPaddingTop(0);
            table.AddCell(facility);

            var filteredDate = CreateDataCell(_filteredDate)
                .SetWidth(datesWidth)
                .SetPaddingTop(0);
            table.AddCell(filteredDate);

            var user = CreateDataCell(_user)
                .SetWidth(userWidth)
                .SetPaddingTop(0);
            table.AddCell(user);

            var auditTime = CreateDataCell($"{_date} {_time}")
                .SetWidth(dateWidth)
                .SetPaddingTop(0);
            table.AddCell(auditTime);

            return table;

            Cell CreateHeaderCell(string text) => CreateCell(text.ToUpper());

            Cell CreateDataCell(string text) => CreateCell(text)
                .SetFontSize(11);

            Cell CreateCell(string text, int red = 0, int green = 0, int blue = 0) =>
                PdfHelper.CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        }
        private Table CreateDevider()
        {
            var borderColor = new DeviceRgb(100, 100, 100);
            var deviderBorder = new SolidBorder(borderColor, 1);
            return new Table(1).UseAllAvailableWidth().SetBorderBottom(deviderBorder);
        }

        private Table CreateTitle(int pageCount, int pageNumber)
        {
            var columSizes = UnitValue.CreatePercentArray(new float[] { 20, 60, 20 });
            var table = new Table(columSizes).UseAllAvailableWidth();

            var logoWidth = UnitValue.CreatePercentValue(100);
            var logo = new Image(_logoData).SetWidth(logoWidth);
            var logoCell = CreateCell().Add(logo);
            table.AddCell(logoCell);

            var titleCell = PdfHelper.CreateTextCell("24 Hr Logged Keyword Report", 18, 38, 50, 55, (float)((_pageSize.GetWidth() - 40) * 0.6))
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

        private static Cell CreateCell() => new Cell(1, 1).SetBorder(Border.NO_BORDER);
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
                .Add(_reportHeaders)
                .Add(_deviderBottom)
                .Close();

            canvas.Release();
        }
    }
}
