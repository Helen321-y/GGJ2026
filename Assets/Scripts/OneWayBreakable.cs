using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayBreakable : MonoBehaviour
{
     public enum BreakDirection
    {
        Left,    
        Right,  
        Above,  
        
        Below    
    }

    [SerializeField] private BreakDirection breakDirection = BreakDirection.Left;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float minApproachSpeed = 0.1f;
    [SerializeField] private GameObject rootToDestroy;

    private void Awake()
    {
        if (rootToDestroy == null)
            rootToDestroy = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector2 v = rb.velocity;

        bool allowed = breakDirection switch
        {
            BreakDirection.Left  => v.x >  minApproachSpeed,
            BreakDirection.Right => v.x < -minApproachSpeed,
            BreakDirection.Above => v.y < -minApproachSpeed,
            BreakDirection.Below => v.y >  minApproachSpeed,
            _ => false
        };

        if (!allowed) return;

        Destroy(rootToDestroy);
    }
}
