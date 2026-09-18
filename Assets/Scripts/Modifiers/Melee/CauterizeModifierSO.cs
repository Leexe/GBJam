using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "Cauterize", menuName = "Game/Modifiers/Melee/Cauterize")]
	public class CauterizeModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public StatusEffectSO TargetStatusEffectSO { get; private set; }

		[field: SerializeField]
		public float BurningDamageMultiplier { get; private set; } = 1.5f;

		[field: SerializeField]
		public float NonBurningDamageMultiplier { get; private set; } = 0.7f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new CauterizeInstance(this, owner);
	}

	public class CauterizeInstance : TowerModifierInstance
	{
		private readonly CauterizeModifierSO _so;

		public CauterizeInstance(CauterizeModifierSO data, Tower owner)
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
