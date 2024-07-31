using Raylib_cs;
using static Raylib_cs.Raylib;
using static Chess.UI.Settings;

namespace Chess;

public class Program
{
    public static void Main()
    {
        // PlayOnePlayer(Player.Player.Type.Human);
        // PlayOnePlayer(Player.Player.Type.Version2);
        PlayMatch(Player.Player.Type.Random, Player.Player.Type.Version2);
    }

    public static void PlayMatch(Player.Player.Type bot1, Player.Player.Type bot2)
    {
        InitWindow(ScreenWidth, ScreenHeight, "Chess");
        SetTargetFPS(60);

        Game.Game game = new();
        game.StartMatch(bot1, bot2);

        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(ScreenColour);

            game.Update();

            EndDrawing();
        }
        Environment.Exit(0);
        CloseWindow();
    }

    public static void PlayOnePlayer(Player.Player.Type opponentType)
    {
        InitWindow(ScreenWidth, ScreenHeight, "Chess");
        SetTargetFPS(60);

        Game.Game game = new();
        game.StartNewGame(Player.Player.Type.Human, opponentType);

        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(ScreenColour);

            game.Update();

            EndDrawing();
        }
        Environment.Exit(0);
        CloseWindow();
    }
}