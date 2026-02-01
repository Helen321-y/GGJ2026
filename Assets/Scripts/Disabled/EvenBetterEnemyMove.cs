using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvenBetterEnemyMove : MonoBehaviour
{
    //animations needed: facingRight, moving, windup, attacking
    public float speed;

    public Transform trans;

    public Rigidbody2D rb;

    Vector2 movement;
    bool canMove;
    bool attacking;

    public Transform target;

    public Animator anim;

    public float attackRange;

    public float windUpTime;

    public float attackSpeed;

    public float attackTime;


    private void Start()
    {
        canMove = true;
    }

    void FixedUpdate()
    {
        if(target.position.x > trans.position.x)
        {
            if (canMove == true) movement.x = speed;
            if (attacking == true) movement.x = attackSpeed;
            anim.SetBool("FacingRight", true);
            if(target.position.x - trans.position.x < attackRange)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            if (canMove == true) movement.x = -speed;
            if (attacking == true) movement.x = -attackSpeed;
            anim.SetBool("FacingRight", false);
            if (target.position.x - trans.position.x > -attackRange)
            {
                StartCoroutine(Attack());
            }
        }

        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    IEnumerator Attack()
    {
        anim.SetBool("WindingUp", true);
        yield return new WaitForSeconds(windUpTime);
        anim.SetBool("WindingUp", false);
        attacking = true;
        anim.SetBool("Attacking", true);
        yield return new WaitForSeconds(attackTime);
        anim.SetBool("Attacking", false);
    }
}
