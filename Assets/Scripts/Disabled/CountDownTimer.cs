using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] private float StartSecond = 60f;
    [SerializeField] private bool StartOnAwake = true;

    [SerializeField] private TMP_Text timerText;

    public UnityEvent onTimeOut;
    public UnityEvent onReset;

    private float remaining;
    private bool running;

    public float Remaining => remaining;

    public bool isRunning => running;

    private void Awake()
    {
        remaining = StartSecond;

        UpdateUI();

        if (StartOnAwake)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if(!isRunning)
        {
            return;
        }

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            running = false;
            UpdateUI();
            onTimeOut?.Invoke();
            return;
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        remaining = StartSecond;
        UpdateUI();
        onReset?.Invoke();
    }

    public void ResetTimer(float newStartSecond)
    {
        StartSecond = Mathf.Max(0f, newStartSecond);
        ResetTimer();
    }

    private void UpdateUI()
    {
        if (timerText == null)
        return;

        int sec = Mathf.CeilToInt(remaining);
        int m = sec/60;
        int s = sec%60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
