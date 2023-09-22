using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public event System.Action<int> PlayChosenMove;

    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();

    public abstract void TurnToMove();

    public abstract void Update();

    public void Decided(int move)
    {
        PlayChosenMove.Invoke(move);
    }
}