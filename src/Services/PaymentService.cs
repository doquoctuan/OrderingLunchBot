using OrderLunch.Interfaces;

namespace OrderLunch.Services;

public class PaymentService : IPaymentService
{
    private const string ACCOUNT_NAME = "DO QUOC TUAN";
    private readonly int _binCode;
    private readonly int _accountNumber;
    private readonly string _templateName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentService"/> class.
    /// Sets the BIN code, account number, and template name from environment variables.
    /// For getting BIN code, access to <see href="https://api.vietqr.io/v2/banks"/>.
    /// If environment variables are not set, default values are used.
    /// </summary>
    public PaymentService()
    {
        _binCode = int.Parse(Environment.GetEnvironmentVariable("BIN_CODE") ?? "970436");
        _accountNumber = int.Parse(Environment.GetEnvironmentVariable("ACCOUNT_NUMBER") ?? "1052390085");
        _templateName = Environment.GetEnvironmentVariable("TEMPLATE_NAME") ?? "V9yTbbv";
    }

    /// <summary>
    /// Generates a payment link with the specified amount and additional data.
    /// </summary>
    /// <param name="amount">The amount for the payment link.</param>
    /// <param name="additionalData">Additional data to be included in the payment link.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated payment link as a string.</returns>
    public string GeneratePaymentLinkAsync(decimal amount, string additionalData)
    {
        return Environment.GetEnvironmentVariable("BASE_IMAGE_PAYMENTINFO");
        // return $"https://api.vietqr.io/image/{_binCode}-{_accountNumber}-{_templateName}.jpg?amount={amount:F0}&addInfo={additionalData}&accountName=${ACCOUNT_NAME}";
    }
}
