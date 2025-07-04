using System.Collections.Generic;

namespace SHARP.ViewModels.Common
{
    public class RearrangeItemsModel
    { 
        public IReadOnlyCollection<RearrangeModel> Items { get; set; }
    }

    public class RearrangeModel
    {
        public int Id { get; set; }

        public int Sequence { get; set; }
    }
}
