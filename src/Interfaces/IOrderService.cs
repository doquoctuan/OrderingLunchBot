namespace OrderRice.Interfaces
{
    public interface IOrderService
    {
        Task<(List<(string, string)>, string, string)> CreateOrderListImage(string folderName);
        Task<Dictionary<string, string>> GetMenu(DateTime dateTime);
        Task<bool> Order(string userName, DateTime dateTime, bool isOrder = true, bool isAll = false);
        Task<Dictionary<int, (string, int)>> UnPaidList();
        Task<List<(string, string)>> CreateUnpaidImage();
        Task BlockOrderTicket();
        Task<bool> PaymentConfirmation(string userName);
    }
}
