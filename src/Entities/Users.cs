namespace OrderRice.Entities
{
    public class Users : BaseEntity<Guid>
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string TelegramId { get; set; }
    }
}
