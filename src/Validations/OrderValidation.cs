namespace OrderLunch.Validations;

internal class OrderValidation
{
    private OrderValidation() { }

    public static Func<string, bool> CreateValidator(DateTime createDate)
    {
        var baseValidation = CombineValidations(
            o => !string.IsNullOrEmpty(o),
            o => !(createDate.DayOfWeek.Equals(DayOfWeek.Saturday) || createDate.DayOfWeek.Equals(DayOfWeek.Sunday))
        );

        return CombineValidations(baseValidation);
    }

    private static Func<string, bool> CombineValidations(params Func<string, bool>[] validations) =>
        order => Array.TrueForAll(validations, v => v(order));
}
