using System.Data;
using System.Runtime.InteropServices;
using TextExcel3.Cells;

namespace TextExcel3;

/// <summary>
/// Represents a single spreadsheet.
/// </summary>
public class Spreadsheet
{
    /// <summary>
    /// Central grid for the spreadsheet. Do not modify directly.
    /// </summary>
    public ICell[,] Cells { get; set; }
    public int Width { get => Cells.GetLength(1); }
    public int Height { get => Cells.GetLength(0); }
    private (int X, int Y) CursorPosition { get; set; }
    private bool Quit { get; set; }

    /// <summary>
    /// Initialize a new spreadsheet filled with empty cells of the default size (20r x 12c)
    /// </summary>
    public Spreadsheet()
    {
        CursorPosition = (0, 0);
        Cells = new ICell[20, 12];
        
        for (int i = 0; i < Cells.GetLength(0); i++)
            for (int j = 0; j < Cells.GetLength(1); j++)
                Cells[i, j] = new EmptyCell();
    }

    public ICell GetCell(SpreadsheetLocation location) => Cells[location.Row, location.Column];
    
    public void Draw()
    {
        // start by writing the formula bar
        Console.Write("  |" + SpreadsheetLocation.GetLetterFromNumber(CursorPosition.X) + (CursorPosition.Y + 1) + "| f: ");
        Console.Write(Cells[CursorPosition.Y, CursorPosition.X].FormattedRealValue);
        
        // for now, all cells have a width of 10
        Console.WriteLine();
        int leftPadding = (int)Math.Floor(Math.Log10(Width));
        
        Console.Write(new string(' ', leftPadding + 2));
        for (int topColumns = 0; topColumns < Width; topColumns++)
        {
            ColorWrite('|', CursorPosition.X == topColumns || CursorPosition.X == topColumns - 1 ? ConsoleColor.DarkRed : ConsoleColor.White);
            
            ColorWrite("-" + SpreadsheetLocation.GetLetterFromNumber(topColumns) + new string('-', 8),
                topColumns == CursorPosition.X ? ConsoleColor.DarkRed : ConsoleColor.White);
        }

        Console.WriteLine('|');
        ConsoleColor oldbg = Console.BackgroundColor;
        
        // Top row is now DONE, now to print cell contents in the grid
        for (int r = 0; r < Height; r++)
        {
            // first, row number padded with justified padding
            // this will print a space as many times as the number of digits in
            // the total number of cells MINUS the number of digits in the current row number
            ColorWrite(new string(' ', leftPadding - (int)Math.Floor(Math.Log10(r + 1))) + (r + 1), CursorPosition.Y == r ? ConsoleColor.DarkRed : ConsoleColor.White);
            ConsoleColor color = ConsoleColor.White;
            if (r == CursorPosition.Y | CursorPosition.X == 0) color = ConsoleColor.DarkRed;
            if (r == CursorPosition.Y && CursorPosition.X == 0) color = ConsoleColor.Red;
            ColorWrite(" |", color);
            
            // now to iterate through columns
            for (int c = 0; c < Width; c++)
            {
                color = ConsoleColor.White;
                char separator = '|';
                
                if (r == CursorPosition.Y) color = ConsoleColor.DarkRed;
                else if (c == CursorPosition.X - 1 || c == CursorPosition.X) color = ConsoleColor.DarkRed;
                
                if (c == CursorPosition.X - 1 && r == CursorPosition.Y)
                {
                    // this cell is immediately to the LEFT of the selected cell
                    separator = '>';
                    color = ConsoleColor.Red;
                    
                }
                else if (c == CursorPosition.X && r == CursorPosition.Y)
                {
                    // this cell is the selected cell
                    separator = '<';
                    color = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Red;
                }

                Console.Write(Cells[r, c].FormattedDisplayValue(10));
                Console.BackgroundColor = oldbg;
                
                ColorWrite(separator, color);
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Draws and empty grid with no values in the cells. This will print a grid the size of the whole sheet, or as large as the character width and heights given.
    /// </summary>
    /// <param name="width">The maximum width to use to draw</param>
    /// <param name="height">The maximum height to use to draw</param>
    public void DrawGrid(int width, int height)
    {
        // start by writing the formula bar
        //Console.Write("  |" + SpreadsheetLocation.GetLetterFromNumber(CursorPosition.X) + (CursorPosition.Y + 1) + "| f: ");
        //Console.Write(Cells[CursorPosition.Y, CursorPosition.X].FormattedRealValue);
        
        // for now, all cells have a width of 10
        Console.WriteLine();
        int leftPadding = (int)Math.Floor(Math.Log10(Width));
        
        Console.Write(new string(' ', leftPadding + 2));
        for (int topColumns = 0; topColumns < Width; topColumns++)
        {
            Console.Write('|');
            Console.Write("-" + SpreadsheetLocation.GetLetterFromNumber(topColumns) + new string('-', 8));
        }

        Console.WriteLine('|');
        
        // Top row is now DONE, now to print cell contents in the grid
        for (int r = 0; r < Height; r++)
        {
            // first, row number padded with justified padding
            // this will print a space as many times as the number of digits in
            // the total number of cells MINUS the number of digits in the current row number
            Console.Write(new string(' ', leftPadding - (int)Math.Floor(Math.Log10(r + 1))) + (r + 1));
            Console.Write(" |");
            
            // now to iterate through columns
            for (int c = 0; c < Width; c++)
            {
                Console.Write("          |");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Start the command loop
    /// </summary>
    public void Run()
    {
        VimMode mode = VimMode.Normal;
        
        // VIM-like commands
        // arrow keys as well as HJKL move you around the sheet
        // use Console.ReadKey() to get inputs
        while (!Quit)
        {
            Console.Clear();
            
            // print sheet first
            Draw();
            
            ConsoleKeyInfo input = Console.ReadKey(true);
            
            // switch-case to decide what to do
            switch (input.Key)
            {
                case ConsoleKey.UpArrow: case ConsoleKey.K:
                    CursorPosition = (CursorPosition.X, Math.Max(0, CursorPosition.Y - 1));
                    continue;
                case ConsoleKey.DownArrow: case ConsoleKey.J:
                    CursorPosition = (CursorPosition.X, Math.Min(Height - 1, CursorPosition.Y + 1));
                    continue;
                case ConsoleKey.LeftArrow: case ConsoleKey.H:
                    CursorPosition = (Math.Max(0, CursorPosition.X - 1), CursorPosition.Y);
                    continue;
                case ConsoleKey.RightArrow: case ConsoleKey.L:
                    CursorPosition = (Math.Min(Width - 1, CursorPosition.X + 1), CursorPosition.Y);
                    continue;
                case ConsoleKey.I:
                    mode = VimMode.Insert;
                    continue;
                case ConsoleKey.V:
                    mode = VimMode.Visual;
                    continue;
            }

            switch (input.KeyChar)
            {
                case ':':
                    // this is VIM-style command input
                    Console.Write("\n: ");
                    ProcessCommand(Console.ReadLine() ?? "");
                    break;
            }

        }
    }

    private void ProcessCommand(string command)
    {
        // process VIM-style text commands
        switch (command)
        {
            case "q":
                Quit = true;
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
}