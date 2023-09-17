using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Color ActiveColor;
    public Color InactiveColor;

    public Text timerText;

    public float secondsRemaining;
    public bool isActive;

    public void SetActive(bool isActive) => this.isActive = isActive;

    public void Toggle() => isActive = !isActive;

    public void SetTime(int seconds)
    {
        Debug.Log($"Called with {seconds}");
        secondsRemaining = seconds;
    }

    public void AddTime(float increment) => secondsRemaining += increment;

    public void Initialise(bool isWhite)
    {
        timerText = isWhite ? GameObject.FindGameObjectWithTag("WhiteTime").GetComponent<Text>() : GameObject.FindGameObjectWithTag("BlackTime").GetComponent<Text>();
    }

    public void Update()
    {
        if (isActive)
        {
            secondsRemaining = Mathf.Max(0, secondsRemaining - Time.deltaTime);

            if (secondsRemaining == 0)
            {
                isActive = false;
                // need to notify that we have timed out somehow
            }

            timerText.color = ActiveColor;
        }
        else
        {
            timerText.color = InactiveColor;
        }

        DisplayTime();
    }

    void DisplayTime()
    {
        int minutes = (int)(secondsRemaining / 60);
        int seconds = (int)(secondsRemaining % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
