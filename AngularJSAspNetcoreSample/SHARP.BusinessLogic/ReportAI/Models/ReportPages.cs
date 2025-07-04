using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.ReportAI.Models
{
    public class ReportPages :IDisposable
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public IList<string> Pages { get; set; }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Pages?.Clear();        
            }


            Pages = null;
            _disposed = true;
        }
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }

 
}
