using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPushable : MonoBehaviour
{
    public enum AllowedPushDirection { LeftToRight, RightToLeft }

    [SerializeField] private AllowedPushDirection allowed = AllowedPushDirection.LeftToRight;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;

    private bool playerOnLeft;
    private bool playerOnRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        FreezeX(true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        Vector2 playerPos = collision.transform.position;
        Vector2 myPos = transform.position;

        playerOnLeft  = playerPos.x < myPos.x;
        playerOnRight = playerPos.x > myPos.x;

        bool allow =
            (allowed == AllowedPushDirection.LeftToRight && playerOnLeft) ||
            (allowed == AllowedPushDirection.RightToLeft && playerOnRight);

        FreezeX(!allow);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        FreezeX(true);
    }

    private void FreezeX(bool freeze)
    {
       if (freeze)
        rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
       else
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
    }
}
