namespace TextExcel3.Cells.Data;

/// <summary>
/// Cell that holds an integer value
/// </summary>
public class IntegerCell(int value) : ICell, IRealCell
{
    public int RawValue { get; set; } = value;
    public decimal DecimalValue => RawValue;
    public string FormattedRealValue => RawValue.ToString();
    public string FormattedDisplayValue(int width)
    {
        int digits = (int)Math.Floor(Math.Log10(RawValue));
        // if the value is too large, use … to cut off digits
        if (digits > width) return RawValue.ToString()[..(width - 1)] + "\u2026";
        return new string(' ', width - digits) + RawValue;
    }
    
    public override string ToString()
    {
        return FormattedRealValue;
    }

}