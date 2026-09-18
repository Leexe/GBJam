using System;
using UnityEngine;

namespace StatusEffects
{
	public class StatusEffect
	{
		public StatusEffectSO Data { get; }
		public Enemy Target { get; }
		public float Duration { get; private set; }
		public float RemainingDuration { get; private set; }
		public float DamagePerTick { get; private set; }
		public float TickInterval { get; }
		public bool IsActive { get; private set; }

		private float _tickTimer;

		public event Action<StatusEffect> OnExpired;
		public event Action<StatusEffect, float> OnTicked;

		public StatusEffect(StatusEffectSO data, Enemy target, float duration, float damagePerTick, float tickInterval)
		{
			Data = data;
			Target = target;
			Duration = duration;
			RemainingDuration = duration;
			DamagePerTick = damagePerTick;
			TickInterval = tickInterval;
			_tickTimer = TickInterval;
			IsActive = true;
		}

		public void Refresh(float duration, float damagePerTick)
		{
			Duration = Mathf.Max(Duration, duration);
			RemainingDuration = Mathf.Max(RemainingDuration, duration);
			DamagePerTick = Mathf.Max(DamagePerTick, damagePerTick);
			IsActive = true;
		}

		public void Update(float deltaTime)
		{
			if (!IsActive)
			{
				return;
			}

			RemainingDuration -= deltaTime;
			_tickTimer -= deltaTime;

			if (_tickTimer <= 0f)
			{
				_tickTimer = TickInterval;
				Tick();
			}

			if (RemainingDuration <= 0f)
			{
				End();
			}
		}

		private void Tick()
		{
			if (Target.CurrentHealth > 0f)
			{
				Target.TakeDamage(DamagePerTick);
				OnTicked?.Invoke(this, DamagePerTick);
			}
		}

		public void End()
		{
			if (!IsActive)
			{
				return;
			}

			IsActive = false;
			OnExpired?.Invoke(this);
		}

		public void Cancel()
		{
			End();
		}
	}
}
