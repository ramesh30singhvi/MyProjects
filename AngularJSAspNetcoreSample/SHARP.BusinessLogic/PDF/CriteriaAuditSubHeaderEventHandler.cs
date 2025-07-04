using System;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;

namespace SHARP.BusinessLogic.PDF
{
	internal class CriteriaAuditSubHeaderEventHandler : IEventHandler
	{

        private readonly Document _document;
        private readonly Rectangle _pageSize;

        internal CriteriaAuditSubHeaderEventHandler(Document document)
		{
            _document = document;
            _pageSize = document.GetPdfDocument().GetDefaultPageSize();
		}

        public void HandleEvent(Event @event)
        {
            //Console.WriteLine("Handle Event");
        }
    }
}

