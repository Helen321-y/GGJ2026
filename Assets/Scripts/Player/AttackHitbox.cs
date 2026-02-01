using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damagePerHit = 1;
    private readonly HashSet<EnemyHealth> _hitThisSwing = new HashSet<EnemyHealth>();

    public void BeginSwing()
    {
        _hitThisSwing.Clear();
    }

    public class PlayerAttackHitbox : MonoBehaviour
    {
        [SerializeField] private int damagePerHit = 1;
        private readonly HashSet<EnemyHealth> hitThisSwing = new HashSet<EnemyHealth>();

        public void BeginSwing() => hitThisSwing.Clear();

        private void OnTriggerEnter2D(Collider2D other)
        {
            var eh = other.GetComponentInParent<EnemyHealth>();
            if (eh == null || eh.IsDead) return;

            if (hitThisSwing.Add(eh))
                eh.TakeHit(damagePerHit);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;
        if (eh.IsDead) return;

        if (_hitThisSwing.Add(eh))
        {
            eh.TakeHit(damagePerHit);

        var enemyCtrl = eh.GetComponent<BigEnemyController>();
        if (enemyCtrl != null)
            enemyCtrl.TakeKnockbackFromPlayer(transform.position);
    }
}
}
