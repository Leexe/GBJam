using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "OverheatingSwing", menuName = "Game/Modifiers/Melee/OverheatingSwing")]
	public class OverheatingSwingModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public int AttacksToProc { get; private set; } = 5;

		[field: SerializeField]
		public float OverheatDuration { get; private set; } = 2f;

		[field: SerializeField]
		public StatusEffectSO BurnEffectSO { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) => new OverheatingSwingInstance(this, owner);
	}

	public class OverheatingSwingInstance : TowerModifierInstance
	{
		private readonly OverheatingSwingModifierSO _so;
		private int _attackCounter;

		public OverheatingSwingInstance(OverheatingSwingModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnAttack(Enemy target)
		{
			_attackCounter++;
			if (_attackCounter >= _so.AttacksToProc)
			{
				_attackCounter = 0;
				Owner.ApplyStatusEffectToAllEnemiesInRange(_so.BurnEffectSO);
				Owner.Stun(_so.OverheatDuration);
			}
		}
	}
}
