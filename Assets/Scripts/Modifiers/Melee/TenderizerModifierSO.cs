using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "Tenderizer", menuName = "Game/Modifiers/Melee/Tenderizer")]
	public class TenderizerModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float ConsecutiveHitMultiplier { get; private set; } = 1.5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new TenderizerInstance(this, owner);
	}

	public class TenderizerInstance : TowerModifierInstance
	{
		private readonly TenderizerModifierSO _so;
		private Enemy _lastTargetHit;

		public TenderizerInstance(TenderizerModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnBeforeDealDamage(Enemy target, ref float damage)
		{
			if (_lastTargetHit == target)
			{
				damage *= _so.ConsecutiveHitMultiplier;
			}
			_lastTargetHit = target;
		}
	}
}
