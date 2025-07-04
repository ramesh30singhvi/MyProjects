using AutoMapper;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Enums;
using SHARP.DAL.Models;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.Configuration.AutoMapper.Converters
{
    public class FormSettingFromJsonConverter : IValueConverter<FormOrganization, ScheduleSettingDto>
    {
        public ScheduleSettingDto Convert(FormOrganization formOrganization, ResolutionContext context)
        {
            if (formOrganization.SettingType == FormSettingType.Triggered || formOrganization.ScheduleSetting == null)
            {
                return null;
            }

            return new ScheduleSettingDto()
            {
                Id = formOrganization.ScheduleSetting.FormOrganizationId,
                ScheduleType = formOrganization.ScheduleSetting.ScheduleType,
                Days = JsonConvert.DeserializeObject<IReadOnlyCollection<int>>(formOrganization.ScheduleSetting.Days)
            };
        }
    }
}
