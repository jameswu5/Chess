using UnityEngine;

using System.Threading.Tasks;

public abstract class Player : MonoBehaviour
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();
    public abstract void Update();
}