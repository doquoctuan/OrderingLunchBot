namespace OrderRice.Interfaces
{
    public interface IOrderService
    {
        Task<(List<(string, string)>, string, string)> CreateOrderListImage();
        Task<Dictionary<string, string>> GetMenu(DateTime dateTime);
        Task<bool> Order(string userName, DateTime dateTime, bool isOrder = true);
        Task<Dictionary<int, string>> UnPaidList();
        Task SeedingDataFromFile();
    }
}
