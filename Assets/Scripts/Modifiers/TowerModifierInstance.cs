using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Modifiers
{
	public class TowerModifierInstance
	{
		public TowerModifierSO Data { get; }
		public Tower Owner { get; }

		protected readonly List<Modifier> AppliedModifiers = new();

		public TowerModifierInstance(TowerModifierSO data, Tower owner)
		{
			Data = data;
			Owner = owner;
		}

		public virtual void OnEquipped()
		{
			foreach (Modifier modifier in Data.StatModifiers)
			{
				AppliedModifiers.Add(Owner.Stats.AddModifier(modifier));
			}
		}

		public virtual void OnUnequipped()
		{
			foreach (Modifier modifier in AppliedModifiers)
			{
				Owner.Stats.RemoveModifier(modifier);
			}
			AppliedModifiers.Clear();
		}

		public virtual void Update(float deltaTime) { }

		public virtual bool CanTarget(Enemy target) => true;

		public virtual void OnAttack(Enemy target) { }

		public virtual void OnBeforeDealDamage(Enemy target, ref float damage) { }

		public virtual void OnAfterDealDamage(Enemy target, float damage) { }

		public virtual void OnProjectileCreated(Projectile projectile) { }

		public virtual void OnExplosionHit(Collider2D[] hits, Vector3 explosionCenter) { }
	}
}
