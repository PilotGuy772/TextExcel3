namespace TextExcel3.Cells;

/// <summary>
/// Represents a basic cell in the spreadsheet
/// </summary>
public interface ICell
{
    /// <summary>
    /// The string representation of this cell's actual content.
    /// </summary>
    public string FormattedRealValue { get; }

    /// <summary>
    /// Format this cell's content into a string for display in a cell with the given width.
    /// </summary>
    /// <param name="width">The width of the given cell.</param>
    /// <returns>The string representation of the cell's actual content, with a length exactly equal to `width`</returns>
    public string FormattedDisplayValue(int width);
}