namespace TextExcel3.IO;

public class HistoryManager(Spreadsheet sheet)
{
    private Spreadsheet Sheet { get; } = sheet;
    private Stack<ActionInformation?> UndoStack { get; set; } = new();
    private Stack<ActionInformation?> RedoStack { get; set; } = new();

    /// <summary>
    /// Pop the most recent action on the UndoStack and revert it
    /// </summary>
    public void Undo()
    {
        if (!UndoStack.TryPop(out ActionInformation? act)) return;
        Sheet.SetCell(act!.Cell, act.OldValue, true);
        RedoStack.Push(act);
    }

    public void Redo()
    {
        if (!RedoStack.TryPop(out ActionInformation? act)) return;
        Sheet.SetCell(act!.Cell, act.NewValue, true);
        UndoStack.Push(act);
    }

    public void RegisterAction(ActionInformation? action)
    {
        UndoStack.Push(action);
    }

}