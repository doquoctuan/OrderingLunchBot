namespace OrderLunch.Entities
{
    public abstract class BaseEntity<TId>
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}
