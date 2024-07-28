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
        (x, y) = GetCoordsFromIndex(index);

        isLight = (x + y) % 2 == 0;
        colour = isLight ? Settings.Square.LightColour : Settings.Square.DarkColour;
        (xcoord, ycoord) = GetSquareDisplayCoords(index);
    }

    public void Display()
    {
        DrawRectangle(xcoord, ycoord, Settings.Square.Size, Settings.Square.Size, colour);
    }

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