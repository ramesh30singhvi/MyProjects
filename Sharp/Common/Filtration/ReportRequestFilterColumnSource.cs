namespace SHARP.Common.Filtration
{
    public class ReportRequestFilterColumnSource<T> : FilterColumnSource<T>
    {
        public ReportRequestFilter ReportRequestFilter { get; set; }

        public int? UserId { get; set; }
    }
}
