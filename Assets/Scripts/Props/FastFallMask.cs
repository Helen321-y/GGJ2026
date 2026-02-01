using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastFallMask : MonoBehaviour
{
     [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) 
        return;

        player.UnlockSlowFall();
        Destroy(gameObject);
    }
}
