using AutoMapper;
using System;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringToTimeSpanConverter : ITypeConverter<string, TimeSpan?>
    {
        public TimeSpan? Convert(string source, TimeSpan? destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default(TimeSpan?);
            }

            if(!TimeSpan.TryParse(source, out TimeSpan result))
            {
                return default(TimeSpan?);
            }

            return result;
        }
    }
}
