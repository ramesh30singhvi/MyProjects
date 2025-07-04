using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using System.Linq;

namespace SHARP.BusinessLogic.Helpers
{
    public class ParserHelper
    {
        public static string FormatAnswerValue(FieldValueDto fieldValue)
        {
            fieldValue.Value = fieldValue.Value ?? string.Empty;

            switch (fieldValue.FieldType)
            {
                case FieldTypes.Checkbox:
                    bool checkboxVlaue = !string.IsNullOrEmpty(fieldValue.Value) ? (bool)JsonConvert.DeserializeObject(fieldValue.Value) : false;
                    return checkboxVlaue ? CommonConstants.YES : CommonConstants.NO;

                case FieldTypes.DatePicker:
                    return !string.IsNullOrEmpty(fieldValue.Value) ? (string)JsonConvert.DeserializeObject(fieldValue.Value) : string.Empty;

                case FieldTypes.ToggleSingleSelect:
                case FieldTypes.DropdownSingleSelect:
                    if(string.IsNullOrEmpty(fieldValue.Value))
                    {
                        return string.Empty;
                    }

                    var optionValue = JsonConvert.DeserializeObject<ControlOptionDto>(fieldValue.Value);
                    return optionValue.Value;

                case FieldTypes.ToggleMultiselect:
                case FieldTypes.DropdownMultiselect:
                    if (string.IsNullOrEmpty(fieldValue.Value))
                    {
                        return string.Empty;
                    }

                    var optionValues = JsonConvert.DeserializeObject<ControlOptionDto[]>(fieldValue.Value);
                    return string.Join(", ", optionValues.Select(optionValue => optionValue.Value));

                default:
                    return fieldValue.Value;
            }
        }
    }
}
