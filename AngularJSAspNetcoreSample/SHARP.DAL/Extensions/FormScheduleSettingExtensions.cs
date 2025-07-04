using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;

namespace SHARP.DAL.Extensions
{
    public static class FormScheduleSettingExtensions
    {
        public static void BuildFormScheduleSettingEntity(this ModelBuilder builder)
        {
            builder.Entity<FormScheduleSetting>(entity =>
            {
                entity.HasKey(formSetting => formSetting.FormOrganizationId);

                entity.Property(formSetting => formSetting.FormOrganizationId)
                    .IsRequired();

                entity.Property(formSetting => formSetting.ScheduleType)
                     .IsRequired();

                entity.Property(formSetting => formSetting.Days)
                    .IsRequired();

                entity.HasOne(formSetting => formSetting.FormOrganization)
                    .WithOne(form => form.ScheduleSetting)
                    .HasForeignKey<FormScheduleSetting>(formSetting => formSetting.FormOrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
