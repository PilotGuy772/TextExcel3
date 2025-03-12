using TextExcel3.Cells;
using TextExcel3.Cells.Data.Util;

namespace TextExcel3.IO;

public class InputHandler(Spreadsheet sheet, DisplayWindow window, DataManager data, CellFiller filler)
{
    private int _cursorX = window.HorizontalRangeStart;
    private int _cursorY = window.VerticalRangeStart;
    private string CommandBuffer { get; set; } = "";

    public int CursorX
    {
        get => _cursorX;
        set
        {
            OldCursorX = _cursorX;
            OldCursorY = _cursorY;
            _cursorX = value;
        }
    }
    public int CursorY
    {
        get => _cursorY;
        set
        {
            OldCursorX = _cursorX;
            OldCursorY = _cursorY;
            _cursorY = value;
        }
    }
    public int OldCursorX { get; set; } = window.HorizontalRangeStart;
    public int OldCursorY { get; set; } = window.VerticalRangeStart;
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
        Filler.FillCell(new SpreadsheetLocation { Row = OldCursorY, Column = OldCursorX});
        Filler.FillCell(new SpreadsheetLocation { Row = CursorY, Column = CursorX}, ConsoleColor.Red);
    }
    
    private void InsertMode()
    {
        Filler.ClearFormulaBar();
        Console.CursorVisible = true;
        Console.SetCursorPosition(Window.FormulaBarValueStart, 0);

       
        string newRawValue =
            LineSafeInput(out ConsoleKeyInfo exitKey, Sheet.GetCell(CursorLocation).ToString() ?? "", true);

        if (exitKey.Key == ConsoleKey.Escape)
        {
            Mode = VimMode.Normal;
            return;
        }
        
        Data.AssignCell(CursorLocation, newRawValue);
        //Data.AssignCell(CursorLocation, ((int)exitKey.Modifiers).ToString());
        
        Console.CursorVisible = false;
        
        // Alt + Enter moves the cursor RIGHT instead of DOWN
        if ((exitKey.Modifiers & ConsoleModifiers.Alt) != 0 && CursorX < Window.HorizontalRangeStart + Window.HorizontalRangeSize)
        {
            CursorX++;
            RedrawCursorCells();
            return;
        }
        
        // No modifier moves the cursor DOWN
        if (CursorY < Window.VerticalRangeStart + Window.VerticalRangeSize)
        {
            CursorY++;
        }

    }

    public void AwaitInput()
    {
        Console.CursorVisible = false;
        // INSERT MODE //
        if (Mode == VimMode.Insert)
        {
            InsertMode();
            return;
        }


        // NORMAL MODE //
        Filler.FillFormulaRow(CursorLocation);
        RedrawCursorCells();
        // block for key input, then act according to the input
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        // first, check for a free text input command from the user
        if (key.KeyChar == '/')
        {
            Console.SetCursorPosition(3, Window.CommandBarDistance);
            Console.CursorVisible = true;
            string command = LineSafeInput("/", true);
            Console.CursorVisible = false;
            Sheet.ProcessCommand(command);
        }
        else if (key.KeyChar == ':')
        {
            Console.SetCursorPosition(3, Window.CommandBarDistance);
            Console.CursorVisible = true;
            string command = LineSafeInput(":", true);
            Console.CursorVisible = false;
            Sheet.ProcessCommand(command);
        }

        bool useBuffer = false;
        switch (key.Key)
        {
            /* VIM CURSOR MOVER INPUTS */
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
            case ConsoleKey.J: case ConsoleKey.DownArrow: case ConsoleKey.Enter:
                if (CursorY == Window.VerticalRangeStart + Window.VerticalRangeSize) break;
                CursorY++;
                break;
            
            default:
                useBuffer = true;
                break;
        }

        if (!useBuffer)
        {
            RedrawCursorCells();
            return;
        }
        
        // finally, for all other commands, use a buffer and input loop 
        // exit the loop only when a valid command is received or escape is pressed
        CommandBuffer += key.KeyChar;

        if (CheckNormalModeCommand(CommandBuffer) || key.Key == ConsoleKey.Escape)
        {
            Filler.ClearCommandBar();
            CommandBuffer = "";
        }
        else
        {
            Console.SetCursorPosition(3, Window.CommandBarDistance);
            Console.Write(CommandBuffer);
        }
        
        RedrawCursorCells();

    }

    private bool CheckNormalModeCommand(string buffer)
    {
        switch (buffer)
        {
            case "x":
                Data.ClearCell(CursorLocation);
                break;
            case "dd":
                Data.ClearRow(CursorY);
                Filler.FillCellRange(new CellRange(new SpreadsheetLocation(
                        Window.HorizontalRangeStart,
                        CursorY),
                    new SpreadsheetLocation(
                        Window.HorizontalRangeStart + Window.HorizontalRangeSize,
                        CursorY)));
                
                break;
            case "i":
                Mode = VimMode.Insert;
                break;
            case "s":
                Mode = VimMode.Insert;
                Data.ClearCell(CursorLocation);
                break;
            default:
                return false;
        }
        
        return true;
    }
    
    

    public static string LineSafeInput(string prefill = "", bool clearAfterInput = false) =>
        LineSafeInput(out _, prefill, clearAfterInput);

    /// <summary>
    /// Step-by-step input that intercepts newlines and other disruptive characters. Input terminates when `RET` is received.
    /// </summary>
    /// <param name="exitKey">Information about the key that was used to terminate the text input.</param>
    /// <param name="prefill">The editable string to prefill in the input field</param>
    /// <param name="clearAfterInput">Whether to clear the input field after inputting is finished.</param>
    /// <returns>The string inputted by the user</returns>
    public static string LineSafeInput(out ConsoleKeyInfo exitKey, string prefill = "", bool clearAfterInput = false)
    {
        Console.Write(prefill);
        

        int cursorPos = prefill.Length;
        ConsoleKeyInfo key = Console.ReadKey(true);
        string input = prefill;
        while (true)
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
                //case ConsoleKey.Tab: case ConsoleKey.UpArrow: case ConsoleKey.DownArrow:
                //    break;
                // escape voids input
                case ConsoleKey.Escape: case ConsoleKey.Enter:
                    if (clearAfterInput)
                    {
                        Console.CursorLeft -= cursorPos;
                        Console.Write(new string(' ', input.Length));
                    }

                    exitKey = key;
                    return input;
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
    }
}