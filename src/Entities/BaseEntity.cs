namespace OrderRice.Entities
{
    public abstract class BaseEntity<TId>
    {
        public TId Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}
