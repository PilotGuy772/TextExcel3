using TextExcel3.Cells.Data;

namespace TextExcel3.Cells;

/// <summary>
/// Class for managing cell data and cell type assignments
/// </summary>
public class DataManager(Spreadsheet sheet)
{
    public Spreadsheet Sheet { get; set; } = sheet;

    public void AssignCell(SpreadsheetLocation cell, string input)
    {
        // cells will default to the most specific data type that their data can be cast into, with TextCell being the least specific
        // double quotes around the data will force it to be a string
        
        // TESTING just assign it to a textcell no matter what
        Sheet.Cells[cell.Row, cell.Column] = new TextCell(input.Trim());
    }

    public void ClearCell(SpreadsheetLocation cell) => Sheet.Cells[cell.Row, cell.Column] = new EmptyCell();
}