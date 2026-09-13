using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private float _frameRate = 0.2f;

	private EnemySO _data;
	private float _currentHealth;
	private List<Vector2Int> _waypoints;
	private int _currentWaypointIndex;
	private Tween _damageTween;
	private float _frameTimer;
	private int _currentFrame;

	public EnemySO Data => _data;
	public float CurrentHealth => _currentHealth;

	public void Initialize(EnemySO data)
	{
		_data = data;
		_currentHealth = data.Health;
		_waypoints = GridManager.Instance.EnemyWaypoints;
		_currentWaypointIndex = 1;
		_currentFrame = 0;
		_frameTimer = 0f;

		transform.position = GridManager.Instance.GridToWorld(_waypoints[0]);
		_spriteRenderer.sprite = _data.SpriteList[0];
		_spriteRenderer.flipX = false;
	}

	private void Update()
	{
		UpdateAnimation();
		UpdateMovement();
	}

	private void UpdateAnimation()
	{
		if (!_data)
		{
			return;
		}

		if (_data.SpriteList.Count <= 1)
		{
			return;
		}

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
		Vector3 target = GridManager.Instance.GridToWorld(_waypoints[_currentWaypointIndex]);
		Vector3 diff = target - transform.position;

		if (Mathf.Abs(diff.x) > 0.01f)
		{
			_spriteRenderer.flipX = diff.x < 0;
		}

		transform.position = Vector3.MoveTowards(transform.position, target, _data.Speed * Time.deltaTime);

		if (Vector3.Distance(transform.position, target) < 0.001f)
		{
			_currentWaypointIndex++;
			if (_currentWaypointIndex >= _waypoints.Count)
			{
				ReachGoal();
			}
		}
	}

	public void TakeDamage(float amount)
	{
		_currentHealth -= amount;
		_damageTween = Tween.PunchScale(transform, new Vector3(0.15f, 0.15f, 0f), 0.1f);

		if (_currentHealth <= 0f)
		{
			Die();
		}
	}

	private void Die()
	{
		GameManager.Instance.GiveGold(_data.GoldReward);
		Deactivate();
	}

	private void ReachGoal()
	{
		GameManager.Instance.DamageHealth(_data.Damage);
		Deactivate();
	}

	private void Deactivate()
	{
		_damageTween.Complete();
		EnemyPool.Instance.Release(this);
	}
}
