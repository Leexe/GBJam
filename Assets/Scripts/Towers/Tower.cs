using System.Collections.Generic;
using Modifiers;
using PrimeTween;
using Sirenix.OdinInspector;
using Stats;
using StatusEffects;
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

	[Header("Modifiers")]
	[SerializeField]
	private List<TowerModifierSO> _initialModifiers = new();

	[Header("Visual")]
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private SpriteRenderer _rangeIndicator;

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
	public StatsState Stats { get; private set; }
	public bool IsStunned => _stunTimer > 0f;
	public IReadOnlyList<TowerModifierInstance> ActiveModifiers => _modifierInstances;

	private float _attackTimer;
	private float _attackInterval;
	private float _rangeSqr;
	private Vector3 _position;
	private Sequence _attackSequence;
	private float _stunTimer;
	private readonly List<TowerModifierInstance> _modifierInstances = new();

	private void Awake()
	{
		_position = transform.position;
		if (_data)
		{
			SetupStats();
			EquipInitialModifiers();
		}
	}

	public void Initialize(TowerSO data)
	{
		_data = data;
		_attackTimer = 0f;
		_stunTimer = 0f;
		_position = transform.position;
		_spriteRenderer.transform.localPosition = Vector3.zero;

		SetupStats();
		EquipInitialModifiers();
	}

	private void SetupStats()
	{
		float baseDamage = _data.TowerType == TowerType.Melee ? _data.Damage : _data.ProjectileData.Damage;
		float baseAoe =
			_data.ProjectileData != null && _data.ProjectileData.IsAoe ? _data.ProjectileData.AoeRadius : 0f;

		var baseMap = new Dictionary<StatType, float>
		{
			{ StatType.Range, _data.Range },
			{ StatType.Damage, baseDamage },
			{ StatType.AttackRate, _data.AttackRate },
			{ StatType.ExplosionRadius, baseAoe },
		};

		Stats = new StatsState(baseMap);
		Stats.OnStatChanged += HandleStatChanged;
		RecalculateDerivedStats();
	}

	private void EquipInitialModifiers()
	{
		for (int i = 0; i < _modifierInstances.Count; i++)
		{
			_modifierInstances[i].OnUnequipped();
		}
		_modifierInstances.Clear();

		for (int i = 0; i < _initialModifiers.Count; i++)
		{
			EquipModifier(_initialModifiers[i]);
		}
	}

	public void EquipModifier(TowerModifierSO modifierSO)
	{
		TowerModifierInstance instance = modifierSO.CreateInstance(this);
		_modifierInstances.Add(instance);
		instance.OnEquipped();
	}

	public void UnequipModifier(TowerModifierSO modifierSO)
	{
		for (int i = _modifierInstances.Count - 1; i >= 0; i--)
		{
			if (_modifierInstances[i].Data == modifierSO)
			{
				_modifierInstances[i].OnUnequipped();
				_modifierInstances.RemoveAt(i);
				break;
			}
		}
	}

	public void Stun(float duration)
	{
		_stunTimer = Mathf.Max(_stunTimer, duration);
	}

	public void ApplyStatusEffectToAllEnemiesInRange(StatusEffectSO effectSO)
	{
		List<Enemy> enemies = EnemyPool.ActiveEnemies;
		for (int i = 0; i < enemies.Count; i++)
		{
			Enemy enemy = enemies[i];
			if (enemy.CurrentHealth > 0f)
			{
				float sqrDist = (enemy.transform.position - _position).sqrMagnitude;
				if (sqrDist <= _rangeSqr)
				{
					enemy.StatusController.ApplyStatusEffect(effectSO);
				}
			}
		}
	}

	private void HandleStatChanged(StatChangedEventArgs args)
	{
		RecalculateDerivedStats();
	}

	private void RecalculateDerivedStats()
	{
		float range = Stats.GetFinalStat(StatType.Range);
		_rangeSqr = range * range;
		_attackInterval = 1f / Stats.GetFinalStat(StatType.AttackRate);
		UpdateRangeIndicator();
	}

	private void UpdateRangeIndicator()
	{
		_rangeIndicator.transform.localPosition = Vector3.zero;
		float diameter = Stats.GetFinalStat(StatType.Range) * 2f;
		_rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
	}

	public void OnCursorEnter()
	{
		_rangeIndicator.gameObject.SetActive(true);
	}

	public void OnCursorExit()
	{
		_rangeIndicator.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		_attackSequence.Stop();
		_spriteRenderer.transform.localPosition = Vector3.zero;
		_rangeIndicator.gameObject.SetActive(false);
	}

	private void Update()
	{
		for (int i = 0; i < _modifierInstances.Count; i++)
		{
			_modifierInstances[i].Update(Time.deltaTime);
		}

		if (_stunTimer > 0f)
		{
			_stunTimer -= Time.deltaTime;
			return;
		}

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
			if (enemy.CurrentHealth <= 0f)
			{
				continue;
			}

			float sqrDist = (enemy.transform.position - _position).sqrMagnitude;
			if (sqrDist > _rangeSqr)
			{
				continue;
			}

			bool canTarget = true;
			for (int m = 0; m < _modifierInstances.Count; m++)
			{
				if (!_modifierInstances[m].CanTarget(enemy))
				{
					canTarget = false;
					break;
				}
			}
			if (!canTarget)
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
		for (int i = 0; i < _modifierInstances.Count; i++)
		{
			_modifierInstances[i].OnAttack(target);
		}

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
		float damage = Stats.GetFinalStat(StatType.Damage);
		for (int i = 0; i < _modifierInstances.Count; i++)
		{
			_modifierInstances[i].OnBeforeDealDamage(target, ref damage);
		}

		if (_data.TowerType == TowerType.Melee)
		{
			target.TakeDamage(damage);
			for (int i = 0; i < _modifierInstances.Count; i++)
			{
				_modifierInstances[i].OnAfterDealDamage(target, damage);
			}
		}
		else
		{
			Projectile proj = ProjectilePool.Instance.Get(transform.position, target.transform, _data.ProjectileData);
			proj.SetDamage(damage);

			float finalAoe = Stats.GetFinalStat(StatType.ExplosionRadius);
			if (finalAoe > 0f)
			{
				proj.SetAoeRadius(finalAoe);
			}

			proj.AddExplosionHitListener(
				(hits, center) =>
				{
					for (int i = 0; i < _modifierInstances.Count; i++)
					{
						_modifierInstances[i].OnExplosionHit(hits, center);
					}
				}
			);

			proj.AddEnemyHitListener(
				(hitEnemy, finalDmg) =>
				{
					for (int i = 0; i < _modifierInstances.Count; i++)
					{
						_modifierInstances[i].OnAfterDealDamage(hitEnemy, finalDmg);
					}
				}
			);

			for (int i = 0; i < _modifierInstances.Count; i++)
			{
				_modifierInstances[i].OnProjectileCreated(proj);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		float range = Stats != null ? Stats.GetFinalStat(StatType.Range) : (_data ? _data.Range : 2.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, range);
	}

	public void OnRemove()
	{
		GameManager.Instance.GiveGold(Data.SellGold);
	}
}
