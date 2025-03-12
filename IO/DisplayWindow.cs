namespace TextExcel3.IO;

/// <summary>
/// Class for managing display of a spreadsheet to the terminal
/// </summary>
public class DisplayWindow(Spreadsheet sheet)
{
    public Spreadsheet Sheet { get; } = sheet;
    public int FormulaBarValueStart { get; private set; }
    public int FormulaBarCoordinateWidth { get; private set; }
    public int FirstCellOffsetHorizontal { get; private set; }
    public int FirstCellOffsetVertical { get; private set; } = 2;
    public int VerticalRangeStart { get; private set; }
    public int VerticalRangeSize { get; private set; }
    public int HorizontalRangeStart { get; private set; }
    public int HorizontalRangeSize { get; private set; }
    public int CommandBarDistance { get; private set; }
    
    
    /// <summary>
    /// Print the basic grid with row and column labels. This will clear the console before running.
    /// </summary>
    /// <param name="verticalRangeStart">Displays rows starting at the specified value and ending at the end of the sheet or when the terminal window out of space</param>
    /// <param name="horizontalRangeStart">Displays columns starting at the specified value and ending at the end of the sheet or when the terminal window runs out of space.</param>
    public void PrintGrid(int verticalRangeStart, int horizontalRangeStart)
    {
        Console.Clear();
        UsableWidthAndHeight(out int columns, out int rows);

        VerticalRangeStart = verticalRangeStart;
        VerticalRangeSize = /*verticalRangeStart + rows > Sheet.Height ? Sheet.Height - verticalRangeStart - 1 :*/ rows;
        HorizontalRangeStart = horizontalRangeStart;
        HorizontalRangeSize = /*horizontalRangeStart + columns > Sheet.Width ? Sheet.Width - horizontalRangeStart - 1 :*/ columns;
        
        // start by printing formula bar
        // The amount of empty space between the braces has to be
        // able to handle the number of digits that might be in 
        // any given cell coordinate in this selection
        int deadSpace = (int)Math.Ceiling(
            Math.Log10(verticalRangeStart + rows) + 
            Math.Ceiling((horizontalRangeStart + columns) / 26.0));

        string formulaBar = " [" + new string(' ', deadSpace) + "] f(x) => ";
        Console.WriteLine(formulaBar);
        FormulaBarCoordinateWidth = deadSpace;
        FormulaBarValueStart = formulaBar.Length;
        
        // next, print the column headers
        int rowDigits = (int)Math.Floor(Math.Log10(verticalRangeStart + rows + 1));
        FirstCellOffsetHorizontal = rowDigits + 2; // add two for the space and the pipe
        Console.Write(new string(' ', rowDigits + 2) + "|");
        for (int topBar = horizontalRangeStart; topBar <= HorizontalRangeStart + HorizontalRangeSize; topBar++)
        {
            string columnName = SpreadsheetLocation.GetLetterFromNumber(topBar);
            Console.Write("-" + columnName + new string('-', 9 - columnName.Length) + "|");
        }

        Console.WriteLine();
        
        // now we can finally print the grid
        for (int r = verticalRangeStart; r <= VerticalRangeStart + VerticalRangeSize; r++)
        {
            Console.Write(new string(' ', rowDigits - (int)Math.Floor(Math.Log10(r + 1))) + (r + 1) + " |");
            for (int c = horizontalRangeStart; c <= HorizontalRangeStart + HorizontalRangeSize; c++)
            {
                Console.Write("          |");
            }

            Console.WriteLine();
        }
        
        // and finish with the command row
        Console.Write(" > ");
        CommandBarDistance = Console.CursorTop;
    }

    private static void UsableWidthAndHeight(out int width, out int height)
    {
        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;
        
        // total number of rows can be height minus:
        // 1 for formula row
        // 1 for title row
        // 1 for command row
        height = consoleHeight - 4;
        
        // total number of columns can be width:
        // - 1 for number padding
        // - log_10( total rows ) for number column
        // / 11 for width of columns
        width = (int)Math.Floor((consoleWidth
                - 2
                - Math.Log10(height))
                / 11) - 1;
        
        
        
        //Console.WriteLine("Have terminal size of " + consoleWidth + "x" + consoleHeight + " and usable rows & cols " + width + "x" + height + ".");
    }
}