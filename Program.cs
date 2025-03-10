using TextExcel3.IO;

namespace TextExcel3;

class Program
{
    static void Main(string[] args)
    {
        Spreadsheet excel = new Spreadsheet();
        //excel.Run();
        DisplayWindow window = new(excel);
        window.PrintGrid(0, 0);
        CellFiller filler = new(excel, window);
        filler.FillAllCells();
        InputHandler input = new(excel, window);
        input.AwaitInput(out string command);

        Console.CursorVisible = true;
        Console.WriteLine();
    }
}