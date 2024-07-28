using Raylib_cs;

namespace Chess.UI;

public static class Test
{
    public static void PlayGround()
    {
        Raylib.InitWindow(Settings.ScreenWidth, Settings.ScreenHeight, "Chess");
        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Settings.ScreenColour);

            Update();

            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }

    // This is run every frame
    private static void Update()
    {
        TestDrawBoard();
    }

    private static void TestDrawSquares()
    {
        for (int i = 0; i < 64; i++)
        {
            new Square(i).Display();
        }
    }

    private static void TestDrawBoard()
    {
        Core.Board board = new Core.Board();
        UI ui = new();
        ui.CreateUI(board.boardState);
        ui.Display();
    }
}