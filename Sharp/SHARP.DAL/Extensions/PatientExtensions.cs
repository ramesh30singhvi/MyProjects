using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class PatientExtensions
    {
        public static void BuildPatientEntity(this ModelBuilder builder)
        {
            builder.Entity<Patient>(entity =>
            {
                entity.HasKey(patient => patient.Id);

                entity.Property(patient => patient.Id)
                    .HasColumnName("ID");

                entity.Property(patient => patient.FirstName)
                    .IsRequired();

                entity.Property(patient => patient.LastName)
                    .IsRequired();

                entity.Property(patient => patient.FacilityId)
                    .HasColumnName("FacilityID")
                    .IsRequired();
            });
        }
    }
}
