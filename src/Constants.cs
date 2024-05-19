namespace OrderRice
{
    public class Constants
    {
        public int EXPIRES_IN { get; set; } = 3500;
    }

    public class ErrorMessages
    {
        public static string CANNOT_FIND_SHEET = "Cannot find sheetId for the current month";
        public static string CANNOT_FIND_PREV_SHEET = "Cannot find sheetId for the prev month";
        public static string CANNOT_FIND_CENTRAL_SHEET = "Cannot find sheetId for the current month";
        public static string CANNOT_FIND_SHEET_TODAY = "Today, do not support registration for lunch";
        public static string USER_DOES_NOT_EXIST = "The user does not exists";
    }
}
