using System;

[Serializable]
public struct EnemyModifier
{
	public float SpeedMultiplier;
	public float HealthMultiplier;
	public float DamageMultiplier;
	public float GoldMultiplier;
	public float ScaleMultiplier;

	public static EnemyModifier Default =>
		new()
		{
			SpeedMultiplier = 1f,
			HealthMultiplier = 1f,
			DamageMultiplier = 1f,
			GoldMultiplier = 1f,
			ScaleMultiplier = 1f,
		};

	public float EffectiveSpeed => SpeedMultiplier > 0f ? SpeedMultiplier : 1f;
	public float EffectiveHealth => HealthMultiplier > 0f ? HealthMultiplier : 1f;
	public float EffectiveDamage => DamageMultiplier > 0f ? DamageMultiplier : 1f;
	public float EffectiveGold => GoldMultiplier > 0f ? GoldMultiplier : 1f;
	public float EffectiveScale => ScaleMultiplier > 0f ? ScaleMultiplier : 1f;
}
