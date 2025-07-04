using AutoMapper;
using Newtonsoft.Json;
using System.Text;

namespace SHARP.Configuration.AutoMapper.Converters
{
    public class StringToModelConverter<T> : ITypeConverter<string, T>
    {
        public T Convert(string source, T destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(source);
        }
    }
}
