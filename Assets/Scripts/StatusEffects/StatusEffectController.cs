using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatusEffects
{
	public class StatusEffectController : MonoBehaviour
	{
		private Enemy _enemy;
		private readonly Dictionary<StatusEffectSO, StatusEffect> _activeEffects = new();
		private readonly List<StatusEffectSO> _expiredEffects = new();

		public IReadOnlyDictionary<StatusEffectSO, StatusEffect> ActiveEffects => _activeEffects;

		public event Action<StatusEffect> OnStatusApplied;
		public event Action<StatusEffect> OnStatusEnded;
		public event Action<StatusEffect, float> OnStatusTicked;

		private void Awake()
		{
			_enemy = GetComponent<Enemy>();
		}

		public void Initialize()
		{
			ClearStatusEffects();
		}

		public bool HasStatusEffect(StatusEffectSO effectSO) => _activeEffects.ContainsKey(effectSO);

		public void ApplyStatusEffect(StatusEffectSO effectSO, float duration = -1f, float damagePerTick = -1f)
		{
			float effDuration = duration > 0f ? duration : effectSO.Duration;
			float effDmg = damagePerTick > 0f ? damagePerTick : effectSO.DamagePerTick;

			if (_activeEffects.TryGetValue(effectSO, out StatusEffect existing))
			{
				existing.Refresh(effDuration, effDmg);
			}
			else
			{
				StatusEffect newEffect = effectSO.CreateInstance(_enemy, effDuration, effDmg);
				newEffect.OnTicked += HandleEffectTicked;
				_activeEffects[effectSO] = newEffect;
				OnStatusApplied?.Invoke(newEffect);
			}
		}

		public void RemoveStatusEffect(StatusEffectSO effectSO)
		{
			if (_activeEffects.TryGetValue(effectSO, out StatusEffect effect))
			{
				effect.OnTicked -= HandleEffectTicked;
				effect.Cancel();
				_activeEffects.Remove(effectSO);
				OnStatusEnded?.Invoke(effect);
			}
		}

		public void ClearStatusEffects()
		{
			foreach (StatusEffect effect in _activeEffects.Values)
			{
				effect.OnTicked -= HandleEffectTicked;
				effect.Cancel();
				OnStatusEnded?.Invoke(effect);
			}
			_activeEffects.Clear();
		}

		public void UpdateEffects(float deltaTime)
		{
			_expiredEffects.Clear();
			foreach (KeyValuePair<StatusEffectSO, StatusEffect> kvp in _activeEffects)
			{
				kvp.Value.Update(deltaTime);
				if (!kvp.Value.IsActive)
				{
					_expiredEffects.Add(kvp.Key);
				}
			}

			foreach (StatusEffectSO key in _expiredEffects)
			{
				if (_activeEffects.TryGetValue(key, out StatusEffect expired))
				{
					expired.OnTicked -= HandleEffectTicked;
					_activeEffects.Remove(key);
					OnStatusEnded?.Invoke(expired);
				}
			}
		}

		private void HandleEffectTicked(StatusEffect effect, float damage)
		{
			OnStatusTicked?.Invoke(effect, damage);
		}
	}
}
