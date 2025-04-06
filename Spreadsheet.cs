using System.Data;
using System.Runtime.InteropServices;
using TextExcel3.Cells;
using TextExcel3.IO;

namespace TextExcel3;

/// <summary>
/// Represents a single spreadsheet.
/// </summary>
public class Spreadsheet
{
    /// <summary>
    /// Central grid for the spreadsheet. Do not modify directly.
    /// </summary>
    public List<List<ICell>> Cells { get; set; }
    public int Width => Cells[0].Count;
    public int Height => Cells.Count;
    private (int X, int Y) CursorPosition { get; set; }
    public HistoryManager History { get; set; }
    public string? OpenFile { get; set; }


    /// <summary>
    /// Initialize a new spreadsheet filled with empty cells of the default size (20r x 12c)
    /// </summary>
    public Spreadsheet()
    {
        CursorPosition = (0, 0);
        Cells = [];
        
        for (int i = 0; i < 20; i++)
        {
            Cells.Add([]);
            for (int j = 0; j < 20; j++)
                Cells[i].Add(new EmptyCell());
        }
    }
    
    public string GetCsv()
    {
        string csv = "";
        foreach (List<ICell> row in Cells)
        {
            foreach (ICell cell in row)
            {
                if (cell is EmptyCell) csv += ",";
                else csv += cell.FormattedRealValue + ",";
            }
            csv = csv[..^1];
            csv += "\n";
        }
        return csv;
    }

    public ICell GetCell(SpreadsheetLocation location)
    {
        // try to get the given cell... if it errors or returns null, 
        // resize the array and set the cell to a new empty cell
        //return Cells[location.Row, location.Column];
        VerifySize(location);

        return Cells[location.Row][location.Column];
    }

    public void SetCell(SpreadsheetLocation location, ICell newCell, bool skipHistory = false)
    {
        VerifySize(location);

        if (!skipHistory) History.RegisterAction(new ActionInformation(location, Cells[location.Row][location.Column], newCell));
        Cells[location.Row][location.Column] = newCell;
        
    }

    private void VerifySize(SpreadsheetLocation location)
    {
        while (Cells.Count <= location.Row)
            Cells.Add([]);

        while (Cells[location.Row].Count <= location.Column)
            Cells[location.Row].Add(new EmptyCell());
    }

    /// <summary>
    /// Add a new row to the sheet AFTER (below) the given row number. Does nothing if the requested action would have no visible effect.
    /// Please refill the grid after calling this to update affected cells.
    /// </summary>
    /// <param name="after"></param>
    public void AddRow(int after)
    {
        if (Cells.Count <= after) return;
        Cells.Insert(after, []);
    }
    
    public void ProcessCommand(string command)
    {
        // process VIM-style text commands
        if (command.StartsWith(":w"))
        {
            string[] args = command.Split(' ');
            string filename;
            if (args.Length > 1)
                filename = args[1];
            else if (OpenFile != null)
                filename = OpenFile;
            else return;
            
            Save(filename);
            
        }
        
        switch (command)
        {
            case ":wq":
                ProcessCommand(":w");
                ProcessCommand(":q");
                return;
            case ":q":
                Program.Quit = true;
                break;
        }
    }

    public void ColorWrite(object text, ConsoleColor color)
    {
        ConsoleColor old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = old;
        
    }

    public void Clear()
    {
        for (int r = 0; r < Cells.Count; r++)
            for (int c = 0; c < Cells[r].Count; c++)
                Cells[r][c] = new EmptyCell();
    }

    public void Save(string fileName)
    {
        File.WriteAllText(fileName, GetCsv());
    }
}