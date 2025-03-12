using TextExcel3.Cells;
using TextExcel3.IO;

namespace TextExcel3;

internal static class Program
{
    public static bool Quit { get; set; }
    private static void Main()
    {
        Spreadsheet excel = new Spreadsheet();
        DisplayWindow window = new(excel);
        window.PrintGrid(0, 0);
        CellFiller filler = new(excel, window);
        filler.FillAllCells();
        HistoryManager history = new(excel);
        excel.History = history;
        DataManager data = new(excel);
        InputHandler input = new(excel, window, data, filler, history);
        
        filler.FillCell(new SpreadsheetLocation { Row = input.CursorY, Column = input.CursorX}, ConsoleColor.Red);
        int consoleW = Console.WindowWidth;
        int consoleH = Console.WindowHeight;
        
        while (!Quit)
        {
            try
            {
                input.AwaitInput();
                input.RedrawCursorCells();
            }
            catch(Exception e)
            {
                //if (e is not ArgumentException) throw;
                throw;
            }
            
            if (Console.WindowHeight != consoleH || Console.WindowWidth != consoleW)
            {
                input.CursorX = 0;
                input.CursorY = 0;
                input.OldCursorX = 0;
                input.OldCursorY = 0;
                window.PrintGrid(0,0);

                filler.FillAllCells();
                input.RedrawCursorCells();
                
                consoleW = Console.WindowWidth;
                consoleH = Console.WindowHeight;
            }

            if (input.CursorX == window.HorizontalRangeStart + window.HorizontalRangeSize)
            {
                input.ScrollHorizontal(1);
            }
            
            if (input.CursorX == window.HorizontalRangeStart && input.CursorX != 0)
            {
                input.ScrollHorizontal(-1);
            }
            
            if (input.CursorY == window.VerticalRangeStart + window.VerticalRangeSize)
            {
                input.ScrollVertical(1);
            }

            if (input.CursorY == window.VerticalRangeStart && input.CursorY != 0)
            {
                input.ScrollVertical(-1);
            }
            
        }

        Console.CursorVisible = true;
        Console.Clear();
    }
}