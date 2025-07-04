using AutoMapper;
using System;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringNameToEnumConverter<T> : ITypeConverter<string, T> where T : struct
    {
        public T Convert(string source, T destination, ResolutionContext context)
        {
            if (!Enum.TryParse(source, true, out T result))
            {
                return default;
            }

            return result;
        }
    }
}
