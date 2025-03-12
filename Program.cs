using TextExcel3.Cells;
using TextExcel3.IO;

namespace TextExcel3;

class Program
{
    public static bool Quit { get; set; }
    static void Main(string[] args)
    {
        Spreadsheet excel = new Spreadsheet();
        //excel.Run();
        DisplayWindow window = new(excel);
        window.PrintGrid(0, 0);
        CellFiller filler = new(excel, window);
        filler.FillAllCells();
        DataManager data = new(excel);
        InputHandler input = new(excel, window, data, filler);
        
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
                if (e is not ArgumentException) throw;
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
        }

        Console.CursorVisible = true;
        Console.Clear();
    }
}