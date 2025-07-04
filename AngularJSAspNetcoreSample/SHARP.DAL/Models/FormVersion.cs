using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class FormVersion
    {
        public int Id { get; set; }

        public int FormId { get; set; }

        public int Version { get; set; }

        public FormVersionStatus Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? CreatedByUserId { get; set; }

        public Form Form { get; set; }

        public User CreatedBy { get; set; }

        public DateTime? ActivationDate { get; set; }

        public ICollection<TableColumn> Columns { get; set; }

        public ICollection<FormSection> Sections { get; set; }

        public ICollection<Audit> Audits { get; set; }

        public ICollection<FormField> FormFields { get; set; }

        public ICollection<TableColumnGroup> Groups { get; set; }

        public FormVersion() { }

        public FormVersion(
            int version,
            FormVersionStatus status,
            DateTime? createdDate,
            User createdBy,
            Form form)
        {
            FormId= form.Id;
            Version = version;
            Status = status;
            CreatedDate = createdDate;
            CreatedByUserId = createdBy?.Id;
            CreatedBy = createdBy;
            Form = form;

            Columns = new List<TableColumn>();
            Sections = new List<FormSection>();
            Audits = new List<Audit>();
            Groups = new List<TableColumnGroup>();
            FormFields = new List<FormField>();
        }

        public FormVersion Clone()
        {
            return new FormVersion(Version + 1, FormVersionStatus.Draft, DateTime.UtcNow, CreatedBy, Form);
        }
    }
}
