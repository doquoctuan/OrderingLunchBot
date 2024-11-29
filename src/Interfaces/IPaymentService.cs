namespace OrderLunch.Interfaces;

public interface IPaymentService
{
    string GeneratePaymentLinkAsync(decimal amount, string additionalData);
}
