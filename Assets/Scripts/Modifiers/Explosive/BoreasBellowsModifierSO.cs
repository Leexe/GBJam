using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "BoreasBellows", menuName = "Game/Modifiers/Explosive/BoreasBellows")]
	public class BoreasBellowsModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float PushDistance { get; private set; } = 0.5f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new BoreasBellowsInstance(this, owner);
	}

	public class BoreasBellowsInstance : TowerModifierInstance
	{
		private readonly BoreasBellowsModifierSO _so;

		public BoreasBellowsInstance(BoreasBellowsModifierSO data, Tower owner)
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
					enemy.DisplaceAwayFrom(explosionCenter, _so.PushDistance);
				}
			}
		}
	}
}
