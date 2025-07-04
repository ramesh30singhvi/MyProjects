using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class EditQuestionGroupsModel
    { 
        public IReadOnlyCollection<EditQuestionGroupModel> Sections { get; set; }
    }

    public class EditQuestionGroupModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Sequence { get; set; }
    }
}
