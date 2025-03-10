namespace TextExcel3.IO;

public class InputHandler(Spreadsheet sheet, DisplayWindow window)
{
    public int CursorX { get; private set; }
    public int CursorY { get; private set; }
    public VimMode Mode { get; set; } = VimMode.Normal;
    public Spreadsheet Sheet { get; private set; } = sheet;
    public DisplayWindow Window { get; private set; } = window;

    public void AwaitInput(out string command)
    {
        Console.CursorVisible = false;
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
        if (clearAfterInput)
        {
            Console.CursorLeft -= cursorPos;
            Console.Write(new string(' ', input.Length));
        }
        return input;
    }
}