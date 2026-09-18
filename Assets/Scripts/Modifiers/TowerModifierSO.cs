using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace Modifiers
{
	public enum ModifierTowerCategory
	{
		Any,
		Projectile,
		Explosive,
		Melee,
	}

	[CreateAssetMenu(fileName = "TowerModifierSO", menuName = "Game/Modifiers/TowerModifierSO", order = 0)]
	public class TowerModifierSO : ScriptableObject
	{
		[field: SerializeField]
		[field: TabGroup("Info", "General")]
		public string Id { get; private set; }

		[field: SerializeField]
		[field: TabGroup("Info", "General")]
		public string Name { get; private set; }

		[field: SerializeField]
		[field: TextArea]
		[field: TabGroup("Info", "General")]
		public string Description { get; private set; }

		[field: SerializeField]
		[field: TabGroup("Info", "General")]
		public Sprite Icon { get; private set; }

		[field: SerializeField]
		[field: TabGroup("Info", "General")]
		public ModifierTowerCategory Category { get; private set; } = ModifierTowerCategory.Any;

		[field: Header("Passive Stat Modifiers")]
		[field: SerializeField]
		[field: TabGroup("Info", "Stats")]
		public List<Modifier> StatModifiers { get; private set; } = new();

		public virtual TowerModifierInstance CreateInstance(Tower owner)
		{
			return new TowerModifierInstance(this, owner);
		}
	}
}
