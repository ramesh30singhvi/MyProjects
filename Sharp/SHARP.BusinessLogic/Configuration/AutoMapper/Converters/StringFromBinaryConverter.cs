using AutoMapper;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class StringFromBinaryConverter : IValueConverter<AuditAIReport, string>
    {
        private string DeCompressJson(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public string Convert(AuditAIReport auditReport, ResolutionContext context)
        {
            if(auditReport.SummaryAI == null)
                return string.Empty;

            if (auditReport.SummaryAI.Length == 0)
                return string.Empty;

            return DeCompressJson(auditReport.SummaryAI);
            

        }
    }
}
