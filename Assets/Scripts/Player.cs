using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();

    public abstract void Update();
}