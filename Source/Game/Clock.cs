using System.Collections;
using System.Collections.Generic;

public class Clock
{
    public event System.Action<bool> ClockTimedOut;

    public Timer white;
    public Timer black;

    public int startTime;
    public int increment;

    public bool whiteToPlay;

    public void Start()
    {
        white.TimedOut += () => ClockTimedOut(true);
        black.TimedOut += () => ClockTimedOut(false);
    }

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

    public void StopClocks()
    {
        white.SetActive(false);
        black.SetActive(false);
    }
}
