using System.Globalization;
using TextExcel3.Cells.Data;
using TextExcel3.IO;

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
        
        ICell newCell;
        if (input.StartsWith('='))
        {
            newCell = new FormulaCell(input, Sheet);
        }
        else if (input.StartsWith('"') && input.EndsWith('"'))
        {
            newCell = new TextCell(input[1..^1]);
        }
        else if (DateOnly.TryParse(input, out DateOnly date))
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
        else if (input.EndsWith('%'))
        {
            string percentString = input.TrimEnd('%');
            if (decimal.TryParse(percentString, out decimal percent))
            {
                newCell = new PercentCell(percent);
            }
            else
            {
                newCell = new TextCell(input);
            }
        }

        else newCell = new TextCell(input);

        Sheet.SetCell(cell, newCell);
    }

    public void ClearCell(SpreadsheetLocation cell) => Sheet.SetCell(cell, new EmptyCell());


    public void ClearRow(int row)
    {
        for (int c = 0; c < Sheet.Width; c++)
            Sheet.SetCell(new SpreadsheetLocation(c, row), new EmptyCell());
    }

    /// <summary>
    /// Clears the entire sheet and loads the given CSV into the sheet
    /// </summary>
    /// <param name="csv">Raw string representation of the CSV file to load</param>
    public void LoadCsv(string csv)
    {
        Sheet.Clear();
        string[] rows = csv.Split('\n');
        for (int r = 0; r < rows.Length; r++)
        {
            string[] cells = rows[r].Split(',');
            for (int c = 0; c < cells.Length; c++)
            {
                if (cells[c].Trim() == "") continue;
                AssignCell(new SpreadsheetLocation(c, r), cells[c]);
            }
        }
    }
}