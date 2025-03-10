using System.Globalization;

namespace TextExcel3.Cells.Data;

/// <summary>
/// A cell with a 64-bit floating point number value
/// </summary>
public class FloatCell(double value) : ICell
{
    public double RawValue { get; set; } = value;
    public string FormattedRealValue => RawValue.ToString(CultureInfo.CurrentCulture);
    public string FormattedDisplayValue(int width)
    {
        string value = RawValue.ToString(CultureInfo.CurrentCulture);
        if (value.Length > width) return value[..width];
        return new string(' ', width - value.Length) + value;    
    }
    
    public override string ToString()
    {
        return FormattedRealValue;
    }
}