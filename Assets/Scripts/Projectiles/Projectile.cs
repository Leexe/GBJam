using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private Collider2D _projectileCollider;

	[SerializeField]
	private LayerMask _enemyLayerMask;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private SpriteRenderer _circleVisual;

	[SerializeField]
	private ParticleSystem _explosionParticles;

	[SerializeField]
	private ExplosionHitbox _explosionHitbox;

	[SerializeField]
	private float _explosionDuration = 0.15f;

	private Vector3 _direction;
	private float _damage;
	private float _speed;
	private int _remainingPierce;
	private float _aoeRadius;
	private float _lifetimeTimer;
	private Tween _explosionTween;
	private readonly HashSet<Enemy> _hitEnemies = new();

	private bool _useDistanceScaling;
	private Vector3 _originPosition;
	private float _minDistance;
	private float _maxDistance;
	private Action<Collider2D[], Vector3> _onExplosionHit;
	private Action<Enemy, float> _onEnemyHit;

	private void Awake()
	{
		if (_projectileCollider == null)
		{
			_projectileCollider = GetComponent<Collider2D>();
		}
		if (_explosionHitbox == null)
		{
			_explosionHitbox = GetComponentInChildren<ExplosionHitbox>(true);
		}
	}

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
		_hitEnemies.Clear();
		_onExplosionHit = null;
		_onEnemyHit = null;

		transform.localScale = new Vector3(projectileData.Size.x, projectileData.Size.y, 1f);
		_spriteRenderer.enabled = true;
		_spriteRenderer.sprite = projectileData.Sprite;

		_projectileCollider.enabled = true;

		_explosionTween.Stop();
		_circleVisual.gameObject.SetActive(false);
		_explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_explosionHitbox.Deactivate();

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
		if (_remainingPierce <= 0)
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
			_projectileCollider.enabled = false;
			_explosionHitbox.Trigger(
				_aoeRadius,
				finalDamage,
				_enemyLayerMask,
				_direction,
				_onEnemyHit,
				_onExplosionHit
			);

			Explode();
			return;
		}
		else
		{
			enemy.PlayBloodParticles(_direction);
			enemy.TakeDamage(finalDamage);
			_onEnemyHit?.Invoke(enemy, finalDamage);
		}

		_remainingPierce--;
		if (_remainingPierce <= 0)
		{
			Deactivate();
		}
	}

	private void Explode()
	{
		_remainingPierce = 0;
		_speed = 0f;
		_spriteRenderer.enabled = false;
		_projectileCollider.enabled = false;
		transform.localScale = Vector3.one;
		transform.rotation = Quaternion.identity;
		_explosionParticles.Play();

		float diameter = _aoeRadius * 2f;
		_circleVisual.transform.localPosition = Vector3.zero;
		_circleVisual.transform.localRotation = Quaternion.identity;
		_circleVisual.transform.localScale = Vector3.zero;
		_circleVisual.gameObject.SetActive(true);

		_explosionTween.Stop();
		_explosionTween = Tween
			.Scale(_circleVisual.transform, new Vector3(diameter, diameter, 1f), _explosionDuration, Ease.OutQuad)
			.OnComplete(this, target => target.Deactivate());
	}

	private void Deactivate()
	{
		_explosionTween.Stop();
		_circleVisual.gameObject.SetActive(false);
		_explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_explosionHitbox.Deactivate();
		_projectileCollider.enabled = false;
		_hitEnemies.Clear();
		_onExplosionHit = null;
		_onEnemyHit = null;
		_useDistanceScaling = false;
		ProjectilePool.Instance.Release(this);
	}
}
