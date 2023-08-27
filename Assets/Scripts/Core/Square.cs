using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public Color lightColor, darkColor, highlightColor, checkColor;

    public GameObject hoverHighlight;
    public GameObject optionHighlight;

    public new SpriteRenderer renderer;

    private bool isLightSquare;

    public const int a1 = 0;
    public const int b1 = 1;
    public const int c1 = 2;
    public const int d1 = 3;
    public const int e1 = 4;
    public const int f1 = 5;
    public const int g1 = 6;
    public const int h1 = 7;

    public const int a8 = 56;
    public const int b8 = 57;
    public const int c8 = 58;
    public const int d8 = 59;
    public const int e8 = 60;
    public const int f8 = 61;
    public const int g8 = 62;
    public const int h8 = 63;

    public void Initialise(string name, bool isLightSquare)
    {
        this.name = name;
        this.isLightSquare = isLightSquare;
        InitialiseColor();
    }

    public void InitialiseColor()
    {
        if (isLightSquare == true)
        {
            SetColor(lightColor);
        }
        else
        {
            SetColor(darkColor);
        }
    }

    public void Highlight() => SetColor(highlightColor);

    public void HighlightCheck() => SetColor(checkColor);

    private void SetColor(Color color) => renderer.color = color;

    public void SetHoverHighlight(bool value) => hoverHighlight.SetActive(value);

    public void SetOptionHighlight(bool value) => optionHighlight.SetActive(value);

    public void DestroySquare() => Destroy(gameObject);

    public static string ConvertIndexToSquareName(int index)
    {
        int rank = (index >> 3) + 1;
        return GetFileName(index) + rank.ToString();
    }

    public static string GetFileName(int index)
    {
        int file = GetFile(index);
        string[] letters = { "#", "a", "b", "c", "d", "e", "f", "g", "h" };
        return letters[file];
    }

    public static int GetFile(int index) => (index & 0b111) + 1;

    public static int GetRank(int index) => (index >> 3) + 1;

    public static int GetIndexFromSquareName(string name)
    {
        int index = 0;

        foreach (char c in name)
        {
            if ("abcdefgh".Contains(c))
            {
                index += c - 'a';
            }
            else
            {
                index += (c - '1') * 8;
            }
        }

        return index;
    }

    
}
