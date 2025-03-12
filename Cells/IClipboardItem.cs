namespace TextExcel3.Cells;

public interface IClipboardItem
{
    public void PasteItem(Spreadsheet sheet, SpreadsheetLocation pos);
}