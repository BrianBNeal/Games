namespace MinesweepLibrary;

public class GameCell
{
    public CellState State { get; private set; }
    public bool Trapped { get; private set; }
    public bool Covered => State == CellState.Covered;

    public GameCell(CellState cellState, bool trapped = false)
    {
        State = cellState;
        Trapped = trapped ;
    }

    public void Reveal()
    {
        if (!Covered) return;

        State = CellState.Revealed;
    }

    public void Trap()
    {
        Trapped = true;
    }

    public void NextTag()
    {
        switch (State)
        {
            case CellState.Covered:
                State = CellState.Flagged;
                return;
            case CellState.Flagged:
                State = CellState.Questionable;
                return;
            case CellState.Questionable:
                State = CellState.Covered;
                return;
            default:
                return;
        }
    }
}