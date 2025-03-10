namespace TextExcel3;

public class SpreadsheetLocation
{
    /// <summary>
    /// The normalized row represented by this location
    /// </summary>
    public int Row { get; set; }
    
    /// <summary>
    /// The normalized column represented by this location
    /// </summary>
    public int Column { get; set; }
    public string FriendlyName { get; set; }

    /// <summary>
    /// Construct a new instance based on the Excel-friendly representation of a location.
    /// </summary>
    /// <param name="id"></param>
    public SpreadsheetLocation(string id)
    {
        // only support single-letter references for now
        Column = GetNumberFromLetter(id[0]);
        Row = int.Parse(id[1..]);
        FriendlyName = id;
    }

    public SpreadsheetLocation(int x, int y)
    {
        Column = x;
        Row = y;
        FriendlyName = GetLetterFromNumber(x) + (y + 1);
    }

    public SpreadsheetLocation()
    {
    }

    /// <summary>
    /// Get the zero-indexed number representing the given letter's position in the alphabet
    /// </summary>
    /// <param name="letter">A CAPITAL letter in the alphabet A-Z</param>
    /// <returns>The distance between A and `letter` in the alphabet</returns>
    public static int GetNumberFromLetter(char letter) => letter - 'A';
    
    /// <summary>
    /// Gets the letter in the alphabet represented by the given zero-indexed number.
    /// </summary>
    /// <param name="number">A number between 0 and 25</param>
    /// <returns>The CAPITAL letter at the given position in the alphabet</returns>
    public static string GetLetterFromNumber(int number) => ((char)('A' + number)).ToString();
}