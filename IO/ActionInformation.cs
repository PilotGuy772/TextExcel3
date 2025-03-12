using TextExcel3.Cells;

namespace TextExcel3.IO;

public record ActionInformation(SpreadsheetLocation Cell, ICell OldValue, ICell NewValue);