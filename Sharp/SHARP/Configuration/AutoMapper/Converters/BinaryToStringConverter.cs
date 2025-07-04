
using AutoMapper;
using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using SHARP.DAL.Models;
using SHARP.BusinessLogic.DTO.Report;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class BinaryToStringConverter : IValueConverter<AuditAIPatientPdfNotesDto, string>
    {
        private string DeCompress(byte[] bytes)
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


        public string Convert(AuditAIPatientPdfNotesDto sourceMember, ResolutionContext context)
        {
            if (sourceMember == null) return string.Empty;

            if (sourceMember.PdfNotes == null) return string.Empty;

            return DeCompress(sourceMember.PdfNotes);
        }
    }
}
