using System;
using System.Collections.Generic;
using System.Linq;
using SHARP.Common.Constants;

namespace SHARP.DAL.Models
{
    public class FormGroup : IIdModel<int>
    {
        public int Id { get; set; }
        
        public int Sequence { get; set; }
        public string Name { get; set; }
        public int FormSectionId { get; set; }
        public FormSection FormSection { get; set; }
        public ICollection<FormField> FormFields { get; set; }

        public FormGroup()
        {
            FormFields = new List<FormField>();
        }

        public FormGroup Clone(FormVersion draftForm, FormSection draftSection)
        {
            var group = new FormGroup();
            group.FormSection = draftSection;
            group.Name = Name;
            group.Sequence = Sequence;

            if (FormFields != null && FormFields.Any())
            {
                FormFields.OrderBy(formFields => formFields.Id).ToList().ForEach(formField =>
                {
                    var newFormField = formField.Clone(group, draftForm);
                    group.FormFields.Add(newFormField);
                });
            }
            
            return group;
        }
    }
}

