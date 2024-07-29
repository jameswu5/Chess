using static Chess.UI.Settings;
using Raylib_cs;

namespace Chess.Game;

public class Timer
{
    public event Action TimedOut;

    public float secondsRemaining;
    public float secondsRemainingAtStart;
    public bool isActive;
    public bool isWhite;

    public Timer(bool isWhite)
    {
        secondsRemaining = 0;
        secondsRemainingAtStart = 0;
        isActive = false;

        this.isWhite = isWhite;
    }

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
        }

        DisplayTime();
    }

    void DisplayTime()
    {
        int minutes = (int)(secondsRemaining / 60);
        int seconds = (int)(secondsRemaining % 60);

        Color colour = isActive ? UI.Settings.Timer.ActiveColor : UI.Settings.Timer.InactiveColor;

        string text = $"{minutes:00}:{seconds:00}";
        int textWidth = Raylib.MeasureText(text, UI.Settings.Timer.FontSize);
        int posX = Board.HorOffset + Board.Size - textWidth;
        int posY = isWhite
            ? Board.VerOffset + Board.Size + UI.Settings.Timer.Padding
            : Board.VerOffset - UI.Settings.Timer.FontSize - UI.Settings.Timer.Padding;

        Raylib.DrawText(text, posX, posY, UI.Settings.Timer.FontSize, colour);
    }
}