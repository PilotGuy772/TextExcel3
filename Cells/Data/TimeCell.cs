using System.Globalization;

namespace TextExcel3.Cells.Data;

public class TimeCell(TimeOnly value) : ICell
{
    public TimeOnly RawValue { get; set; } = value;
    public string FormattedRealValue => RawValue.ToString(CultureInfo.CurrentCulture);
    public string FormattedDisplayValue(int width)
    {
        // use C#'s builtin localization features for this
        // just right-align the output
        string value = RawValue.ToString(CultureInfo.CurrentCulture);
        if (value.Length > width) return value[..width];
        return new string(' ', width - value.Length) + value;
    }
    
    public override string ToString()
    {
        return FormattedRealValue;
    }
}