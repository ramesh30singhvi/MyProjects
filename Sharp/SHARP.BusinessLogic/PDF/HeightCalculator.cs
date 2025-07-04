using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Renderer;

namespace SHARP.BusinessLogic.PDF
{
    internal static class HeightCalculator
    {
        internal static float GetHeight<T>(AbstractElement<T> element, Document document)
            where T : IElement
        {
            var layoutResult = GetLayoutResult(element, document);
            return layoutResult.GetOccupiedArea().GetBBox().GetHeight();
        }

        internal static float GetWidth<T>(AbstractElement<T> element, Document document)
            where T : IElement
        {
            var layoutResult = GetLayoutResult(element, document);
            return layoutResult.GetOccupiedArea().GetBBox().GetWidth();
        }

        private static LayoutResult GetLayoutResult<T>(AbstractElement<T> element, Document document)
            where T : IElement
        {
            var documentRenderer = new DocumentRenderer(document);
            var cellRenderer = element.CreateRendererSubTree();
            cellRenderer.SetParent(documentRenderer);

            Rectangle pageSize = document.GetPdfDocument().GetDefaultPageSize();

            var layoutArea = new LayoutArea(0, pageSize);
            var layoutContext = new LayoutContext(layoutArea);
            return cellRenderer.Layout(layoutContext);
        }
    }
}
