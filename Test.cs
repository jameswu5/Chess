using Raylib_cs;
using static Raylib_cs.Raylib;
using static Chess.UI.Settings;

namespace Chess;

public static class Test
{

    public static void PlayGround()
    {
        InitWindow(ScreenWidth, ScreenHeight, "Chess");
        SetTargetFPS(60);

        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(ScreenColour);

            Update();

            EndDrawing();
        }
        CloseWindow();
    }

    // This is run every frame
    private static void Update()
    {

    }

    private static void TestDrawSquares()
    {
        for (int i = 0; i < 64; i++)
        {
            new UI.Square(i).Display();
        }
    }

    private static void TestDrawBoard()
    {
        Core.Board board = new();
        UI.UI ui = new();
        ui.CreateUI(board.boardState);
        ui.Display();
    }

    private static void TestMouseInput()
    {
        if (IsMouseButtonDown(0))
        {
            Console.WriteLine(Player.Human.GetMouseIndex());
        }
    }

    public static void TestGame()
    {
        InitWindow(ScreenWidth, ScreenHeight, "Chess");
        SetTargetFPS(60);

        Game game = new();

        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(ScreenColour);

            game.Update();

            EndDrawing();
        }
        CloseWindow();
    }
}