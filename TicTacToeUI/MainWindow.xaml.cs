using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using TicTacToeLibrary;

namespace TicTacToeUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private readonly Dictionary<Player, ImageSource> _imageSources = new()
    {
        {Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
        {Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
    };

    private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> _animations = new()
    {
        {Player.X, new ObjectAnimationUsingKeyFrames() },
        {Player.O, new ObjectAnimationUsingKeyFrames() },
    };

    private readonly DoubleAnimation _fadeOutAnimation = new()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 1,
        To = 0
    };

    private readonly DoubleAnimation _fadeInAnimation = new()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 0,
        To = 1
    };

    private readonly Image[,] _imageControls = new Image[3, 3];
    private readonly GameState _gameState = new GameState();

    public MainWindow()
    {
        InitializeComponent();
        SetupGameGrid();
        SetupAnimations();

        _gameState.MoveMade += OnMoveMade;
        _gameState.GameEnded += OnGameEnded;
        _gameState.GameRestarted += OnGameRestarted;
    }

    private void SetupGameGrid()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Image imageControl = new Image();
                GameGrid.Children.Add(imageControl);
                _imageControls[r, c] = imageControl;
            }
        }
    }

    private void SetupAnimations()
    {
        _animations[Player.X].Duration = TimeSpan.FromSeconds(.25);
        _animations[Player.O].Duration = TimeSpan.FromSeconds(.25);

        for (int i = 0; i < 16; i++)
        {
            Uri xUri = new Uri($"pack://application:,,,/Assets/X{i}.png");
            BitmapImage xImg = new BitmapImage(xUri);
            DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
            _animations[Player.X].KeyFrames.Add(xKeyFrame);

            Uri oUri = new Uri($"pack://application:,,,/Assets/O{i}.png");
            BitmapImage oImg = new BitmapImage(oUri);
            DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
            _animations[Player.O].KeyFrames.Add(oKeyFrame);
        }
    }

    private async Task WaitForAnimation(UIElement uIElement, DependencyProperty property, AnimationTimeline animation)
    {
        uIElement.BeginAnimation(property, animation);
        await Task.Delay(animation.Duration.TimeSpan);
    }

    private async Task FadeOut(UIElement uIElement)
    {
        await WaitForAnimation(uIElement, OpacityProperty, _fadeOutAnimation);
        uIElement.Visibility = Visibility.Hidden;
    }

    private async Task FadeIn(UIElement uIElement)
    {
        uIElement.Visibility = Visibility.Visible;
        await WaitForAnimation(uIElement, OpacityProperty, _fadeInAnimation);
    }

    private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
    {
        await Task.Delay(1000);
        await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas), FadeOut(Line));
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        await Task.WhenAll(FadeIn(EndScreen));
    }

    private async Task TransitionToGameScreen()
    {
        await Task.Delay(1000);
        await Task.WhenAll(FadeOut(EndScreen));
        await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
    }

    private (Point, Point) FindLinePoints(WinInfo winInfo)
    {
        double squareSize = GameGrid.Width / 3;
        double margin = squareSize / 2;

        switch (winInfo.Type)
        {
            case WinType.Row:
                double y = winInfo.Number * squareSize + margin;
                return (new Point(0, y), new Point(GameGrid.Width, y));

            case WinType.Column:
                double x = winInfo.Number * squareSize + margin;
                return (new Point(x, 0), new Point(x, GameGrid.Width));

            case WinType.MainDiagonal:
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));

            default:
                return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));

        }
    }

    private void ResetLine()
    {
        Line.Visibility = Visibility.Hidden;
        Line.Opacity = 1;
        Line.BeginAnimation(Line.X2Property, null);
        Line.X1 = 0;
        Line.X2 = 0;
        Line.BeginAnimation(Line.Y2Property, null);
        Line.Y1 = 0;
        Line.Y2 = 0;
    }

    private async Task ShowLine(WinInfo winInfo)
    {
        ResetLine();
        await Task.Delay(1000);

        (Point start, Point end) = FindLinePoints(winInfo);

        Line.X1 = start.X;
        Line.Y1 = start.Y;
        Line.X2 = start.X;
        Line.Y2 = start.Y;
        Line.Visibility = Visibility.Visible;
        
        double duration = .25;

        DoubleAnimation xDrawAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(duration),
            From = start.X,
            To = end.X
        };

        DoubleAnimation yDrawAnimation = new()
        {
            Duration = TimeSpan.FromSeconds(duration),
            From = start.Y,
            To = end.Y
        };

        await Task.WhenAll(WaitForAnimation(Line, Line.X2Property, xDrawAnimation), WaitForAnimation(Line,Line.Y2Property, yDrawAnimation));
        await Task.Delay(500);
    }

    private async Task OnMoveMade(int r, int c)
    {
        Player player = _gameState.GameGrid[r, c];
        await WaitForAnimation(_imageControls[r, c], Image.SourceProperty, _animations[player]);
    }

    private async Task OnGameEnded(GameResult gameResult)
    {
        if (gameResult.Winner == Player.None)
        {
            await TransitionToEndScreen("It's a tie!", null);
        }
        else
        {
            await ShowLine(gameResult.WinInfo);
            await TransitionToEndScreen("Winner:", _imageSources[gameResult.Winner]);
        }
    }

    private async void OnGameRestarted()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                _imageControls[r,c].BeginAnimation(Image.SourceProperty, null);
                _imageControls[r, c].Source = null;
            }
        }

        PlayerImage.Source = _imageSources[_gameState.CurrentPlayer];
        await TransitionToGameScreen();
    }

    private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        double squareSize = GameGrid.Width / 3;
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squareSize);
        int col = (int)(clickPosition.X / squareSize);
        _gameState.MakeMove(row, col);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (_gameState.GameOver)
        {
            _gameState.Reset();
        }
    }
}
