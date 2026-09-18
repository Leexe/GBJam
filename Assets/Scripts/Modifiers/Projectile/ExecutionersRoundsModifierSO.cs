using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "ExecutionersRounds", menuName = "Game/Modifiers/Projectile/ExecutionersRounds")]
	public class ExecutionersRoundsModifierSO : TowerModifierSO
	{
		public override TowerModifierInstance CreateInstance(Tower owner) =>
			new ExecutionersRoundsInstance(this, owner);
	}

	public class ExecutionersRoundsInstance : TowerModifierInstance
	{
		public ExecutionersRoundsInstance(TowerModifierSO data, Tower owner)
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
