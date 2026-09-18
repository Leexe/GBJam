using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers
{
	[CreateAssetMenu(fileName = "ScorchedPowder", menuName = "Game/Modifiers/Projectile/ScorchedPowder")]
	public class ScorchedPowderModifierSO : TowerModifierSO
	{
		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float ContinuousFireThreshold { get; private set; } = 5f;

		[field: SerializeField]
		[field: TabGroup("Info", "Settings")]
		public float OverheatDuration { get; private set; } = 2f;

		public override TowerModifierInstance CreateInstance(Tower owner) => new ScorchedPowderInstance(this, owner);
	}

	public class ScorchedPowderInstance : TowerModifierInstance
	{
		private readonly ScorchedPowderModifierSO _so;
		private float _continuousFireTimer;
		private float _timeSinceLastAttack;

		public ScorchedPowderInstance(ScorchedPowderModifierSO data, Tower owner)
			: base(data, owner)
		{
			_so = data;
		}

		public override void Update(float deltaTime)
		{
			_timeSinceLastAttack += deltaTime;
			if (_timeSinceLastAttack > 1.2f)
			{
				_continuousFireTimer = 0f;
			}
		}

		public override void OnAttack(Enemy target)
		{
			_continuousFireTimer += _timeSinceLastAttack;
			_timeSinceLastAttack = 0f;

			if (_continuousFireTimer >= _so.ContinuousFireThreshold)
			{
				_continuousFireTimer = 0f;
				Owner.Stun(_so.OverheatDuration);
			}
		}
	}
}
