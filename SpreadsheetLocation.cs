using System.Text.RegularExpressions;

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
        MatchCollection matches = Regex.Matches(id.ToUpper(), "[A-Z]");
        string letters = "";
        foreach (Match m in matches) letters += m.Value;
        //foreach (string m in matches) letters += m;
        Column = GetNumberFromLetter(letters);
        Row = int.Parse(id[(letters.Length)..]) - 1;
        //Column = GetNumberFromLetter(id[0]);
        //Row = int.Parse(id[1..]);
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
    /// <param name="letters"></param>
    /// <returns>The distance between A and `letter` in the alphabet</returns>
    private static int GetNumberFromLetter(string letters)
    {
        // Z: 26; AA: 27; AZ: 52; 
        //return letter - 'A';
        // starts from the right and moves left
        // each consecutive place value is worth 26 * distanceFromLeft
        // basically, base-26 numerals

        return letters.Aggregate(0, (current, t) => current * (26 + (t - 'A' + 1)));
    }

    /// <summary>
    /// Gets the letter in the alphabet represented by the given zero-indexed number.
    /// </summary>
    /// <param name="number">A number between 0 and 25</param>
    /// <returns>The CAPITAL letter at the given position in the alphabet</returns>
    public static string GetLetterFromNumber(int number)
    {
        number++;
        //return ((char)('A' + number)).ToString();
        //if (number < 1)
        //    throw new ArgumentException("Number must be 1 or greater.");

        string columnName = "";
    
        while (number > 0)
        {
            number--; // Adjust for 1-based indexing
            char letter = (char)('A' + (number % 26));
            columnName = letter + columnName;
            number /= 26;
        }

        return columnName;
    }
    
    public override string ToString()
    {
        return FriendlyName;
    }
}