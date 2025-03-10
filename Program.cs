using TextExcel3.Cells;
using TextExcel3.IO;

namespace TextExcel3;

class Program
{
    static void Main(string[] args)
    {
        Spreadsheet excel = new Spreadsheet();
        //excel.Run();
        DisplayWindow window = new(excel);
        window.PrintGrid(8, 3);
        CellFiller filler = new(excel, window);
        filler.FillAllCells();
        DataManager data = new(excel);
        InputHandler input = new(excel, window, data, filler);
        
        filler.FillCell(new SpreadsheetLocation { Row = input.CursorY, Column = input.CursorX}, ConsoleColor.Red);

        while (true)
        {
            
            int oldCursorX = input.CursorX;
            int oldCursorY = input.CursorY;
            input.AwaitInput(out string command);
            input.RedrawCursorCells();
        }
    }
}