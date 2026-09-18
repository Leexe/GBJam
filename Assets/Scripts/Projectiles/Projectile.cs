using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private LayerMask _enemyLayerMask;

	private Transform _target;
	private float _damage;
	private float _speed;
	private int _remainingPierce;
	private float _aoeRadius;
	private float _lifetimeTimer;

	private bool _useDistanceScaling;
	private Vector3 _originPosition;
	private float _minDistance;
	private float _maxDistance;
	private Action<Collider2D[], Vector3> _onExplosionHit;
	private Action<Enemy, float> _onEnemyHit;

	public void Initialize(Transform target, ProjectileSO projectileData)
	{
		_target = target;
		_damage = projectileData.Damage;
		_speed = projectileData.Speed;
		_remainingPierce = projectileData.PierceCount;
		_aoeRadius = projectileData.IsAoe ? projectileData.AoeRadius : 0f;
		_lifetimeTimer = projectileData.Lifetime;
		_useDistanceScaling = false;
		_onExplosionHit = null;
		_onEnemyHit = null;
	}

	public void SetDamage(float damage) => _damage = damage;

	public void SetAoeRadius(float aoeRadius) => _aoeRadius = aoeRadius;

	public void SetDistanceDamageScaler(Vector3 origin, float minDistance, float maxDistance)
	{
		_useDistanceScaling = true;
		_originPosition = origin;
		_minDistance = minDistance;
		_maxDistance = maxDistance;
	}

	public void AddExplosionHitListener(Action<Collider2D[], Vector3> callback)
	{
		_onExplosionHit += callback;
	}

	public void AddEnemyHitListener(Action<Enemy, float> callback)
	{
		_onEnemyHit += callback;
	}

	private void Update()
	{
		_lifetimeTimer -= Time.deltaTime;
		if (_lifetimeTimer <= 0f)
		{
			Deactivate();
			return;
		}

		transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
		if (transform.position - _target.position != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(transform.position - _target.position);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if ((_enemyLayerMask.value & (1 << other.gameObject.layer)) == 0)
		{
			return;
		}

		float finalDamage = _damage;
		if (_useDistanceScaling)
		{
			float dist = Vector3.Distance(_originPosition, transform.position);
			if (dist < _minDistance)
			{
				finalDamage *= 0.15f;
			}
			else
			{
				float t = Mathf.Clamp01((dist - _minDistance) / Mathf.Max(0.1f, _maxDistance - _minDistance));
				finalDamage *= Mathf.Lerp(1f, 2.5f, t);
			}
		}

		if (_aoeRadius > 0f)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _aoeRadius, _enemyLayerMask);
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].TryGetComponent<Enemy>(out Enemy hitEnemy))
				{
					hitEnemy.TakeDamage(finalDamage);
					_onEnemyHit?.Invoke(hitEnemy, finalDamage);
				}
			}
			_onExplosionHit?.Invoke(hits, transform.position);
		}
		else if (other.TryGetComponent<Enemy>(out Enemy enemy))
		{
			enemy.TakeDamage(finalDamage);
			_onEnemyHit?.Invoke(enemy, finalDamage);
		}

		_remainingPierce--;
		if (_remainingPierce <= 0)
		{
			Deactivate();
		}
	}

	private void Deactivate()
	{
		_onExplosionHit = null;
		_onEnemyHit = null;
		_useDistanceScaling = false;
		ProjectilePool.Instance.Release(this);
	}
}
