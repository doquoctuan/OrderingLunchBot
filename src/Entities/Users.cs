﻿namespace OrderRice.Entities
{
    public class Users : BaseEntity<Guid>
    {
        public int RowNum { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public long? TelegramId { get; set; }
        public string Email { get; set; }
        public DateTime DayIn { get; set; }
        public DateTime Birthday { get; set; }
        public string PhoneNumber { get; set; }

        public string Department { get; set; }
        public bool? IsBlacklist { get; set; }
        public int Floor { get; set; }
        public string GetDayIn()
        {
            return this.DayIn.ToString("dd/MM/yyyy");
        }

        public string GetBirthday()
        {
            return this.Birthday.ToString("dd/MM/yyyy");
        }
        public string GetIsBlacklist()
        {
            return this.IsBlacklist == true ? "x" : string.Empty;
        }
    }
}
