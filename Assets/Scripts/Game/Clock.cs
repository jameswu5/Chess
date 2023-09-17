using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public Timer white;
    public Timer black;

    public int startTime;
    public int increment;

    public bool whiteToPlay;

    public void Initialise(int startTime, int increment)
    {
        this.startTime = startTime;
        this.increment = increment;
        white.Initialise(true);
        black.Initialise(false);

        NewGame();
    }

    public void NewGame()
    {
        white.SetTime(startTime);
        black.SetTime(startTime);

        whiteToPlay = true;
        white.SetActive(true);
        black.SetActive(false);
    }

    public void ToggleClock()
    {
        white.Toggle();
        black.Toggle();
        if (whiteToPlay)
        {
            white.AddTime(increment);
        }
        else
        {
            black.AddTime(increment);
        }
        whiteToPlay = !whiteToPlay;
    }
}
