using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public Board board = GameObject.FindGameObjectWithTag("BoardObject").GetComponent<Board>();

    public void Start()
    {
        Debug.Log("called");

    }

    public abstract void Update();
}