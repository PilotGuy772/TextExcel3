namespace TextExcel3.Cells;

public class SingleCellClipboard(ICell self) : IClipboardItem
{
    private ICell Self { get; } = self;
    
    public void PasteItem(Spreadsheet sheet, SpreadsheetLocation pos)
    {
        sheet.SetCell(pos, Self);
    }
}