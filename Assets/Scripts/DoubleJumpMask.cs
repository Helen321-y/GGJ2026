using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpMask : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

  
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        pc.EnableDoubleJump();
        Destroy(gameObject);
    }
}
