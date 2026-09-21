using System;
using System.Collections.Generic;
using PrimeTween;
using StatusEffects;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Header("Visuals & Animation")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private StatusEffectController _statusController;

	[SerializeField]
	private float _frameRate = 0.2f;

	[Header("Hit Stop")]
	[SerializeField]
	private float _hitStopDuration = 0.06f;

	[SerializeField]
	private float _hitStopCooldown = 0.2f;

	[Header("Death & Blood")]
	[SerializeField]
	private ParticleSystem _deathParticles;

	[SerializeField]
	private ParticleSystem _bloodParticles;

	[SerializeField]
	private ParticleSystem _burnParticles;

	[SerializeField]
	private float _particleLingerDuration = 0.5f;

	private EnemySO _data;
	private EnemyModifier _modifier;
	private float _currentHealth;
	private List<Vector2Int> _waypoints;
	private int _currentWaypointIndex;
	private Sequence _deathSequence;
	private float _frameTimer;
	private int _currentFrame;
	private float _totalDistance;
	private float _hitStopTimer;
	private float _hitStopCooldownTimer;
	private float _speedMultiplier = 1f;
	private float _speedBuffTimer;

	public EnemySO Data => _data;
	public EnemyModifier Modifier => _modifier;
	public float CurrentHealth => _currentHealth;
	public float MaxHealth { get; private set; }
	public float DistanceTraveled { get; private set; }
	public float TravelProgress { get; private set; }
	public StatusEffectController StatusController => _statusController;

	[HideInInspector]
	public Action<Enemy> OnDeath;

	public void Initialize(EnemySO data, EnemyModifier modifier = default)
	{
		_data = data;
		_modifier = modifier;
		MaxHealth = data.Health * _modifier.EffectiveHealth;
		_currentHealth = MaxHealth;
		_waypoints = GridManager.Instance.EnemyWaypoints;
		_totalDistance = GridManager.Instance.TotalPathDistance;
		DistanceTraveled = 0f;
		TravelProgress = 0f;
		_currentWaypointIndex = 1;
		_currentFrame = 0;
		_frameTimer = 0f;
		_hitStopTimer = 0f;
		_hitStopCooldownTimer = 0f;
		_speedMultiplier = 1f;
		_speedBuffTimer = 0f;

		transform.position = GridManager.Instance.GridToWorld(_waypoints[0]);
		transform.localScale = Vector3.one * _modifier.EffectiveScale;
		_spriteRenderer.gameObject.SetActive(true);
		_spriteRenderer.transform.localPosition = Vector3.zero;
		_spriteRenderer.transform.localScale = Vector3.one;
		_spriteRenderer.sprite = _data.SpriteList[0];
		_spriteRenderer.flipX = false;
		_deathParticles.gameObject.SetActive(false);
		_bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_burnParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		_statusController.OnStatusApplied += HandleStatusApplied;
		_statusController.OnStatusEnded += HandleStatusEnded;
		_statusController.Initialize();
	}

	private void Update()
	{
		if (_currentHealth <= 0f)
		{
			return;
		}

		if (_hitStopCooldownTimer > 0f)
		{
			_hitStopCooldownTimer -= Time.deltaTime;
		}

		if (_speedBuffTimer > 0f)
		{
			_speedBuffTimer -= Time.deltaTime;
			if (_speedBuffTimer <= 0f)
			{
				_speedMultiplier = 1f;
			}
		}

		_statusController.UpdateEffects(Time.deltaTime);

		UpdateAnimation();
		UpdateMovement();
	}

	private void UpdateAnimation()
	{
		_frameTimer += Time.deltaTime;
		if (_frameTimer >= _frameRate)
		{
			_frameTimer = 0f;
			_currentFrame = (_currentFrame + 1) % _data.SpriteList.Count;
			_spriteRenderer.sprite = _data.SpriteList[_currentFrame];
		}
	}

	private void UpdateMovement()
	{
		if (_hitStopTimer > 0f)
		{
			_hitStopTimer -= Time.deltaTime;
			return;
		}

		Vector3 target = GridManager.Instance.GridToWorld(_waypoints[_currentWaypointIndex]);
		Vector3 diff = target - transform.position;

		if (Mathf.Abs(diff.x) > 0.01f)
		{
			_spriteRenderer.flipX = diff.x < 0;
		}

		Vector3 prevPosition = transform.position;
		transform.position = Vector3.MoveTowards(
			transform.position,
			target,
			_data.Speed * _modifier.EffectiveSpeed * _speedMultiplier * Time.deltaTime
		);

		DistanceTraveled += Vector3.Distance(prevPosition, transform.position);
		TravelProgress = Mathf.Clamp01(DistanceTraveled / _totalDistance);

		if (Vector3.Distance(transform.position, target) < 0.001f)
		{
			_currentWaypointIndex++;
			if (_currentWaypointIndex >= _waypoints.Count)
			{
				ReachGoal();
				return;
			}
		}
	}

	public void BuffSpeed(float multiplier, float duration)
	{
		_speedMultiplier = multiplier;
		_speedBuffTimer = duration;
	}

	public void DisplaceToward(Vector3 center, float maxDistance)
	{
		Vector3 direction = (center - transform.position).normalized;
		float distance = Mathf.Min(maxDistance, Vector3.Distance(transform.position, center));
		transform.position += direction * distance;
	}

	public void DisplaceAwayFrom(Vector3 center, float distance)
	{
		Vector3 direction = (transform.position - center).normalized;
		transform.position += direction * distance;
	}

	public void ApplyKnockback(Vector3 direction, float distance)
	{
		Vector3 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.up;
		transform.position += dir * distance;
	}

	public void TakeDamage(float amount)
	{
		_currentHealth -= amount;

		if (_hitStopCooldownTimer <= 0f)
		{
			_hitStopTimer = _hitStopDuration;
			_hitStopCooldownTimer = _hitStopCooldown;
		}

		if (_currentHealth <= 0f)
		{
			Die();
		}
	}

	private void Die()
	{
		_deathSequence.Stop();
		if (!_data.DeathSfx.IsNull)
		{
			AudioManager.Instance.PlayOneShot(_data.DeathSfx);
		}
		GameManager.Instance.GiveGold(Mathf.RoundToInt(_data.GoldReward * _modifier.EffectiveGold));
		_spriteRenderer.gameObject.SetActive(false);
		_deathParticles.gameObject.SetActive(true);
		_deathParticles.Play();
		_deathSequence = Sequence.Create().ChainDelay(_particleLingerDuration).ChainCallback(Deactivate);
	}

	private void ReachGoal()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CreatureEnters_Sfx);
		GameManager.Instance.DamageHealth(Mathf.RoundToInt(_data.Damage * _modifier.EffectiveDamage));
		CameraManager.Instance.ShakeScreen();

		Deactivate();
	}

	private void Deactivate()
	{
		_deathSequence.Stop();
		_hitStopTimer = 0f;
		_hitStopCooldownTimer = 0f;
		_speedMultiplier = 1f;
		_speedBuffTimer = 0f;
		_statusController.OnStatusApplied -= HandleStatusApplied;
		_statusController.OnStatusEnded -= HandleStatusEnded;
		_statusController.ClearStatusEffects();
		_deathParticles.gameObject.SetActive(false);
		_bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_burnParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_spriteRenderer.gameObject.SetActive(true);
		_spriteRenderer.transform.localPosition = Vector3.zero;
		_spriteRenderer.transform.localScale = Vector3.one;
		transform.localScale = Vector3.one;
		OnDeath?.Invoke(this);
		EnemyPool.Instance.Release(this);
	}

	public void PlayBloodParticles(Vector3 direction)
	{
		float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 45f;
		_bloodParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		_bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		_bloodParticles.Play();
	}

	private void HandleStatusApplied(StatusEffect effect)
	{
		if (effect.Data.Id == "burn")
		{
			_burnParticles.Play();
		}
	}

	private void HandleStatusEnded(StatusEffect effect)
	{
		if (effect.Data.Id == "burn")
		{
			_burnParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		}
	}
}
