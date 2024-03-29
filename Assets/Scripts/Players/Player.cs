using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public enum Type { Human, Random, Version1, Version2 };

    public event System.Action<int> PlayChosenMove;

    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();

    public abstract void TurnToMove(Timer timer);

    public abstract void Update();

    public abstract override string ToString();

    public void Decided(int move)
    {
        PlayChosenMove.Invoke(move);
    }
}