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

        while (!Quit)
        {
            input.AwaitInput();
            input.RedrawCursorCells();
        }

        Console.CursorVisible = true;
        Console.Clear();
    }
}