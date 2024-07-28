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

    public Square(int index)
    {
        this.index = index;
        x = index % 8;
        y = index / 8;

        isLight = (x + y) % 2 == 0;
        colour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
        xcoord = Settings.Board.HorOffset + x * Settings.Square.Size;
        ycoord = Settings.Board.VerOffset + y * Settings.Square.Size;
    }

    public void Display()
    {
        DrawRectangle(xcoord, ycoord, Settings.Square.Size, Settings.Square.Size, colour);
    }
}