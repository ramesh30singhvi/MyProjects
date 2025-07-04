namespace SHARP.DAL.Models
{
    public class OrganizationMemo
    {
        public int OrganizationId { get; set; }

        public int MemoId { get; set; }

        public Organization Organization { get; set; }

        public Memo Memo { get; set; }
    }
}
