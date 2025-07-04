using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Memo
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ValidityDate { get; set; }

        public User User { get; set; }

        public ICollection<OrganizationMemo> OrganizationMemos { get; set; }
    }
}
