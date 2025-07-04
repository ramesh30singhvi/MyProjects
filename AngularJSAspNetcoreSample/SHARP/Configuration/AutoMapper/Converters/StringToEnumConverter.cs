using AutoMapper;
using System;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringToEnumConverter<T> : ITypeConverter<string, T>
    {
        public T Convert(string source, T destination, ResolutionContext context)
        {
            if (!int.TryParse(source, out int status))
            {
                return default;
            }

            return (T)Enum.Parse(typeof(T), source, true);
        }
    }
}
