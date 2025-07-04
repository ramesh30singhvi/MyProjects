using AutoMapper;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.BusinessLogic.Helpers;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class JsonValueToStringConverter : IValueConverter<FieldValueDto, string>
    {
        public string Convert(FieldValueDto fieldValue, ResolutionContext context)
        {
            return ParserHelper.FormatAnswerValue(fieldValue);
        }
    }
}
