using Sirenix.OdinInspector;
using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "InfernalCrucible", menuName = "Game/Modifiers/Melee/InfernalCrucible")]
	public class InfernalCrucibleModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public StatusEffectSO TargetStatusEffectSO { get; private set; }

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float BurningDamageMultiplier { get; private set; } = 1.5f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float NonBurningDamageMultiplier { get; private set; } = 0.7f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new InfernalCrucibleInstance(this, owner);
	}

	public class InfernalCrucibleInstance : TowerModifierInstance
	{
		private readonly InfernalCrucibleModifierSO _so;

		public InfernalCrucibleInstance(InfernalCrucibleModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			if (target.StatusController.HasStatusEffect(_so.TargetStatusEffectSO))
			{
				damage *= _so.BurningDamageMultiplier;
			}
			else
			{
				damage *= _so.NonBurningDamageMultiplier;
			}
		}
	}
}
