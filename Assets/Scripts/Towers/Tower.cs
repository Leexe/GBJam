using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public enum TowerPriorityType
{
	First,
	Far,
	Near,
	Strong,
	Weak,
}

public class Tower : MonoBehaviour
{
	[Header("Data")]
	[SerializeField]
	private TowerSO _data;

	[SerializeField]
	private TowerPriorityType _priorityType = TowerPriorityType.First;

	[Header("Visual")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[Header("Attack Animation")]
	[SerializeField]
	private float _launchDistance = 0.1f;

	[SerializeField]
	[MinMaxSlider(0f, 1f, showFields: true)]
	private Vector2 _launchReturnRatio = new(0.2f, 0.4f);

	[SerializeField]
	private Ease _launchEase = Ease.OutQuad;

	[SerializeField]
	private Ease _returnEase = Ease.InQuad;

	public TowerPriorityType PriorityType
	{
		get => _priorityType;
		set => _priorityType = value;
	}
	public TowerSO Data => _data;

	private float _attackTimer;
	private float _attackInterval;
	private float _rangeSqr;
	private Vector3 _position;
	private Sequence _attackSequence;

	private void Awake()
	{
		_position = transform.localPosition;
		if (_data)
		{
			_rangeSqr = _data.Range * _data.Range;
			_attackInterval = 1f / _data.AttackRate;
		}
	}

	public void Initialize(TowerSO data)
	{
		_data = data;
		_attackTimer = 0f;
		_position = transform.localPosition;
		_rangeSqr = data.Range * data.Range;
		_attackInterval = 1f / data.AttackRate;
		_spriteRenderer.transform.localPosition = Vector3.zero;
	}

	private void OnDisable()
	{
		_attackSequence.Stop();
		_spriteRenderer.transform.localPosition = Vector3.zero;
	}

	private void Update()
	{
		_attackTimer -= Time.deltaTime;
		if (_attackTimer > 0f)
		{
			return;
		}

		Enemy target = FindTarget();
		if (target == null)
		{
			_attackTimer = 0.1f;
			return;
		}

		Attack(target);
		_attackTimer = _attackInterval;
	}

	private Enemy FindTarget()
	{
		List<Enemy> enemies = EnemyPool.ActiveEnemies;
		Enemy targetEnemy = null;
		bool isMin = _priorityType is TowerPriorityType.Near or TowerPriorityType.Weak;
		float bestScore = isMin ? float.MaxValue : -1f;

		for (int i = 0; i < enemies.Count; i++)
		{
			Enemy enemy = enemies[i];
			float sqrDist = (enemy.transform.position - _position).sqrMagnitude;
			if (sqrDist > _rangeSqr)
			{
				continue;
			}

			float score = _priorityType switch
			{
				TowerPriorityType.First => enemy.TravelProgress,
				TowerPriorityType.Near or TowerPriorityType.Far => sqrDist,
				TowerPriorityType.Strong or TowerPriorityType.Weak => enemy.CurrentHealth,
				_ => 0f,
			};

			if (isMin ? score < bestScore : score > bestScore)
			{
				bestScore = score;
				targetEnemy = enemy;
			}
		}

		return targetEnemy;
	}

	private void Attack(Enemy target)
	{
		float launchDuration = _attackInterval * _launchReturnRatio.x;
		float returnDuration = _attackInterval * _launchReturnRatio.y;

		Vector3 direction = (target.transform.position - _position).normalized;
		Vector3 targetPos = _position + (direction * _launchDistance);

		_attackSequence.Stop();
		_attackSequence = Sequence
			.Create()
			.Chain(Tween.Position(_spriteRenderer.transform, targetPos, launchDuration, _launchEase))
			.ChainCallback(() =>
			{
				PerformAttack(target);
			})
			.Chain(Tween.Position(_spriteRenderer.transform, _position, returnDuration, _returnEase));
	}

	private void PerformAttack(Enemy target)
	{
		if (_data.TowerType == TowerType.Melee)
		{
			target.TakeDamage(_data.Damage);
		}
		else
		{
			ProjectilePool.Instance.Get(transform.position, target.transform, _data.ProjectileData);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_data)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, _data.Range);
		}
	}

	public void OnRemove()
	{
		GameManager.Instance.GiveGold(Data.SellGold);
	}
}
