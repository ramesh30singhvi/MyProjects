using SHARP.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace SHARP.DAL.Models
{
    public class FormField : IIdModel<int>
    {
        public int Id { get; set; }

        public int FieldTypeId { get; set; }

        public int? FormGroupId { get; set; }

        public int Sequence { get; set; }

        public string FieldName { get; set; }

        public string LabelName { get; set; }

        public int FormVersionId { get; set; }

        public bool IsRequired { get; set; }

        public FieldType FieldType { get; set; }

        public FormGroup? FormGroup { get; set; }

        public FormVersion FormVersion { get; set; }

        public ICollection<FormFieldItem> Items { get; set; }

        public ICollection<AuditFieldValue> Values { get; set; }

        public FormField() { }

        public FormField(int fieldTypeId, int sequence, string fieldName, string labelName, bool isRequired, FormVersion formVersion)
        {
            FieldTypeId = fieldTypeId;
            Sequence = sequence;
            FieldName = fieldName;
            LabelName = labelName;
            IsRequired = isRequired;
            FormVersion = formVersion;

            Items = new List<FormFieldItem>();
            Values = new List<AuditFieldValue>();
        }

        public FormField Clone(FormVersion draftForm)
        {
            var formField = new FormField(FieldTypeId, Sequence, FieldName, LabelName, IsRequired, draftForm);

            if (Items != null && Items.Any())
            {
                Items.ToList().ForEach(item => {
                    var fieldItem = item.Clone(formField);
                    formField.Items.Add(fieldItem);
                });
            }

            return formField;
        }
        
        public FormField Clone(FormGroup draftFormGroup, FormVersion draftForm)
        {
            var formField = new FormField(FieldTypeId, Sequence, FieldName, LabelName, IsRequired, draftForm);
            formField.FormGroup = draftFormGroup;

            if (Items != null && Items.Any())
            {
                Items.OrderBy(field => field.Id).ToList().ForEach(item => {
                    var fieldItem = item.Clone(formField);
                    formField.Items.Add(fieldItem);
                });
            }

            return formField;
        }
    }
}
