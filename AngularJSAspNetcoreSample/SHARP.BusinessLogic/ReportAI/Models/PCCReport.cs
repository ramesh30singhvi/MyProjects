using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARP.BusinessLogic.ReportAI.Models
{
    public class PCCReport :IDisposable
    {
        public string Header { get; set; }
        public string CcombinedText { get; set; }

        public Dictionary<int, Dictionary<string, object>> Notes { get; set; }
        public string ID { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }


        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach(var note in Notes)
                {
                    note.Value?.Clear();
                }
                Notes?.Clear();
            }


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
