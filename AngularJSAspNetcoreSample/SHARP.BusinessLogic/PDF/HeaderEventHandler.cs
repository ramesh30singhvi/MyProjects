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
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.BusinessLogic.PDF
{
    internal class HeaderEventHandler : IEventHandler
    {
        protected const string DATE_FORMAT = "MMM dd, yyyy";
        private readonly PdfFont _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        private readonly Document _document;
        private readonly float _height;
        private readonly Table _deviderTop;
        private readonly Table _deviderBottom;
        private readonly Table _auditHeaders;
        private readonly Table _auditSubHeaders;

        private readonly Rectangle _pageSize;

        private readonly string _headerTitle;

        public float Height => _height;

        private ImageData _logoData;

        private bool showSubHeaders = true;
        private CriteriaAuditDetailsDto[] _audits;

        internal HeaderEventHandler(Document document, string headerTitle, ICollection<HeaderItem> headerItems, params CriteriaAuditDetailsDto[] audits)
        {
            _audits = audits;
            showSubHeaders = audits.Count() > 1;
            _document = document;
            _headerTitle = headerTitle;

            _logoData = ImageDataFactory.Create(Resources.Logo);

            _pageSize = document.GetPdfDocument().GetDefaultPageSize();

            var title = CreateTitle(1, 1);
            var titleHeight = HeightCalculator.GetHeight(title, _document);

            _deviderTop = CreateDevider()
                .SetMarginTop(10)
                .SetMarginLeft(-20)
                .SetMarginRight(-20);
            var deviderTopHeight = HeightCalculator.GetHeight(_deviderTop, _document);

            _auditHeaders = CreateHeader(headerItems);
            var auditHeadersHeight = HeightCalculator.GetHeight(_auditHeaders, _document);

            var auditSubHeadersHeight = 0f;
            if (showSubHeaders)
            {
                _auditSubHeaders = CreateSubHeader(null, 0);
                auditSubHeadersHeight = HeightCalculator.GetHeight(_auditSubHeaders, _document); 
            }

            _deviderBottom = CreateDevider()
                .SetMarginLeft(-20)
                .SetMarginRight(-20);
            var deviderBottomHeight = HeightCalculator.GetHeight(_deviderBottom, _document);

            _height = titleHeight + deviderTopHeight + auditHeadersHeight + deviderBottomHeight + auditSubHeadersHeight;
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
            

            var pageNumber = pdfDocument.GetPageNumber(page);
            var pageCount = pdfDocument.GetNumberOfPages();
            var title = CreateTitle(pageCount, pageNumber);


            if (showSubHeaders)
            {
                if (pageNumber == 1)
                {
                    var deviderTopHeight = HeightCalculator.GetHeight(_deviderTop, _document);
                    var auditSubHeadersHeight = HeightCalculator.GetHeight(_auditSubHeaders, _document) + deviderTopHeight;

                    Rectangle rect = new Rectangle(coordX, coordY, width, _height);
                    new Canvas(canvas, rect)
                    .Add(title)
                    .Add(_deviderTop)
                    .Add(_auditHeaders)
                    .Add(_deviderBottom)
                    .Close();
                }
                else
                {
                   // Console.WriteLine($"Number of audits {_audits.Length}");
                    //Console.WriteLine($"Property {_document.GetProperty<int>(Property.ID)}");
                    if (_document.HasProperty(Property.ID))
                    {
                        var index = _document.GetProperty<int>(Property.ID);
                        var subHeaderTable = CreateSubHeader(_audits[index], _document.GetProperty<int>(Property.ID));

                        Rectangle rect = new Rectangle(coordX, coordY, width, _height);
                        new Canvas(canvas, rect)
                        .Add(title)
                        .Add(_deviderTop)
                        .Add(_auditHeaders)
                        .Add(subHeaderTable)
                        .Close();
                    }
                    
                }
            }
            else
            {

                Rectangle rect = new Rectangle(coordX, coordY, width, _height);
                new Canvas(canvas, rect)
                .Add(title)
                .Add(_deviderTop)
                .Add(_auditHeaders)
                .Add(_deviderBottom)
                .Close();
            }

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

            var titleCell = PdfHelper.CreateTextCell(_headerTitle, 18,  38, 50, 55, (float)((_pageSize.GetWidth() - 40) * 0.6))
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

        protected Table CreateHeader(ICollection<HeaderItem> headerItems)
        {
            var percentArray = UnitValue.CreatePointArray(headerItems.Select(item => item.WidthPoints).ToArray());
            var table = new Table(percentArray)
                .SetMarginTop(5)
                .SetMarginBottom(5)
                .UseAllAvailableWidth();

            foreach (var headerItem in headerItems)
            {
                var label = CreateAuditHeaderCell(headerItem.Label)
                    .SetWidth(headerItem.WidthPoints)
                    .SetPaddingBottom(0);
                table.AddCell(label);
            }

            foreach (var headerItem in headerItems)
            {
                var value = CreateAuditDataCell(headerItem.Value)
                    .SetWidth(headerItem.WidthPoints)
                    .SetPaddingTop(0);
                table.AddCell(value);
            }

            return table;

            Cell CreateAuditHeaderCell(string text) => CreateAuditCell(text?.ToUpper());

            Cell CreateAuditDataCell(string text) => CreateAuditCell(text)
                .SetFontSize(12);

            Cell CreateAuditCell(string text, int red = 0, int green = 0, int blue = 0) =>
                PdfHelper.CreateTextCell(text, red: red, green: green, blue: blue)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        protected Table CreateSubHeader(AuditDto audit, int counter)
        {
            if (audit == null) return PdfHelper.CreateCriteriaAuditDetailedDummyHeader();
           return PdfHelper.CreateCriteriaAuditDetailedHeader(audit);
        }

        private static Cell CreateCell() => new Cell(1, 1).SetBorder(Border.NO_BORDER);
    }
}
