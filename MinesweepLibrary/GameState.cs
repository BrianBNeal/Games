namespace MinesweepLibrary;

public class GameState
{
    public GameGrid GameGrid { get; }
    public bool GameOver { get; private set; }
    public int MineTotal { get; private set; }
    public int MinesLeft { get; private set; }

    public GameState()
    {
        GameGrid = new(8,8);
        GameOver = false;
    }
}
