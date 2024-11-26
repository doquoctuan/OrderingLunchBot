using OrderLunch.Interfaces;

namespace OrderLunch.Services;

public class PaymentService : IPaymentService
{
    private readonly int _binCode;
    private readonly int _accountNumber;
    private readonly string _templateName;

    public PaymentService()
    {
        _binCode = int.Parse(Environment.GetEnvironmentVariable("BIN_CODE") ?? "970436");
        _accountNumber = int.Parse(Environment.GetEnvironmentVariable("ACCOUNT_NUMBER") ?? "1052390085");
        _templateName = Environment.GetEnvironmentVariable("TEMPLATE_NAME") ?? "compact";
    }

    public Task<string> GeneratePaymentLinkAsync(decimal amount, string additionalData)
    {
        return Task.FromResult($"https://img.vietqr.io/image/{_binCode}-{_accountNumber}-{_templateName}.png?amount={amount:F0}&addInfo={additionalData}");
    }
}
