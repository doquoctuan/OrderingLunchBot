namespace OrderRice
{
    public class Constants
    {
        public (string, double) AccessToken { get; set; }
        public int EXPIRES_IN { get; set; } = 3599;
    }

    public class ErrorMessages
    {
        public static string CANNOT_FIND_SHEET = "Cannot find sheetId for the current month";
        public static string CANNOT_FIND_PREV_SHEET = "Cannot find sheetId for the prev month";
        public static string CANNOT_FIND_SHEET_TODAY = "Today, do not support registration for lunch";
    }

    public class CustomEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            // Implement your custom equality logic here
            return x.Contains(y, StringComparison.OrdinalIgnoreCase) || y.Contains(x, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            // Implement your custom hash code generation logic here
            return obj.GetHashCode() ^ obj.GetHashCode();
        }
    }

    public class BlackLists
    {
        public static HashSet<string> Set = new(new CustomEqualityComparer())
        {
            "Nguyễn Minh Thông",
            "Nguyễn Văn Lạc",
            "Huỳnh Lê Bảo",
            "TTS"
        };
    }
}
