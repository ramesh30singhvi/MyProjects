using AutoMapper;
using System;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringToDateTimeConverter: ITypeConverter<string, DateTime?>
    {
        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(DateTime?);
            }

            if(!DateTime.TryParse(source, out DateTime result))
            {
                return default(DateTime?);
            }

            return result;
        }
    }
}
