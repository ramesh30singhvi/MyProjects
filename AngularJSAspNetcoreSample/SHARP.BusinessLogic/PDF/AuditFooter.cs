using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace SHARP.BusinessLogic.PDF
{
    internal class AuditFooterEventHandler : IEventHandler
    {
        private readonly PdfFont _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        public void HandleEvent(Event @event)
        {
            var documentEvent = (PdfDocumentEvent)@event;
            var pdfDocument = documentEvent.GetDocument();
            var page = documentEvent.GetPage();
            var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDocument);
            Rectangle pageSize = page.GetPageSize();
            var coordX = pageSize.GetX() + 20;

            var pageNumber = pdfDocument.GetPageNumber(page);
            var pageCount = pdfDocument.GetNumberOfPages();

            string footerText = pageNumber == pageCount? "End of report" : "Continues on next page";

            Paragraph footerParagraph = new Paragraph(footerText)
                .SetFont(_font)
                .SetHeight(30)
                .SetFontSize(10)
                .SetFontColor(new DeviceRgb(144, 155, 168));

            new Canvas(canvas, pageSize)
                .ShowTextAligned(footerParagraph, coordX, 10, TextAlignment.LEFT, VerticalAlignment.MIDDLE)
                .Close();

            canvas.Release();
        }       
    }
}
