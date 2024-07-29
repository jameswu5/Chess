using static Chess.UI.Settings;
using Raylib_cs;

namespace Chess.Game;

public class Timer
{
    public event Action TimedOut;

    public double secondsRemaining;
    public double secondsRemainingAtStart;
    public bool isActive;
    public bool isWhite;

    public Timer(bool isWhite)
    {
        secondsRemaining = 0;
        secondsRemainingAtStart = 0;
        isActive = false;

        this.isWhite = isWhite;
    }

    public double TimeElapsedThisTurn => secondsRemainingAtStart - secondsRemaining;

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

    private void DisplayTime()
    {
        int minutes = (int)(secondsRemaining / 60);
        int seconds = (int)(secondsRemaining % 60);

        Color colour = isActive ? Board.ActiveColor : Board.InactiveColor;

        string text = $"{minutes:00}:{seconds:00}";
        int textWidth = Raylib.MeasureText(text, Board.FontSize);
        int posX = Board.HorOffset + Board.Size - textWidth;
        int posY = isWhite
            ? Board.VerOffset + Board.Size + Board.Padding
            : Board.VerOffset - Board.FontSize - Board.Padding;

        Raylib.DrawText(text, posX, posY, Board.FontSize, colour);
    }
}