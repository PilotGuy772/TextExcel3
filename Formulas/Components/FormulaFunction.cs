namespace TextExcel3.Formulas.Components;

/// <summary>
/// Stores all of the (static) methods for functions used in formulas.
/// Can also be instantiated to create a formula term wrapping a function.
/// </summary>
public class FormulaFunction(Func<IFormulaTerm[], decimal> func, IFormulaTerm[] args) : IFormulaTerm
{
    public decimal DecimalValue
    {
        get
        {
            try
            {
                return func(args);
            }
            catch (Exception e)
            {
                throw e switch
                {
                    DivideByZeroException => new FormulaException("Division by zero", "#DIV0", e),
                    InvalidCastException => new FormulaException("Non-real value referenced", "#VALUE", e),
                    _ => new FormulaException("Evaluation error", "#EVAL", e)
                };
            }
        }
    }

    public static decimal Add(IFormulaTerm[] args)
    {
        return args.Sum(arg => arg.DecimalValue);
    }

    public static decimal Sum(IFormulaTerm[] args) => Add(args);
    
    public static decimal Subtract(IFormulaTerm[] args)
    {
        return args[0].DecimalValue - args[1].DecimalValue;
    }
    
    public static decimal Multiply(IFormulaTerm[] args)
    {
        return args.Aggregate<IFormulaTerm, decimal>(1, (current, arg) => current * arg.DecimalValue);
    }
    
    public static decimal Divide(IFormulaTerm[] args)
    {
        return args[0].DecimalValue / args[1].DecimalValue;
    }

    public static decimal Average(IFormulaTerm[] args) => Add(args) / args.Length;
    public static decimal Avg(IFormulaTerm[] args) => Average(args);

    public static decimal Power(IFormulaTerm[] args)
    {
        decimal baseValue = args[0].DecimalValue;
        decimal exponent = args[1].DecimalValue;
        
        if (exponent == 0)
            return 1;

        if (baseValue == 0)
        {
            if (exponent < 0)
                throw new ArgumentException("Zero cannot be raised to a negative power.");
            return 0;
        }

        // Convert to double for logarithmic operations
        double baseDouble = (double)baseValue;
        double exponentDouble = (double)exponent;

        // Handle negative base with non-integer exponent (complex number territory)
        if (baseValue < 0 && exponent != Math.Floor(exponent))
            throw new ArgumentException("Negative base with non-integer exponent results in a complex number.");

        double result = Math.Pow(baseDouble, exponentDouble);

        // Convert back to decimal with rounding
        return (decimal)result;
    }
}