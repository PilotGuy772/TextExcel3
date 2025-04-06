namespace TextExcel3.Formulas;

public class FormulaException : Exception
{
    public FormulaException(string message, string shortMessage, Exception b) : base(message, b)
    {
        ShortMessage = shortMessage;
    }
    
    public FormulaException(string message, string shortMessage) : base(message)
    {
        ShortMessage = shortMessage;
    }


    public string ShortMessage { get; set; }
}