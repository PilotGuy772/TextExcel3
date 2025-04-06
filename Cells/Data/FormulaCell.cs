using TextExcel3.Formulas;

namespace TextExcel3.Cells.Data;

public class FormulaCell(string formula, Spreadsheet sheet) : ICell, IRealCell
{
    public string Formula { get; } = formula;
    public IFormulaTerm FormulaTerm { get; } = FormulaBuilder.BuildFormula(formula[1..], sheet);
    public string FormattedRealValue { get; } = formula;
    public string FormattedDisplayValue(int width)
    {
        try
        {
            string value = "" + FormulaTerm.DecimalValue;
            if (value.Length > width) return value[..width];
            return new string(' ', width - value.Length) + value;
        }
        catch (FormulaException e)
        {
            string value = e.ShortMessage;
            if (value.Length > width) return value[..width];
            return value + new string(' ', width - value.Length);
        }
    }

    public decimal DecimalValue
    {
        get
        {
            try
            {
                return FormulaTerm.DecimalValue;
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

    public override string ToString()
    {
        return Formula;
    }
}