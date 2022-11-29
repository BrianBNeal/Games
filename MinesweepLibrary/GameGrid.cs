using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MinesweepLibrary;

public class GameGrid
{
    private readonly GameCell[,] _grid;
    public int Rows { get; }
    public int Columns { get; }
    public GameCell this[int row, int col]
    {
        get => _grid[row, col];
        set => _grid[row, col] = value;
    }

    public GameGrid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _grid = new GameCell[rows, columns];
    }

    public bool IsInside(int r, int c)
    {
        return r >= 0 && r < Rows && c >= 0 && c < Columns;
    }

    public IEnumerable<GameCell> AdjacentCells(int r, int c)
    {
        for (int row = -1; row < 2; row++)
        {
            for (int col = -1; col < 2; col++)
            {
                var adjustedRow = r + row;
                var adjustedCol = c + col;

                if (!(adjustedRow == row && adjustedCol == col) && IsInside(r - row, c - col))
                {
                    yield return _grid[adjustedRow, adjustedCol];
                }
            }
        }
    }

    public void ResetGrid()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                ResetCell(r, c);
            }
        }
    }

    public void ResetCell(int r, int c)
    {
        _grid[r, c] = new GameCell(r, c, CellState.Covered, false);
    }
    //click cell
    //
}
