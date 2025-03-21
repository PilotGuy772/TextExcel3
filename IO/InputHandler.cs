using TextExcel3.Cells;
using TextExcel3.Cells.Data.Util;

namespace TextExcel3.IO;

public class InputHandler(Spreadsheet sheet, DisplayWindow window, DataManager data, CellFiller filler, HistoryManager history)
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
    private CellRange? Selection { get; set; }
    private CellRange? OldSelection { get; set; }
    public SpreadsheetLocation CursorLocation
    {
        get => new(CursorX, CursorY);
        set
        {
            CursorX = value.Column;
            CursorY = value.Row;
        }
    }
    private VimMode Mode { get; set; } = VimMode.Normal;
    private Spreadsheet Sheet { get; } = sheet;
    private DisplayWindow Window { get; } = window;
    private DataManager Data { get; } = data;
    private CellFiller Filler { get; } = filler;
    private HistoryManager History { get; } = history;
    private IClipboardItem? Clipboard { get; set; }
    
    /// <summary>
    /// Appropriately redraws cells affected by a change in the position of the cursor or the selection.
    /// </summary>
    public void RedrawCursorCells()
    {
        // update selected cell
        Filler.FillCell(new SpreadsheetLocation { Row = OldCursorY, Column = OldCursorX});

        if (Mode == VimMode.Visual)
        {
            Selection ??= new CellRange(CursorLocation, CursorLocation);
            OldSelection ??= new CellRange(CursorLocation, CursorLocation);
            
            OldSelection.RangeEnd.Column = Selection.RangeEnd.Column;
            OldSelection.RangeEnd.Row = Selection.RangeEnd.Row;
            Selection.RangeEnd = CursorLocation;
            
            Filler.FillCellRange(OldSelection);
            Filler.FillCellRange(Selection, ConsoleColor.Cyan);
        }
        
        Filler.FillCell(CursorLocation, ConsoleColor.Red);
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

    // private void VisualMode()
    // {
    //     Selection ??= new CellRange(CursorLocation, CursorLocation);
    //     OldSelection ??= new CellRange(CursorLocation, CursorLocation);
    //     if (!Equals(Selection.RangeEnd, CursorLocation))
    //     {
    //         OldSelection = Selection;
    //         Selection.RangeEnd = CursorLocation;
    //     }
    // }

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
        if (key.KeyChar == '/' && Mode == VimMode.Normal)
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
            
            case ConsoleKey.Escape when Mode == VimMode.Visual && CommandBuffer.Length == 0:
                Mode = VimMode.Normal;
                Filler.FillCellRange(Selection);
                Selection = null;
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
            case "x" when Mode == VimMode.Normal:
                Data.ClearCell(CursorLocation);
                break;
            case "dd" when Mode == VimMode.Normal:
                Data.ClearRow(CursorY);
                Filler.FillCellRange(new CellRange(new SpreadsheetLocation(
                        Window.HorizontalRangeStart,
                        CursorY),
                    new SpreadsheetLocation(
                        Window.HorizontalRangeStart + Window.HorizontalRangeSize,
                        CursorY)));
                
                break;
            case "i" when Mode == VimMode.Normal:
                Mode = VimMode.Insert;
                break;
            case "Cd":
                ScrollVertical(Window.VerticalRangeSize / 2);
                break;
            case "Cu":
                ScrollVertical(Window.VerticalRangeSize / -2);
                break;
            case "gg":
                ScrollVertical(Window.VerticalRangeStart * -1);
                break;
            case "s" when Mode == VimMode.Normal:
                Sheet.AddRow(CursorY + 1);
                Filler.FillAllCells();
                break;
            case "S" when Mode == VimMode.Normal:
                Sheet.AddRow(CursorY);
                Filler.FillAllCells();
                break;
            case "u":
                History.Undo();
                break;
            case "Cr":
                History.Redo();
                break;
            case "y" when Mode == VimMode.Normal:
                Clipboard = new SingleCellClipboard(Sheet.GetCell(CursorLocation));
                break;
            case "y" when Mode == VimMode.Visual:
                ICell[,] range = new ICell[CursorY - Selection.RangeStart.Row + 1, CursorX - Selection.RangeStart.Column + 1];
                for (int r = Selection.RangeStart.Row; r <= Selection.RangeEnd.Row; r++)
                {
                    for (int c = Selection.RangeStart.Column; c <= Selection.RangeEnd.Column; c++)
                    {
                        range[r + Selection.RangeStart.Row, c + Selection.RangeStart.Column] = Sheet.GetCell(new SpreadsheetLocation(c, r));
                    }
                }

                Clipboard = new RangeClipboard { Contents = range };
                break;
            case "p" when Mode == VimMode.Normal:
                //if (Clipboard is null) Console.Beep();
                Clipboard?.PasteItem(Sheet, CursorLocation);
                Filler.FillAllCells();
                break;
            case "v":
                Mode = VimMode.Visual;
                break;
            default:
                return false;
        }
        
        return true;
    }

    public void ScrollHorizontal(int amount)
    {
        Window.PrintGrid(Window.VerticalRangeStart, Window.HorizontalRangeStart + amount);
        Filler.FillAllCells();
        RedrawCursorCells();
    }
    
    public void ScrollVertical(int amount)
    {
        if (Window.VerticalRangeStart + amount < 0)
        {
            ScrollVertical(Window.VerticalRangeStart * -1);
            return;
        }
        Window.PrintGrid(Window.VerticalRangeStart + amount, Window.HorizontalRangeStart);
        // if (CursorY < Window.VerticalRangeStart || CursorY > Window.VerticalRangeStart + Window.HorizontalRangeSize)
        // {
        //     CursorY = Window.VerticalRangeStart + 1;
        //     OldCursorY = CursorY;
        // }

        Filler.FillAllCells();
        RedrawCursorCells();
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
                case ConsoleKey.Home:
                    Console.CursorLeft -= cursorPos;
                    cursorPos = 0;
                    break;
                case ConsoleKey.End:
                    Console.CursorLeft += input.Length - cursorPos;
                    cursorPos = input.Length;
                    break;
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