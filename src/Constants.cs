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
}
