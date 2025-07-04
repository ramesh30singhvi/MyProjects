namespace SHARP.Common.Filtration
{
    public class UserFilterColumnSource<T> : FilterColumnSource<T>
    {
        public UserFilter UserFilter { get; set; }
    }
}
