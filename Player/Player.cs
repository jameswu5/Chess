using Raylib_cs;

using Chess.Core;

namespace Chess.Player;

public abstract class Player
{
    public enum Type { Human, Random, Version1, Version2 };

    public event Action<int> PlayChosenMove;

    public Game.Game game;
    public Board board;

    public bool isActive;

    public abstract void TurnToMove(Game.Timer timer);

    public abstract void Update();

    public abstract override string ToString();

    public void Decided(int move)
    {
        isActive = false;
        PlayChosenMove.Invoke(move);
    }

    public void DisplayName(bool isWhite)
    {
        int posX = UI.Settings.Board.HorOffset;
        int posY = isWhite
            ? UI.Settings.Board.VerOffset + UI.Settings.Board.Size + UI.Settings.Board.Padding
            : UI.Settings.Board.VerOffset - UI.Settings.Board.FontSize - UI.Settings.Board.Padding;

        Raylib.DrawText(ToString(), posX, posY, UI.Settings.Board.FontSize, UI.Settings.Board.ActiveColor);
    }
}