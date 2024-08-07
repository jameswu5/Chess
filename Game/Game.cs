
using Chess.Core;
using Chess.Player;

namespace Chess.Game;

public class Game
{
    public Core.Board board;
    public UI.UI ui;

    public Player.Player whitePlayer;
    public Player.Player blackPlayer;

    public Clock clock;
    public const int allowedTime = 300;
    public const int increment = 0;

    public Match match;

    public Game()
    {
        board = new();
        ui = new(board);
        ui.CreateUI(board.boardState);

        whitePlayer = new Player.Human(this);
        blackPlayer = new Player.Human(this);

        CreatePlayer(ref whitePlayer, Player.Player.Type.Human);
        CreatePlayer(ref blackPlayer, Player.Player.Type.Human);

        clock = new();
        clock.Initialise(allowedTime, increment, board.turn == Core.Piece.White);
        clock.ClockTimedOut += TimedOut;

        match = new Match();
        match.StartGame += StartMatchGame;
    }

    public void Update()
    {
        // Display stuff
        Display();

        if (board.gameResult != Core.Judge.Result.Playing) return;

        whitePlayer.Update();
        blackPlayer.Update();

        clock.Update();
    }

    public void Display()
    {
        ui.Display(GetSelectedPieceIndex());
        if (match.isActive)
        {
            whitePlayer.DisplayName(true, match.GetScore(true));
            blackPlayer.DisplayName(false, match.GetScore(false));
        }
        else
        {
            whitePlayer.DisplayName(true);
            blackPlayer.DisplayName(false);
        }
        clock.Display();
    }

    private int GetSelectedPieceIndex()
    {
        if (whitePlayer is Human & whitePlayer.isActive)
        {
            Human human = (Human)whitePlayer;
            if (human.currentState != Human.InputState.Idle)
            {
                return human.pieceIndex;
            }
        }

        if (blackPlayer is Human & blackPlayer.isActive)
        {
            Human human = (Human)blackPlayer;
            if (human.currentState != Human.InputState.Idle)
            {
                return human.pieceIndex;
            }
        }

        return -1;
    }

    private void CreatePlayer(ref Player.Player player, Player.Player.Type type)
    {
        // need to change when bots are added
        if (type == Player.Player.Type.Human)
        {
            player = new Player.Human(this);
        }
        else
        {
            player = Player.Bot.GetBotFromBotType(type, this);
        }

        player.PlayChosenMove += PlayMove;
    }

    public void PlayMove(int move)
    {
        // if in check, deactivate highlight
        if (board.inCheck)
        {
            int index = board.kingIndices[board.GetColourIndex(board.turn)];
            ui.SetSquareDefaultColour(index);
            ui.ResetSquareColour(index);
        }

        board.MakeMove(move);
        ui.MakeMove(move);

        // if in check activate highlight
        if (board.inCheck)
        {
            int index = board.kingIndices[board.GetColourIndex(board.turn)];
            ui.SetSquareDefaultColour(index, UI.Settings.Square.CheckColour);
            ui.ResetSquareColour(index);
        }

        Audio.Audio.PlaySound(Core.Move.IsCaptureMove(move));

        clock.ToggleClock();
        board.gameResult = Core.Judge.GetResult(board);

        if (board.gameResult != Core.Judge.Result.Playing)
        {
            HandleGameOver();
            return;
        }

        if (board.turn == Core.Piece.White)
        {
            whitePlayer.TurnToMove(clock.white);
        }
        else
        {
            blackPlayer.TurnToMove(clock.black);
        }
    }

    public void TimedOut(bool isWhite)
    {
        board.gameResult = isWhite ? Core.Judge.Result.WhiteOutOfTime : Core.Judge.Result.BlackOutOfTime;
        HandleGameOver();
    }

    public void HandleGameOver()
    {
        if (match.isActive)
        {
            if (board.gameResult == Core.Judge.Result.BlackIsMated || board.gameResult == Core.Judge.Result.BlackOutOfTime)
            {
                match.ReportResult(Match.GameResult.WhiteWins);
            }
            else if (board.gameResult == Core.Judge.Result.WhiteIsMated || board.gameResult == Core.Judge.Result.WhiteOutOfTime)
            {
                match.ReportResult(Match.GameResult.BlackWins);
            }
            else
            {
                match.ReportResult(Match.GameResult.Draw);
            }
        }
        else
        {
            clock.StopClocks();
        }
    }

    // Match games

    public void StartNewGame(Player.Player.Type whitePlayerType, Player.Player.Type blackPlayerType, string FEN = FEN.standard)
    {
        Console.WriteLine($"{whitePlayerType} vs {blackPlayerType}");
        board.Initialise(FEN);
        ui.Reset();
        ui.CreateUI(board.boardState);

        clock.NewGame(board.turn == Core.Piece.White);

        CreatePlayer(ref whitePlayer, whitePlayerType);
        CreatePlayer(ref blackPlayer, blackPlayerType);

        if (board.turn == Core.Piece.White)
        {
            whitePlayer.TurnToMove(clock.white);
        }
        else
        {
            blackPlayer.TurnToMove(clock.black);
        }
    }

    // Starts a match between two bots
    public void StartMatch(Player.Player.Type bot1, Player.Player.Type bot2)
    {
        match.SetBots(bot1, bot2);
        match.StartMatch();
    }

    public void StartMatchGame(string FEN = FEN.standard)
    {
        // Player1 is white if gameNumber is even
        if (match.gameNumber % 2 == 0)
        {
            StartNewGame(match.player1, match.player2, FEN);
        }
        else
        {
            StartNewGame(match.player2, match.player1, FEN);
        }
    }
}