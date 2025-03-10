namespace TextExcel3.Cells;

/// <summary>
/// Represents a cell with no content
/// </summary>
public class EmptyCell : ICell
{
    public string FormattedRealValue { get; } = "<empty>";
    public string FormattedDisplayValue(int width)
    {
        string r = "";
        for (int i = 0; i < width; i++) r += " ";
        return r;
    }
}