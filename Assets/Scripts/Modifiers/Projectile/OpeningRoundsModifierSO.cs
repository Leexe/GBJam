using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "OpeningRounds", menuName = "Game/Modifiers/Projectile/OpeningRounds")]
	public class OpeningRoundsModifierSO : TowerModifierSO
	{
		public override TowerModifierInstance CreateInstance(Tower owner) =>
			new OpeningRoundsInstance(this, owner);
	}

	public class OpeningRoundsInstance : TowerModifierInstance
	{
		public OpeningRoundsInstance(TowerModifierSO data, Tower owner)
			: base(data, owner) { }

		public override bool CanTarget(Enemy target)
		{
			return target.CurrentHealth >= target.Data.Health * 0.5f;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			if (Mathf.Approximately(target.CurrentHealth, target.Data.Health))
			{
				damage *= 2f;
			}
		}
	}
}
