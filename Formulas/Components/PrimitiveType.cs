namespace TextExcel3.Formulas.Components;

public class PrimitiveType<T>(T value) : IFormulaTerm
{
    public decimal DecimalValue => Convert.ToDecimal(Value);
    private T Value { get; } = value;
    
    /*
     * For reference-- all primitive types supported by the formula engine include:
     * - decimal (contains all other number types including int, double, etc.)
     * - boolean
     * - DateTime (goes in as unix time)
     * - TimeSpan (goes in as total milliseconds)
     * - string (to be implemented later)
     */
}