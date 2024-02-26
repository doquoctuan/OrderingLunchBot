namespace OrderRice.Entities
{
    public class Users : BaseEntity<Guid>
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string TelegramId { get; set; }
        public string Email { get; set; }
        public DateTime DayIn { get; set; }
        public DateTime Birthday { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public bool IsBlacklist { get; set; }
    }
}
