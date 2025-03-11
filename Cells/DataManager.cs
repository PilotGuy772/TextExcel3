using System.Globalization;
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
        //Sheet.Cells[cell.Row, cell.Column] = new TextCell(input.Trim());
        ICell newCell;
        
        if (DateOnly.TryParse(input, out DateOnly date))
        {
            newCell = new DateCell(date);
        }
        else if (TimeOnly.TryParse(input, out TimeOnly time))
        {
            newCell = new TimeCell(time);
        }
        else if (decimal.TryParse(input, out decimal dec))
        {
            newCell = new DecimalCell(dec);
        }
        else if (int.TryParse(input, out int num))
        {
            newCell = new DecimalCell(num);
        }

        else newCell = new TextCell(input);

        Sheet.Cells[cell.Row, cell.Column] = newCell;
    }

    public void ClearCell(SpreadsheetLocation cell) => Sheet.Cells[cell.Row, cell.Column] = new EmptyCell();

    public void ClearRow(int row)
    {
        for (int c = 0; c < Sheet.Width; c++)
            Sheet.Cells[row, c] = new EmptyCell();
    }
}