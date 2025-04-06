namespace TextExcel3.Formulas.Components;

/// <summary>
/// Stores all of the (static) methods for functions used in formulas. Can also be instantiated to create a formula term wrapping a function.
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
}