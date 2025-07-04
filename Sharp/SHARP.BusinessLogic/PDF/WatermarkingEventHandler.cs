using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SHARP.BusinessLogic.Properties;

namespace SHARP.BusinessLogic.PDF
{
    public class WatermarkingEventHandler : IEventHandler
    {
        ImageData _img;

        public WatermarkingEventHandler()
        {
            _img = ImageDataFactory.Create(Resources.Watermark_horizontal);
        }

        public void HandleEvent(Event currentEvent)
        {
            float w = _img.GetWidth();
            float h = _img.GetHeight();

            PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
            PdfPage page = docEvent.GetPage();

            Rectangle pageSize = page.GetPageSize();

            float x = (pageSize.GetLeft() + pageSize.GetRight()) / 2;

            /*PdfExtGState gs = new PdfExtGState()
                .SetFillOpacity(0.3f);*/

            PdfCanvas canvas = new PdfCanvas(page);

            canvas.SaveState();
            // canvas.SetExtGState(gs);
            canvas.AddImageWithTransformationMatrix(_img, w/2F, 0, 0, h/2F, x - w/4f, 50, true);
            canvas.RestoreState();

            Paragraph paragraph = new Paragraph("CONFIDENTIAL:  This document has been prepared at the request of and for review and evaluation by the Quality Assessment and Assurance Committee " +
                "and is entitled to the protection of the peer review, medical review, quality assurance, or other similar privileges provided for by state and federal law. It is not to be copied " +
                "or distributed without the expressed, written consent of the legal counsel")
                .SetFontSize(10)
                .SetMaxWidth(pageSize.GetWidth() - 40)
                .SetOpacity(0.7f);

            new Canvas(canvas, pageSize)
                .SetFontColor(ColorConstants.GRAY)
                .ShowTextAligned(paragraph, x, 40, 0,TextAlignment.CENTER, VerticalAlignment.MIDDLE, 0)
                .Close();

            canvas.Release();
        }
    }
}
