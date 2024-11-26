namespace OrderLunch.Interfaces;

public interface IPaymentService
{
    Task<string> GeneratePaymentLinkAsync(decimal amount, string additionalData);
}
