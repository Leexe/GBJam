using System;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionHitbox : MonoBehaviour
{
	[SerializeField]
	private CircleCollider2D _collider;

	private readonly HashSet<Enemy> _hitEnemies = new();
	private float _damage;
	private Vector3 _fallbackDirection;
	private Action<Enemy, float> _onEnemyHit;

	public Collider2D[] Trigger(
		float radius,
		float damage,
		LayerMask enemyLayerMask,
		Vector3 fallbackDirection,
		Action<Enemy, float> onEnemyHit,
		Action<Collider2D[], Vector3> onExplosionHit
	)
	{
		_damage = damage;
		_fallbackDirection = fallbackDirection;
		_onEnemyHit = onEnemyHit;
		_hitEnemies.Clear();

		_collider.isTrigger = true;
		_collider.enabled = true;

		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayerMask);
		for (int i = 0; i < hits.Length; i++)
		{
			if (hits[i].TryGetComponent<Enemy>(out Enemy hitEnemy) && _hitEnemies.Add(hitEnemy))
			{
				Vector3 explosionDir = hitEnemy.transform.position - transform.position;
				explosionDir.z = 0f;
				Vector3 bloodDir = explosionDir.sqrMagnitude > 0.0001f ? explosionDir.normalized : _fallbackDirection;

				hitEnemy.PlayBloodParticles(bloodDir);
				hitEnemy.TakeDamage(_damage);
				_onEnemyHit?.Invoke(hitEnemy, _damage);
			}
		}

		onExplosionHit?.Invoke(hits, transform.position);
		return hits;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent<Enemy>(out Enemy hitEnemy) && _hitEnemies.Add(hitEnemy))
		{
			Vector3 explosionDir = hitEnemy.transform.position - transform.position;
			explosionDir.z = 0f;
			Vector3 bloodDir = explosionDir.sqrMagnitude > 0.0001f ? explosionDir.normalized : _fallbackDirection;

			hitEnemy.PlayBloodParticles(bloodDir);
			hitEnemy.TakeDamage(_damage);
			_onEnemyHit?.Invoke(hitEnemy, _damage);
		}
	}

	public void Deactivate()
	{
		_collider.enabled = false;
		_hitEnemies.Clear();
		_onEnemyHit = null;
	}
}
