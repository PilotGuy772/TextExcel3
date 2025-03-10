using TextExcel3.Cells;

namespace TextExcel3.IO;

public class InputHandler(Spreadsheet sheet, DisplayWindow window, DataManager data, CellFiller filler)
{
    public int CursorX { get; private set; } = window.HorizontalRangeStart;
    public int CursorY { get; private set; } = window.VerticalRangeStart;
    public int OldCursorX { get; private set; } = window.HorizontalRangeStart;
    public int OldCursorY { get; private set; } = window.VerticalRangeStart;
    public SpreadsheetLocation CursorLocation
    {
        get => new(CursorX, CursorY);
        set
        {
            CursorX = value.Column;
            CursorY = value.Row;
        }
    }
    public int? SelectionStartX { get; private set; } = null;
    public int? SelectionStartY { get; private set; } = null;
    private VimMode Mode { get; set; } = VimMode.Normal;
    private Spreadsheet Sheet { get; } = sheet;
    private DisplayWindow Window { get; } = window;
    private DataManager Data { get; } = data;
    private CellFiller Filler { get; } = filler;
    
    /// <summary>
    /// Appropriately redraws cells affected by a change in the position of the cursor or the selection.
    /// </summary>
    public void RedrawCursorCells()
    {
        // update selected cell
        filler.FillCell(new SpreadsheetLocation { Row = OldCursorY, Column = OldCursorX});
        filler.FillCell(new SpreadsheetLocation { Row = CursorY, Column = CursorX}, ConsoleColor.Red);
    }
    
    private void InsertMode()
    {
        Filler.ClearFormulaBar();
        Console.CursorVisible = true;
        Console.SetCursorPosition(Window.FormulaBarValueStart, 0);
        string newRawValue =
            LineSafeInput(Sheet.GetCell(CursorLocation).ToString() ?? "", true);
        Data.AssignCell(CursorLocation, newRawValue);
        Mode = VimMode.Normal;
        Console.CursorVisible = false;
    }

    public void AwaitInput(out string command)
    {
        Console.CursorVisible = false;
        // INSERT MODE //
        if (Mode == VimMode.Insert)
            InsertMode();
        
        
        // NORMAL MODE //
        filler.FillFormulaRow(new SpreadsheetLocation(CursorX, CursorY));
        // block for key input, then act according to the input
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        // first, check for a free text input command from the user
        if (key.KeyChar == '/')
        {
            Console.SetCursorPosition(3, Window.CommandBarDistance);
            Console.CursorVisible = true;
            command = LineSafeInput("/", true);
            Console.CursorVisible = false;
        }
        else if (key.KeyChar == ':')
        {
            Console.SetCursorPosition(3, Window.CommandBarDistance);
            Console.CursorVisible = true;
            command = LineSafeInput(":", true);
            Console.CursorVisible = false;
        }

        command = "";
        OldCursorX = CursorX;
        OldCursorY = CursorY;
        switch (key.Key)
        {
            case ConsoleKey.H: case ConsoleKey.LeftArrow:
                if (CursorX == Window.HorizontalRangeStart) break;
                CursorX--;
                break;
            case ConsoleKey.L: case ConsoleKey.RightArrow:
                if (CursorX == Window.HorizontalRangeStart + Window.HorizontalRangeSize) break;
                CursorX++;
                break;
            case ConsoleKey.K: case ConsoleKey.UpArrow:
                if (CursorY == Window.VerticalRangeStart) break;
                CursorY--;
                break;
            case ConsoleKey.J: case ConsoleKey.DownArrow:
                if (CursorY == Window.VerticalRangeStart + Window.VerticalRangeSize) break;
                CursorY++;
                break;
            case ConsoleKey.I: case ConsoleKey.Insert:
                // insert mode
                Mode = VimMode.Insert;
                break;
        }
        
    }

    /// <summary>
    /// Step-by-step input that intercepts newlines and other disruptive characters. Input terminates when `RET` is received.
    /// </summary>
    /// <param name="prefill">The editable string to prefill in the input field</param>
    /// <returns>The string inputted by the user</returns>
    public static string LineSafeInput(string prefill = "", bool clearAfterInput = false)
    {
        Console.Write(prefill);
        

        int cursorPos = prefill.Length;
        ConsoleKeyInfo key = Console.ReadKey(true);
        string input = prefill;
        while (key.Key != ConsoleKey.Enter)
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace when cursorPos != 0 && cursorPos != input.Length:
                    // to make backspace work we have to:
                    // move every character RIGHT of the cursor to the left one
                    // replace the last character with nothing
                    // 0123456789
                    // abcdefghij
                    input = input[..(cursorPos - 1)] + input[cursorPos..];
                    cursorPos--;
                    Console.CursorLeft--;
                    Console.Write(input[(cursorPos)..] + ' ');
                    Console.CursorLeft -= input.Length - cursorPos + 1;
                    
                    break;
                
                case ConsoleKey.Backspace when cursorPos == input.Length:
                    // backspace at the end of the string is simpler
                    // just move th cursor backwards, print a space, and move it back again
                    cursorPos--;
                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;
                    input = input[..(input.Length - 1)];
                    break;
                
                case ConsoleKey.Delete when cursorPos != input.Length:
                    input = input[..cursorPos] + input[(cursorPos + 1)..];
                    Console.Write(input[cursorPos..] + ' ');
                    Console.CursorLeft -= input.Length - cursorPos + 1;
                    break;
                case ConsoleKey.LeftArrow when cursorPos != 0:
                    cursorPos--;
                    Console.CursorLeft--;
                    break;
                case ConsoleKey.RightArrow when cursorPos != input.Length:
                    cursorPos++;
                    Console.CursorLeft++;
                    break;
                // forbidden characters
                case ConsoleKey.Tab: case ConsoleKey.UpArrow: case ConsoleKey.DownArrow: case ConsoleKey.Delete:
                    break;
                // escape voids input
                case ConsoleKey.Escape:
                    if (!clearAfterInput) return prefill;
                    
                    Console.CursorLeft -= cursorPos;
                    Console.Write(new string(' ', input.Length));
                    return prefill;
                default:
                    // if the cursor is not at the end, we need to account for overtype vs. insert
                    // so, after every input, we have to shift right
                    int oldCoord = Console.CursorLeft;
                    input = input.Insert(cursorPos, "" + key.KeyChar);
                    Console.Write(input[cursorPos..]);
                    cursorPos++;
                    Console.CursorLeft = oldCoord + 1;
                    
                    break;
            }

            key = Console.ReadKey(true);


        }

        Console.CursorLeft += input.Length - cursorPos;
        
        if (!clearAfterInput) return input;
        
        Console.CursorLeft -= cursorPos;
        Console.Write(new string(' ', input.Length));
        return input;
    }
}