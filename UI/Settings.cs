using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Chess.UI;

public static class Settings
{
    public const int ScreenWidth = 1080;
    public const int ScreenHeight = 720;
    public static readonly Color ScreenColour = new(7, 51, 59, 255);

    public static class Square
    {
        public const int Size = 76;

        public static readonly Color LightColour = new(200, 227, 232, 255);
        public static readonly Color DarkColour  = new(86, 126, 133, 255);
        public static readonly Color HighlightColour = new(240, 235, 204, 255);
        public static readonly Color CheckColour = new(237, 159, 145, 255);
    }

    public static class Board
    {
        public const int HorOffset = ScreenWidth - (Square.Size << 3) >> 1;
        public const int VerOffset = ScreenHeight - (Square.Size << 3) >> 1;
    }
}