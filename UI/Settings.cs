using Raylib_cs;

namespace Chess.UI;

public static class Settings
{
    public const int ScreenWidth = 1080;
    public const int ScreenHeight = 720;
    public static readonly Color ScreenColour = new(29, 36, 43, 255);

    public const int FrameRate = 60;

    public static class Square
    {
        public const int Size = 76;

        public static readonly Color LightColour  = new(193, 208, 214, 255);
        public static readonly Color DarkColour = new(86, 119, 140, 255);
        public static readonly Color HighlightColour = new(240, 235, 204, 255);
        public static readonly Color CheckColour = new(214, 127, 111, 255);

        public static readonly Color OptionColour = new(0, 0, 0, 100);
        public const int OptionRadius = Size / 4;

        public static readonly Color HoverColour = new(255, 255, 255, 150);
    }

    public static class Board
    {
        public const int HorOffset = ScreenWidth - (Square.Size << 3) >> 1;
        public const int VerOffset = ScreenHeight - (Square.Size << 3) >> 1;
        public const int Size = Square.Size << 3;
        public static readonly Color CoverColour = new(0, 0, 0, 150);

        public const int FontSize = 30;
        public const int Padding = 10;
        public static readonly Color ActiveColor   = new(255, 255, 255, 255);
        public static readonly Color InactiveColor = new(170, 170, 170, 255);
    }
}