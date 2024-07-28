using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Chess.UI;

public class Image
{
    private Raylib_cs.Image img;
    private Texture2D texture;

    public int width;
    public int height;

    public Image(string path, int width, int height)
    {
        this.width = width;
        this.height = height;
        
        img = LoadImage(path);
        ImageResize(ref img, width, height);
        texture = LoadTextureFromImage(img);
    }

    public void Draw(int posX, int posY)
    {
        DrawTexture(texture, posX, posY, new Color(255, 255, 255, 255));
    }
}
