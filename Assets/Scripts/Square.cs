using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public Color lightColor, darkColor;
    public SpriteRenderer renderer;
    // Start is called before the first frame update
    

    public void SetColor(bool isOffset) {
        renderer.color = isOffset ? lightColor : darkColor;
    }

    
}
