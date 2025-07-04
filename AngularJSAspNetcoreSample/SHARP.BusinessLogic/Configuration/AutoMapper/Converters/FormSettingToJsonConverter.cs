using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Enums;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class FormSettingToJsonConverter : IValueConverter<FormSettingDto, FormScheduleSetting>
    {
        public FormScheduleSetting Convert(FormSettingDto formSetting, ResolutionContext context)
        {
            if(formSetting.SettingType == FormSettingType.Triggered || formSetting.ScheduleSetting == null)
            {
                return null;
            }

            return new FormScheduleSetting()
            {
                FormOrganizationId = formSetting.Id,
                ScheduleType = formSetting.ScheduleSetting.ScheduleType,
                Days = JsonConvert.SerializeObject(formSetting.ScheduleSetting.Days)
            };
        }
    }
}
