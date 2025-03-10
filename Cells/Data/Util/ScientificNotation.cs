/*namespace TextExcel3.Cells.Data.Util;

/// <summary>
/// Class to hold value for a number expressed in scientific notation
/// </summary>
public class ScientificNotation(decimal scalar, int power)
{
    public decimal Scalar { get; set; } = scalar;
    public int Power { get; set; } = power;

    public override string ToString()
    {
        return "" + Scalar + " \u00d7 10^" + Power;
    }

    /// <summary>
    /// ToString, but digits in the scalar are clipped to meet the width limit. If numbers are clipped, `…` replaced the last digit of the scalar.
    /// </summary>
    /// <param name="width">The maximum allowed width for this number in scientific notation</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Thrown when the number cannot be written in scientific notation inside of the given width.</exception>
    public string ToStringSizeLimited(int width)
    {
        string end = " \u00d7 10^" + Power;
        int maxScalarLength = width - end.Length;
        if (maxScalarLength <)
    }
}*/