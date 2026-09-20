using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private LayerMask _enemyLayerMask;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	private Vector3 _direction;
	private float _damage;
	private float _speed;
	private int _remainingPierce;
	private float _aoeRadius;
	private float _lifetimeTimer;
	private bool _isDeactivated;
	private readonly HashSet<Enemy> _hitEnemies = new();

	private bool _useDistanceScaling;
	private Vector3 _originPosition;
	private float _minDistance;
	private float _maxDistance;
	private Action<Collider2D[], Vector3> _onExplosionHit;
	private Action<Enemy, float> _onEnemyHit;

	public void Initialize(Transform target, ProjectileSO projectileData)
	{
		Vector3 diff = target.position - transform.position;
		diff.z = 0f;
		Initialize(diff.normalized, projectileData);
	}

	public void Initialize(Vector3 direction, ProjectileSO projectileData)
	{
		_direction = direction;
		_damage = projectileData.Damage;
		_speed = projectileData.Speed;
		_remainingPierce = projectileData.PierceCount;
		_aoeRadius = projectileData.IsAoe ? projectileData.AoeRadius : 0f;
		_lifetimeTimer = projectileData.Lifetime;
		_useDistanceScaling = false;
		_isDeactivated = false;
		_hitEnemies.Clear();
		_onExplosionHit = null;
		_onEnemyHit = null;

		transform.localScale = new Vector3(projectileData.Size.x, projectileData.Size.y, 1f);
		_spriteRenderer.sprite = projectileData.Sprite;

		float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
		transform.rotation = Quaternion.Euler(0f, 0f, angle);
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

		transform.position += _direction * (_speed * Time.deltaTime);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_isDeactivated || _remainingPierce <= 0)
		{
			return;
		}

		if ((_enemyLayerMask.value & (1 << other.gameObject.layer)) == 0)
		{
			return;
		}

		if (!other.TryGetComponent<Enemy>(out Enemy enemy) || !_hitEnemies.Add(enemy))
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
		else
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
		if (_isDeactivated)
		{
			return;
		}

		_isDeactivated = true;
		_hitEnemies.Clear();
		_onExplosionHit = null;
		_onEnemyHit = null;
		_useDistanceScaling = false;
		ProjectilePool.Instance.Release(this);
	}
}
