using Raylib_cs;

namespace Chess.Game;

public class Timer
{
    public event System.Action TimedOut;

    public static readonly Color ActiveColor = new(255, 255, 255, 255);
    public static readonly Color InactiveColor = new(170, 170, 170, 255);

    public float secondsRemaining;
    public float secondsRemainingAtStart;
    public bool isActive;

    public float TimeElapsedThisTurn => secondsRemainingAtStart - secondsRemaining;

    public void SetActive(bool isActive) => this.isActive = isActive;

    public void Toggle() => isActive = !isActive;

    public void SetTime(int seconds) => secondsRemaining = seconds;

    public void AddTime(float increment) => secondsRemaining += increment;

    public void Update()
    {
        if (isActive)
        {
            secondsRemaining = Math.Max(0, secondsRemaining - Raylib.GetFrameTime());

            if (secondsRemaining == 0)
            {
                isActive = false;
                TimedOut.Invoke();
            }

            // timerText.color = ActiveColor;
        }
        else
        {
            // timerText.color = InactiveColor;
        }

        DisplayTime();
    }

    void DisplayTime()
    {
        int minutes = (int)(secondsRemaining / 60);
        int seconds = (int)(secondsRemaining % 60);
        // timerText.text = $"{minutes:00}:{seconds:00}";
    }
}