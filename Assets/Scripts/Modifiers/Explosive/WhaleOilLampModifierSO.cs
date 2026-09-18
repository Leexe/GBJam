using Sirenix.OdinInspector;
using StatusEffects;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "WhaleOilLamp", menuName = "Game/Modifiers/Explosive/WhaleOilLamp")]
	public class WhaleOilLampModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public StatusEffectSO BurnEffectSO { get; private set; }

		public override TowerModifierInstance CreateInstance(Tower owner) => new WhaleOilLampInstance(this, owner);
	}

	public class WhaleOilLampInstance : TowerModifierInstance
	{
		private readonly WhaleOilLampModifierSO _so;

		public WhaleOilLampInstance(WhaleOilLampModifierSO data, Tower owner)
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
