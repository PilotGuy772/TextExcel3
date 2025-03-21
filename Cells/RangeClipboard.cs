namespace TextExcel3.Cells;

/// <summary>
/// Represents a clipboard clip of an arbitrary (rectangular) range of cells
/// </summary>
public class RangeClipboard : IClipboardItem
{
    public ICell[,] Contents { get; set; }
    
    public void PasteItem(Spreadsheet sheet, SpreadsheetLocation pos)
    {
        // position is top left of the given array
        for (int r = pos.Row; r < pos.Row + Contents.GetLength(0); r++)
        {
            for (int c = pos.Column; c < pos.Column + Contents.GetLength(1); c++)
            {
                Console.Beep();
                sheet.SetCell(new SpreadsheetLocation(c, r), Contents[r - pos.Row,c - pos.Column]);
            }
        }
    }
}