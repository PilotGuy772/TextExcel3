using TextExcel3.Cells;
using TextExcel3.Cells.Data;
using TextExcel3.Formulas;
using TextExcel3.Formulas.Components;
using TextExcel3.IO;

namespace TextExcel3;

internal static class Program
{
    public static bool Quit { get; set; }
    private static void Main(string[] args)
    {
        // const string arithmetic = "1 + 2 * 3 / 4 - 5 ^ 6";
        // string rpn = FormulaBuilder.ShuntingYard(arithmetic);
        // Console.WriteLine(rpn);
        // Console.WriteLine(FormulaBuilder.RpnToFormulaTerm(rpn));
        // const string formula = "add(avg(a1:a3), 1)"; // 3
        // Spreadsheet test = new();
        // test.SetCell(new SpreadsheetLocation("A1"), new DecimalCell(1), true);
        // test.SetCell(new SpreadsheetLocation("A2"), new DecimalCell(2), true);
        // test.SetCell(new SpreadsheetLocation("A3"), new DecimalCell(3), true);
        //
        //
        // IFormulaTerm term = FormulaBuilder.BuildFormula(formula, test);
        // Console.WriteLine(term.DecimalValue);

        Spreadsheet excel = new();
        DisplayWindow window = new(excel);
        window.PrintGrid(0, 0);
        CellFiller filler = new(excel, window);
        filler.FillAllCells();
        HistoryManager history = new(excel);
        excel.History = history;
        DataManager data = new(excel);
        InputHandler input = new(excel, window, data, filler, history);
        
        if (args.Length > 0)
        {
            string fileName = args[0];
            excel.OpenFile = fileName;
            data.LoadCsv(File.ReadAllText(fileName));
            filler.FillAllCells();
        }
        
        filler.FillCell(new SpreadsheetLocation { Row = input.CursorY, Column = input.CursorX}, ConsoleColor.Red);
        int consoleW = Console.WindowWidth;
        int consoleH = Console.WindowHeight;
        
        while (!Quit)
        {
            input.AwaitInput();
            input.RedrawCursorCells();
            
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