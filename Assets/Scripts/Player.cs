using UnityEngine;

using System.Threading.Tasks;

public abstract class Player : MonoBehaviour
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();
    public UI boardUI;

    public void Start()
    {
        boardUI = board.boardUI;
    }

    public abstract void Update();
}