using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "IncendiaryShells", menuName = "Game/Modifiers/Explosive/IncendiaryShells")]
	public class IncendiaryShellsModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		public StatusEffectSO BurnEffectSO { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) => new IncendiaryShellsInstance(this, owner);
	}

	public class IncendiaryShellsInstance : TowerModifierInstance
	{
		private readonly IncendiaryShellsModifierSO _so;

		public IncendiaryShellsInstance(IncendiaryShellsModifierSO data, Tower owner)
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
					enemy.StatusController.ApplyStatusEffect(_so.BurnEffectSO);
				}
			}
		}
	}
}
