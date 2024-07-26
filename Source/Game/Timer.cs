using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public event System.Action TimedOut;

    public Color ActiveColor;
    public Color InactiveColor;

    public Text timerText;

    public float secondsRemaining;
    public bool isActive;

    public float secondsRemainingAtStart;

    public float TimeElapsedThisTurn => secondsRemainingAtStart - secondsRemaining;

    public void SetActive(bool isActive) => this.isActive = isActive;

    public void Toggle() => isActive = !isActive;

    public void SetTime(int seconds) => secondsRemaining = seconds;

    public void AddTime(float increment) => secondsRemaining += increment;

    public void Initialise(bool isWhite)
    {
        timerText = isWhite ? GameObject.FindGameObjectWithTag("WhiteTime").GetComponent<Text>() : GameObject.FindGameObjectWithTag("BlackTime").GetComponent<Text>();
    }

    public void Start()
    {
        ActiveColor = new Color(1, 1, 1, 1);
        InactiveColor = new Color(0.7f, 0.7f, 0.7f, 1);
    }

    public void Update()
    {
        if (isActive)
        {
            secondsRemaining = Mathf.Max(0, secondsRemaining - Time.deltaTime);

            if (secondsRemaining == 0)
            {
                isActive = false;
                TimedOut.Invoke();
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
