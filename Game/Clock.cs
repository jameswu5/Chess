
namespace Chess.Game;

public class Clock
{
    public event System.Action<bool> ClockTimedOut;

    public Timer white;
    public Timer black;

    public int startTime;
    public int increment;

    public bool whiteToPlay;

    public Clock()
    {
        white = new Timer();
        black = new Timer();

        white.TimedOut += () => ClockTimedOut(true);
        black.TimedOut += () => ClockTimedOut(false);
    }

    public void Initialise(int startTime, int increment)
    {
        this.startTime = startTime;
        this.increment = increment;

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