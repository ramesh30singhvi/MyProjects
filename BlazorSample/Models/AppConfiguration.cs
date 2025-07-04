using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Models
{
    public class AppConfiguration
    {
        public string ApiUrl { get; set; }
        public bool IsDev { get; set; }
        public string Environment { get; set; }
    }
    public class SynergyAppSettings
    {
        public string SynergyCDNBaseUrl { get; set; }
    }
}
