using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   [SerializeField] private int hitsToDie = 5;
    private int hits;

    public bool IsDead { get; private set; }

    public void TakeHit(int amount = 1)
    {
        if (IsDead) return;

        hits += amount;
        if (hits >= hitsToDie)
        {
            IsDead = true;
            Destroy(gameObject);
        }
    }
}
