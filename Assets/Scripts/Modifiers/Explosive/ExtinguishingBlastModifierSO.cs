using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "ExtinguishingBlast", menuName = "Game/Modifiers/Explosive/ExtinguishingBlast")]
	public class ExtinguishingBlastModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public StatusEffectSO StatusEffectToRemove { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) =>
			new ExtinguishingBlastInstance(this, owner);
	}

	public class ExtinguishingBlastInstance : TowerModifierInstance
	{
		private readonly ExtinguishingBlastModifierSO _so;

		public ExtinguishingBlastInstance(ExtinguishingBlastModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnExplosionHit(Collider2D[] hits, Vector3 explosionCenter)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].TryGetComponent<Enemy>(out Enemy enemy))
				{
					enemy.StatusController.RemoveStatusEffect(_so.StatusEffectToRemove);
				}
			}
		}
	}
}
