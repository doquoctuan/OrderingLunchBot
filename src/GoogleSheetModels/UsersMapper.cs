using OrderLunch.Entities;
using System.Globalization;

namespace OrderLunch.GoogleSheetModels
{
    public static class UsersMapper
    {
        public static List<User> MapFromRangeData(IList<IList<object>> values)
        {
            var items = new List<User>();

            int i = 1;
            foreach (var value in values)
            {
                i++;
                if (!value[1].ToString().Contains("viettel"))
                {
                    continue;
                }

                User item = new()
                {
                    RowNum = i,
                    UserName = value[0].ToString(),
                    Email = value[1].ToString(),
                    FullName = value[2].ToString(),
                    DayIn = DateTime.ParseExact(value[3].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Birthday = DateTime.ParseExact(value[4].ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    PhoneNumber = value[5].ToString().PadLeft(10, '0'),
                    IsBlacklist = !string.IsNullOrEmpty(value[6].ToString()),
                    Department = value[7].ToString(),
                    Floor = int.Parse(value[8].ToString()),
                    TelegramId = long.Parse(value[9].ToString()),
                };

                items.Add(item);
            }

            return items;
        }

        public static IList<IList<object>> MapToRangeData(User item)
        {
            var objectList = new List<object>() { item.UserName, item.Email, item.FullName, item.GetDayIn(), item.GetBirthday(), item.PhoneNumber, item.GetIsBlacklist(), item.Department, item.Floor, item.TelegramId };
            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }
    }
}
