using TextExcel3.Cells;

namespace TextExcel3.Formulas.Components;

public class CellReference(SpreadsheetLocation target, Spreadsheet sheet) : IFormulaTerm
{
    private Spreadsheet Sheet { get; } = sheet;
    private SpreadsheetLocation Target { get; } = target;
    public decimal DecimalValue
    {
        get
        {
            try
            {
                return ((IRealCell)Sheet.GetCell(Target)).DecimalValue;
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
}