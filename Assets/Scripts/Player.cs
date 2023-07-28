using UnityEngine;

public abstract class Player
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();
    public abstract void Update();

}