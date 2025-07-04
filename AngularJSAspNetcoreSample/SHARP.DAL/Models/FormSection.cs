using System;
using System.Collections.Generic;
using System.Linq;
using SHARP.Common.Constants;

namespace SHARP.DAL.Models
{
	public class FormSection : IIdModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public int Sequence { get; set; }
        public int FormVersionId { get; set; }
        public FormVersion FormVersion { get; set; }

        public ICollection<FormGroup> Groups { get; set; }

        public FormSection()
        {
            Groups = new List<FormGroup>();
        }

        public FormSection Clone(FormVersion draftForm)
        {
            var section = new FormSection();
            section.Name = Name;
            section.FormVersion = draftForm;
            section.Sequence = Sequence;

            if (Groups != null && Groups.Any())
            {
                Groups.OrderBy(group => group.Id).ToList().ForEach(group =>
                {
                    var newGroup = group.Clone(draftForm, section);
                    section.Groups.Add(newGroup);
                });
            }

            return section;
        }
    }
}

