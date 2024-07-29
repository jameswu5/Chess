using Raylib_cs;

namespace Chess.Audio;

public static class Audio
{
    private static Sound MoveSound;
    private static Sound CaptureSound;

    static Audio()
    {
        Raylib.InitAudioDevice();
        MoveSound = Raylib.LoadSound("Audio/MoveSound.mp3");
        CaptureSound = Raylib.LoadSound("Audio/CaptureSound.mp3");
    }

    public static void PlaySound(bool isCapture)
    {
        Raylib.PlaySound(isCapture ? CaptureSound : MoveSound);
    }
}