namespace TextExcel3.Cells;

/// <summary>
/// Represents a cell with no content
/// </summary>
public class EmptyCell : ICell
{
    public string RawValue { get; set; } = "";
    public string FormattedRealValue => "<empty>";

    public string FormattedDisplayValue(int width)
    {
        string r = "";
        for (int i = 0; i < width; i++) r += " ";
        return r;
    }

    public override string ToString()
    {
        return "";
    }
}