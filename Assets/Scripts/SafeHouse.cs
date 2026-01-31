using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeHouse : MonoBehaviour
{
    [SerializeField] private CountDownTimer timer;
    [SerializeField] string playerTag = "Player";

    [SerializeField] private bool ResumeTimer = true;

    private BoxCollider2D _collider2d;

    private void Start()
    {

        _collider2d = GetComponent<BoxCollider2D>();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag(playerTag))
        return;

        if(timer == null)
        return;

        timer.ResetTimer();
        if(ResumeTimer)
        timer.StartTimer();
    }
}
