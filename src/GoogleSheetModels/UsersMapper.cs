using OrderRice.Entities;
using System.Globalization;

namespace OrderRice.GoogleSheetModels
{
    public static class UsersMapper
    {
        public static List<Users> MapFromRangeData(IList<IList<object>> values)
        {
            var items = new List<Users>();

            foreach (var value in values)
            {
                if (!value[1].ToString().Contains("viettel"))
                {
                    continue;
                }
                Users item = new()
                {
                    Id = Guid.NewGuid(),
                    UserName = value[0].ToString(),
                    Email = value[1].ToString(),
                    FullName = value[2].ToString(),
                    DayIn = DateTime.ParseExact(value[3].ToString(), "mm/dd/yyyy", CultureInfo.InvariantCulture),
                    Birthday = DateTime.ParseExact(value[4].ToString(), "mm/dd/yyyy", CultureInfo.InvariantCulture),
                    PhoneNumber = value[5].ToString().PadLeft(10, '0'),
                    IsBlacklist = !string.IsNullOrEmpty(value[6].ToString()),
                    Department = value[7].ToString(),
                };

                items.Add(item);
            }

            return items;
        }

        public static IList<IList<object>> MapToRangeData(Users item)
        {
            var objectList = new List<object>() { item.UserName, item.Email, item.FullName, item.DayIn, item.Birthday, item.PhoneNumber, item.IsBlacklist };
            var rangeData = new List<IList<object>> { objectList };
            return rangeData;
        }
    }
}
