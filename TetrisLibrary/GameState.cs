﻿using System;
using System.Collections.Generic;

namespace TetrisLibrary;

public class GameState
{
    private Block _currentBlock;

    public Block CurrentBlock
    {
        get => _currentBlock;
        private set
        {
            _currentBlock = value;
            _currentBlock.Reset();

            for (int i = 0; i < 2; i++)
            {
                _currentBlock.Move(1, 0);

                if (!BlockFits())
                {
                    _currentBlock.Move(-1, 0);
                }
            }
        }
    }

    public GameGrid GameGrid { get; }
    public BlockQueue BlockQueue { get; }
    public bool GameOver { get; private set; }
    public int Score { get; private set; }
    public Block HeldBlock { get; private set; }
    public bool CanHold { get; private set; }

    public GameState()
    {
        GameGrid = new(22, 10);
        BlockQueue = new BlockQueue();
        CurrentBlock = BlockQueue.GetAndUpdate();
        CanHold = true;
    }

    private bool BlockFits()
    {
        foreach (Position p in CurrentBlock.TilePositions())
        {
            if (!GameGrid.IsEmpty(p.Row, p.Column))
            {
                return false;
            }
        }

        return true;
    }

    public void HoldBlock()
    {
        if (!CanHold)
        {
            return;
        }

        if (HeldBlock == null)
        {
            HeldBlock = CurrentBlock;
            CurrentBlock = BlockQueue.GetAndUpdate();
        }
        else
        {
            Block tmp = CurrentBlock;
            CurrentBlock = HeldBlock;
            HeldBlock = tmp;
        }

        CanHold = false;
    }

    public void RotateBlockCW()
    {
        CurrentBlock.RotateClockwise();

        if (!BlockFits())
        {
            CurrentBlock.RotateCounterClockwise();
        }
    }

    public void RotateBlockCounterCW()
    {
        CurrentBlock.RotateCounterClockwise();

        if (!BlockFits())
        {
            CurrentBlock.RotateClockwise();
        }
    }

    public void MoveBlockLeft()
    {
        CurrentBlock.Move(0, -1);

        if (!BlockFits())
        {
            CurrentBlock.Move(0, 1);
        }
    }

    public void MoveBlockRight()
    {
        CurrentBlock.Move(0, 1);

        if (!BlockFits())
        {
            CurrentBlock.Move(0, -1);
        }
    }

    private bool IsGameOver()
    {
        return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
    }

    private void PlaceBlock()
    {
        foreach (Position p in CurrentBlock.TilePositions())
        {
            GameGrid[p.Row, p.Column] = CurrentBlock.Id;
        }

        int clearedRows = GameGrid.ClearFullRows();
        Score += (clearedRows * 10) + clearedRows;
        //Score += _scoringTable[GameGrid.ClearFullRows()]; ;
        

        if (IsGameOver())
        {
            GameOver = true;
        }
        else
        {
            CurrentBlock = BlockQueue.GetAndUpdate();
            CanHold = true;
        }
    }

    public void MoveBlockDown()
    {
        CurrentBlock.Move(1, 0);

        if (!BlockFits())
        {
            CurrentBlock.Move(-1, 0);
            PlaceBlock();
        }
    }

    private int TileDropDistance(Position position)
    {
        int drop = 0;

        while (GameGrid.IsEmpty(position.Row + drop + 1, position.Column))
        {
            drop++;
        }

        return drop;
    }

    public int BlockDropDistance()
    {
        int distance = GameGrid.Rows;

        foreach (Position p in CurrentBlock.TilePositions())
        {
            distance = Math.Min(distance, TileDropDistance(p));
        }

        return distance;
    }

    public void DropBlock()
    {
        CurrentBlock.Move(BlockDropDistance(), 0);
        PlaceBlock();
    }

}
