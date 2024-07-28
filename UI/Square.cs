using Raylib_cs;
using static Raylib_cs.Raylib;
using static Chess.UI.Settings;

namespace Chess.UI;

public class Square
{
    public int index;
    public int x;
    public int y;

    private bool isLight;
    private Color colour;
    private int xcoord;
    private int ycoord;

    private bool optionHighlight;
    private int optionXPos;
    private int optionYPos;

    private bool isHovered;

    public Square(int index)
    {
        this.index = index;
        (x, y) = GetCoordsFromIndex(index);

        isLight = (x + y) % 2 == 0;
        colour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
        (xcoord, ycoord) = GetSquareDisplayCoords(index);

        optionXPos = xcoord + Settings.Square.Size / 2;
        optionYPos = ycoord + Settings.Square.Size / 2;

        isHovered = false;
    }

    public void Display()
    {
        DrawRectangle(xcoord, ycoord, Settings.Square.Size, Settings.Square.Size, colour);
        if (optionHighlight)
        {
            DrawCircle(optionXPos, optionYPos, Settings.Square.OptionRadius, Settings.Square.OptionColour);
        }
        if (isHovered)
        {
            DrawRectangle(xcoord, ycoord, Settings.Square.Size, Settings.Square.Size, Settings.Square.HoverColour);
        }
    }

    private void SetColor(Color colour) => this.colour = colour;

    public void InitialiseColor()
    {
        colour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
    }

    public void Highlight() => SetColor(Settings.Square.HighlightColour);

    public void SetOptionHighlight(bool highlight) => optionHighlight = highlight;

    public void SetHoverHighlight(bool value) => isHovered = value;

    public static (int, int) GetSquareDisplayCoords(int index)
    {
        int x = index % 8;
        int y = index / 8;
        int xcoord = Settings.Board.HorOffset + x * Settings.Square.Size;
        int ycoord = Settings.Board.VerOffset + (7 - y) * Settings.Square.Size;

        return (xcoord, ycoord);
    }

    public static int GetIndexFromCoords(int x, int y) => y * 8 + x;

    public static (int, int) GetCoordsFromIndex(int index) => (index % 8, index / 8);
}