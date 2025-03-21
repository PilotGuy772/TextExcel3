namespace TextExcel3.Cells.Data.Util;

public class CellRange(SpreadsheetLocation start, SpreadsheetLocation end)
{
    public SpreadsheetLocation RangeStart { get; } = start;
    public SpreadsheetLocation RangeEnd { get; set; } = end;

    public IEnumerable<SpreadsheetLocation> AllCells
    {
        get
        {
            List<SpreadsheetLocation> locations = [];
            for (int r = RangeStart.Row; r <= RangeEnd.Row; r++)
                for (int c = RangeStart.Column; c <= RangeEnd.Column; c++)
                    locations.Add(new SpreadsheetLocation(c, r));
            return locations;
        }
    }
}