using System.Collections.Generic;

namespace TetrisLibrary;

public abstract class Block
{
    protected abstract Position[][] Tiles { get; }
    protected abstract Position StartOffset { get; }
    public abstract int Id { get; }

    private int _rotationState;
    private Position _offset;

    public Block()
    {
        _offset = new Position(StartOffset.Row, StartOffset.Column);
    }

    public IEnumerable<Position> TilePositions()
    {
        foreach (Position position in Tiles[_rotationState])
        {
            yield return new Position(position.Row + _offset.Row, position.Column + _offset.Column);
        }
    }

    public void RotateClockwise()
    {
        _rotationState = (_rotationState + 1) % Tiles.Length;
    }

    public void RotateCounterClockwise()
    {
        if (_rotationState == 0)
        {
            _rotationState = Tiles.Length - 1;
        }
        else
        {
            _rotationState--;
        }
    }

    public void Move(int rows, int columns)
    {
        _offset.Row += rows;
        _offset.Column += columns;
    }

    public void Reset()
    {
        _rotationState = 0;
        _offset.Row = StartOffset.Row;
        _offset.Column = StartOffset.Column;
    }
}
