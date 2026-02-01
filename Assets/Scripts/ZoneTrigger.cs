using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public enum ZoneType { Detect, Attack }

    [SerializeField] private ZoneType zoneType;
    [SerializeField] private string playerTag = "Player";

    private BigEnemyController owner;

    private void Awake()
    {
        owner = GetComponentInParent<BigEnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (owner == null) return;

        if (zoneType == ZoneType.Detect) owner.SetPlayerDetected(true, other.transform);
        else owner.SetPlayerInAttackRange(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (owner == null) return;

        if (zoneType == ZoneType.Detect) owner.SetPlayerDetected(false, other.transform);
        else owner.SetPlayerInAttackRange(false);
    }
}
