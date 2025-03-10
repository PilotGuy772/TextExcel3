namespace TextExcel3.Cells.Data;

public class TextCell(string value) : ICell
{
    public string RawValue { get; set; } = value;
    public string FormattedRealValue => RawValue;
    public string FormattedDisplayValue(int width)
    {
        if (RawValue.Length > width) return RawValue[..width];
        return RawValue + new string(' ', width - RawValue.Length);
    }
    
    public override string ToString()
    {
        return RawValue;
    }
    
}