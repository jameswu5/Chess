using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public Color lightColor, darkColor, highlightColor;
    public new SpriteRenderer renderer;

    private bool isLightSquare;
    private int squareIndex;


    public void Initialise(int index, string squareName, bool isOffset)
    {
        squareIndex = index;
        name = squareName;
        isLightSquare = isOffset;
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


    public void SetColor(Color color) {
        renderer.color = color;
    }

    
}
