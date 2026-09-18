using Sirenix.OdinInspector;
using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "CthulhusTears", menuName = "Game/Modifiers/Explosive/CthulhusTears")]
	public class CthulhusTearsModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public StatusEffectSO StatusEffectToRemove { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) =>
			new CthulhusTearsInstance(this, owner);
	}

	public class CthulhusTearsInstance : TowerModifierInstance
	{
		private readonly CthulhusTearsModifierSO _so;

		public CthulhusTearsInstance(CthulhusTearsModifierSO data, Tower owner)
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
