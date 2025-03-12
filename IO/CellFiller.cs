using System.Security.Principal;
using TextExcel3.Cells.Data.Util;

namespace TextExcel3.IO;

/// <summary>
/// Manages drawing cell values on top of an already printed grid
/// </summary>
public class CellFiller(Spreadsheet sheet, DisplayWindow window)
{
    private Spreadsheet Sheet { get; set; } = sheet;
    private DisplayWindow Window { get; set; } = window;
    
    /// <summary>
    /// Clears out any text still in the formula bar
    /// </summary>
    public void ClearFormulaBar()
    {
        Console.SetCursorPosition(Window.FormulaBarValueStart, 0);
        Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
    }

    public void ClearCommandBar()
    {
        Console.SetCursorPosition(3, Window.CommandBarDistance);
        Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
    }
    
    public void FillCell(SpreadsheetLocation cell, ConsoleColor? background = null)
    {
        if (cell.Row < Window.VerticalRangeStart || cell.Row > Window.VerticalRangeStart + Window.VerticalRangeSize
                                                 || cell.Column < Window.HorizontalRangeStart 
                                                 || cell.Column > Window.HorizontalRangeStart + Window.HorizontalRangeSize)
            throw new ArgumentException("The given cell is out of the range of the display window: " + cell.Row + ", " + cell.Column);
        
        // first, find the console coordinates of the given cell
        // based on its spreadsheet location, the offsets, and the display range
        int cellX = (cell.Column - Window.HorizontalRangeStart) * 11 + Window.FirstCellOffsetHorizontal + 1;
        int cellY = cell.Row - Window.VerticalRangeStart + Window.FirstCellOffsetVertical;
        
        // then simply print the formatted display value at this cursor position
        // optionally with the background color changed
        Console.CursorVisible = false;
        Console.SetCursorPosition(cellX, cellY);
        
        ConsoleColor old = Console.BackgroundColor;
        if (background is not null)
            Console.BackgroundColor = (ConsoleColor)background;
        
        Console.Write(Sheet.GetCell(cell).FormattedDisplayValue(10));
        Console.BackgroundColor = old;

    }

    public void FillAllCells()
    {
        for (int r = Window.VerticalRangeStart; r <= Window.VerticalRangeStart + Window.VerticalRangeSize; r++)
        {
            for (int c = Window.HorizontalRangeStart; c <= Window.HorizontalRangeStart + Window.HorizontalRangeSize; c++)
            {
                FillCell(new SpreadsheetLocation(c, r));
            }
        }
        
        //Console.SetCursorPosition(3, Window.CommandBarDistance);
        //Console.CursorVisible = true;
    }

    public void FillFormulaRow(SpreadsheetLocation cell)
    {
        ClearFormulaBar(); 
        Console.SetCursorPosition(2, 0);
        Console.Write(cell.FriendlyName);
        Console.Write(new string(' ', Window.FormulaBarCoordinateWidth - cell.FriendlyName.Length));
        Console.CursorLeft = Window.FormulaBarValueStart;
        Console.Write(Sheet.GetCell(cell).FormattedRealValue);
    }

    public void FillCellRange(CellRange range)
    {
        foreach (SpreadsheetLocation loc in range.AllCells)
            FillCell(loc);
    }
    
}