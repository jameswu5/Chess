
using Chess.Core;

namespace Chess.Player;

public abstract class Player
{
    public enum Type { Human };

    public event System.Action<int> PlayChosenMove;

    public Game game;
    public Board board;

    // public abstract void TurnToMove(Timer timer);

    public abstract void Update();

    public abstract override string ToString();

    public void Decided(int move)
    {
        Console.WriteLine(move);
        PlayChosenMove.Invoke(move);
    }
}