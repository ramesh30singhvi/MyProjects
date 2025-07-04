namespace SHARP.DAL.Models
{
    public interface IEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
