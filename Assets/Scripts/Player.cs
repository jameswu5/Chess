using UnityEngine;

using System.Threading.Tasks;

public abstract class Player
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();
    public abstract Task Update();

}