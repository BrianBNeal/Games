using System;
using System.CodeDom;
using System.Collections.Generic;

namespace SnakeLibrary;

public class GameState
{
    public int Rows { get; }
    public int Cols { get; }
    public GridValue[,] Grid { get; }
    public Direction Dir { get; private set; }
    public int Score { get; private set; }
    public bool GameOver { get; private set; }

    private readonly LinkedList<Position> _snakePositions = new LinkedList<Position>();
    private readonly Random _rand = new Random();

    public GameState(int rows, int cols)
    {
        Rows= rows;
        Cols= cols;
        Grid = new GridValue[rows, cols];
        Dir = Direction.Right;

        AddSnake();
        AddFood();
    }

    private void AddSnake()
    {
        int r = Rows / 2;
        for (int c = 1; c <= 3; c++)
        {
            Grid[r, c] = GridValue.Snake;
            _snakePositions.AddFirst(new Position(r, c));
        }
    }

    private IEnumerable<Position> EmptyPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[r,c] == GridValue.Empty)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    private void AddFood()
    {
        List<Position> empty = new List<Position>(EmptyPositions());

        if (empty.Count == 0)
        {
            return;
        }

        Position pos = empty[_rand.Next(empty.Count)];
        Grid[pos.Row, pos.Col] = GridValue.Food;
    }

    public Position HeadPosition()
    {
        return _snakePositions.First.Value;
    }

    public Position TailPosition()
    {
        return _snakePositions.Last.Value;
    }

    public IEnumerable<Position> SnakePositions()
    {
        return _snakePositions;
    }

    private void AddHead(Position pos)
    {
        _snakePositions.AddFirst(pos);
        Grid[pos.Row, pos.Col] = GridValue.Snake;
    }

    private void RemoveTail()
    {
        Position tail = TailPosition();
        Grid[tail.Row, tail.Col] = GridValue.Empty;
        _snakePositions.RemoveLast();
    }

    public void ChangeDirection(Direction dir)
    {
        Dir = dir;
    }

    private bool OutsideGrid(Position pos)
    {
        return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col>= Cols;
    }

    private GridValue WillHit(Position newHeadPos)
    {
        if (OutsideGrid(newHeadPos))
        {
            return GridValue.Outside;
        }

        if (newHeadPos == TailPosition())
        {
            return GridValue.Empty;
        }

        return Grid[newHeadPos.Row, newHeadPos.Col];
    }

    public void Move()
    {
        Position newHeadPos = HeadPosition().Translate(Dir);
        GridValue hit = WillHit(newHeadPos);

        if (hit == GridValue.Outside || hit == GridValue.Snake)
        {
            GameOver = true;
        }
        else if (hit == GridValue.Empty)
        {
            RemoveTail();
            AddHead(newHeadPos);
        }
        else if (hit == GridValue.Food)
        {
            AddHead(newHeadPos);
            Score++;
            AddFood();
        }
    }
}
