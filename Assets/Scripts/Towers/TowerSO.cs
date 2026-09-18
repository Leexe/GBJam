using Sirenix.OdinInspector;
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
	public Sprite Icon { get; private set; }

	[Header("Combat")]
	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	public float Range { get; private set; } = 2.5f;

	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	[field: ShowIf(nameof(TowerType), TowerType.Melee)]
	public float Damage { get; private set; } = 10f;

	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	[field: MinValue(0.01f)]
	public float AttackRate { get; private set; } = 1f;

	[field: SerializeField]
	[field: TabGroup("Tab", "Combat")]
	[field: Required]
	[field: HideIf(nameof(TowerType), TowerType.Melee)]
	public ProjectileSO ProjectileData { get; private set; }

	[Header("Economy")]
	[field: SerializeField]
	[field: TabGroup("Tab", "Economy")]
	public int Cost { get; private set; } = 50;

	public int SellGold => Cost / 2;
}
