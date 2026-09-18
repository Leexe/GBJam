using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "Tenderize", menuName = "Game/Modifiers/Melee/Tenderize")]
	public class TenderizeModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public float ConsecutiveHitMultiplier { get; private set; } = 1.5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new TenderizeInstance(this, owner);
	}

	public class TenderizeInstance : TowerModifierInstance
	{
		private readonly TenderizeModifierSO _so;
		private Enemy _lastTargetHit;

		public TenderizeInstance(TenderizeModifierSO data, Tower owner)
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
