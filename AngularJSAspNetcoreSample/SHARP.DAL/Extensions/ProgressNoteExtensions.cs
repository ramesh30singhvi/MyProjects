using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using System;

namespace SHARP.DAL.Extensions
{
    public static class ProgressNoteExtensions
    {
        public static void BuildProgressNoteEntity(this ModelBuilder builder)
        {
            builder.Entity<ProgressNote>(entity =>
            {
                entity.HasKey(progressNote => progressNote.Id);

                entity.Property(progressNote => progressNote.Id)
                    .HasColumnName("ID");

                entity.Property(progressNote => progressNote.PatientId)
                    .HasColumnName("PatientID")
                    .IsRequired();

                entity.Property(progressNote => progressNote.CreatedBy)
                    .IsRequired();

                entity.Property(progressNote => progressNote.CreatedDate)
                    .HasConversion(
                        to => to,
                        from => DateTime.SpecifyKind(from, DateTimeKind.Utc))
                    .IsRequired();

                entity.Property(progressNote => progressNote.EffectiveDate)
                    .HasConversion(
                       to => to,
                       from => DateTime.SpecifyKind(from, DateTimeKind.Utc));

                entity.Property(progressNote => progressNote.ProgressNoteText)
                   .IsRequired();

                entity.HasOne(progressNote => progressNote.Patient)
                    .WithMany(patient => patient.ProgressNotes)
                    .HasForeignKey(progressNote => progressNote.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
