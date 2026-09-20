using System;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionHitbox : MonoBehaviour
{
	[SerializeField]
	private CircleCollider2D _collider;

	private readonly HashSet<Enemy> _hitEnemies = new();

	public void Trigger(
		float radius,
		float damage,
		LayerMask enemyLayerMask,
		Vector3 direction,
		Action<Enemy, float> onEnemyHit,
		Action<Collider2D[], Vector3> onExplosionHit,
		float knockback = 0f
	)
	{
		_hitEnemies.Clear();
		_collider.enabled = true;

		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayerMask);
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].TryGetComponent<Enemy>(out Enemy hitEnemy) && _hitEnemies.Add(hitEnemy))
			{
				Vector3 explosionDir = hitEnemy.transform.position - transform.position;
				explosionDir.z = 0f;
				Vector3 bloodDir = explosionDir == Vector3.zero ? direction : explosionDir.normalized;

				hitEnemy.PlayBloodParticles(bloodDir);
				hitEnemy.TakeDamage(damage);
				if (knockback > 0f)
				{
					hitEnemy.DisplaceAwayFrom(transform.position, knockback);
				}
				onEnemyHit?.Invoke(hitEnemy, damage);
			}
		}

		onExplosionHit?.Invoke(hits, transform.position);
	}

	public void Deactivate()
	{
		_collider.enabled = false;
		_hitEnemies.Clear();
	}
}
