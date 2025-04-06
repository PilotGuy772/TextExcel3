namespace TextExcel3.Cells.Data;

public class PercentCell : ICell, IRealCell
{
    public decimal DecimalValue { get; }
    public string FormattedRealValue => (RawValue * 100) + "%";
    
    private decimal RawValue { get; set; }

    public PercentCell(decimal value)
    {
        RawValue = value / 100;
    }
    public string FormattedDisplayValue(int width)
    {
        string value = $"{(RawValue * 100):F2}%";
        if (value.Length > width) return value[..width];
        return new string(' ', width - value.Length) + value;   
    }

}