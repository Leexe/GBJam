using System.Collections.Generic;
using FMODUnity;
using Sirenix.OdinInspector;
using StatusEffects;
using UnityEngine;

public enum TowerType
{
	Melee,
	Projectile,
	Explosive,
}

[CreateAssetMenu(fileName = "TowerSO", menuName = "Game/TowerSO", order = 0)]
public class TowerSO : ScriptableObject
{
	[Header("Context")]
	[field: SerializeField]
	[field: TabGroup("Tab", "Context")]
	public string Name { get; private set; }

	[field: SerializeField]
	[field: TabGroup("Tab", "Context")]
	public string Id { get; private set; }

	[field: SerializeField]
	[field: TextArea]
	[field: TabGroup("Tab", "Context")]
	public string Description { get; private set; }

	[field: SerializeField]
	[field: TabGroup("Tab", "Context")]
	public TowerType TowerType { get; private set; }

	[field: SerializeField]
	[field: TabGroup("Tab", "Context")]
	[field: PreviewField(50, ObjectFieldAlignment.Left)]
	public Sprite Icon { get; private set; }

	[Header("Combat")]
	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	public float Range { get; private set; } = 2.5f;

	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	[field: ShowIf(nameof(TowerType), TowerType.Melee)]
	public float Knockback { get; private set; } = 0f;

	[TabGroup("Tab", "Combat")]
	public EventReference FireSfx;

	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	[field: ListDrawerSettings(ShowIndexLabels = true)]
	public List<TowerAttack> Attacks { get; private set; }

	[Header("Economy")]
	[field: SerializeField]
	[field: TabGroup("Tab", "Economy")]
	public int Cost { get; private set; } = 50;

	public int SellGold => Cost / 2;

	public float TotalAttackDelay
	{
		get
		{
			float total = 0f;
			for (int i = 0; i < Attacks.Count; i++)
			{
				total += Attacks[i].Delay;
			}
			return total;
		}
	}

	public float TotalDamage
	{
		get
		{
			float total = 0f;
			for (int i = 0; i < Attacks.Count; i++)
			{
				total += TowerType == TowerType.Melee ? Attacks[i].Damage : Attacks[i].ProjectileData.Damage;
			}
			return total;
		}
	}

	public int DPS => Mathf.RoundToInt(TotalDamage / TotalAttackDelay);
}

[System.Serializable]
public class TowerAttack
{
	[field: SerializeField]
	[field: MinValue(0.01f)]
	public float Delay { get; private set; } = 0.5f;

	[field: SerializeField]
	[field: ShowIf("@$root.TowerType == TowerType.Melee")]
	public float Damage { get; private set; } = 10f;

	[field: SerializeField]
	[field: HideIf("@$root.TowerType == TowerType.Melee")]
	public ProjectileSO ProjectileData { get; private set; }

	[field: SerializeField]
	public List<StatusEffectSO> StatusEffects { get; private set; }

	[field: SerializeField]
	[field: PreviewField(40, ObjectFieldAlignment.Left)]
	public Sprite[] TopSprites { get; private set; }

	[field: SerializeField]
	[field: PreviewField(40, ObjectFieldAlignment.Left)]
	public Sprite[] BottomSprites { get; private set; }

	[field: SerializeField]
	[field: MinValue(0)]
	public int ImpactFrame { get; private set; } = 1;
}
