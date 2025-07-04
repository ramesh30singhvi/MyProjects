using System.Collections.Concurrent;

namespace SHARP.Common.Models
{
    public class AppConfig
    {
        public ConcurrentDictionary<string, object> Application { get; } = new ConcurrentDictionary<string, object>();
    }
}
