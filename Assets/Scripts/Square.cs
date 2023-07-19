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
}
