using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "CrackedSextant", menuName = "Game/Modifiers/Explosive/CrackedSextant")]
	public class CrackedSextantModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float MinDistance { get; private set; } = 1f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float MaxDistance { get; private set; } = 4f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new CrackedSextantInstance(this, owner);
	}

	public class CrackedSextantInstance : TowerModifierInstance
	{
		private readonly CrackedSextantModifierSO _so;

		public CrackedSextantInstance(CrackedSextantModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void OnProjectileCreated(Projectile projectile)
		{
			projectile.SetDistanceDamageScaler(Owner.transform.position, _so.MinDistance, _so.MaxDistance);
		}
	}
}
