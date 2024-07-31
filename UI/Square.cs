using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Chess.UI;

public class Square
{
    private readonly int index;
    private readonly int x;
    private readonly int y;

    private readonly bool isLight;
    private Color defaultColour;
    private Color colour;
    private readonly int xcoord;
    private readonly int ycoord;

    private bool optionHighlight;
    private readonly int optionXPos;
    private readonly int optionYPos;

    private bool isHovered;

    public Square(int index)
    {
        this.index = index;
        (x, y) = GetCoordsFromIndex(index);

        isLight = (x + y) % 2 == 0;
        defaultColour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
        colour = defaultColour;
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

    public void SetColour(Color colour) => this.colour = colour;

    public void InitialiseColor()
    {
        colour = defaultColour;
    }

    public void SetDefaultColour(Color? colour)
    {
        // if blank, then set to board colours
        if (colour == null)
        {
            defaultColour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
        }
        else
        {
            defaultColour = (Color)colour;
        }
    }

    public void Highlight() => SetColour(Settings.Square.HighlightColour);

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