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
    private int squareIndex;


    public void Initialise(int squareIndex, string name, bool isLightSquare)
    {
        this.squareIndex = squareIndex;
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

    public void Highlight()
    {
        SetColor(highlightColor);
    }

    public void HighlightCheck()
    {
        SetColor(checkColor);
    }

    public void SetColor(Color color) {
        renderer.color = color;
    }

    public void SetHoverHighlight(bool value)
    {
        hoverHighlight.SetActive(value);
    }

    public void SetOptionHighlight(bool value)
    {
        optionHighlight.SetActive(value);
    }

    public void DestroySquare()
    {
        Destroy(gameObject);
    }



    public static string ConvertIndexToSquareName(int index)
    {
        int rank = (index / 8) + 1;
        return GetFileName(index) + rank.ToString();
    }

    public static string GetFileName(int index)
    {
        int file = (index % 8) + 1;
        string[] letters = { "#", "a", "b", "c", "d", "e", "f", "g", "h" };
        return letters[file];
    }


}
