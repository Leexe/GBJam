using System;
using System.Collections.Generic;
using Modifiers;
using UnityEngine;

[Serializable]
public class ModifierManager
{
	private readonly List<TowerModifierSO> _acquiredModifiers = new();

	public IReadOnlyList<TowerModifierSO> AcquiredModifiers => _acquiredModifiers;
	public Action<TowerModifierSO> OnModifierSelected;

	public void Initialize()
	{
		GridManager.Instance.OnTowerSpawned += HandleTowerSpawned;
	}

	public void Cleanup()
	{
		if (GridManager.Instance != null)
		{
			GridManager.Instance.OnTowerSpawned -= HandleTowerSpawned;
		}
	}

	public void SelectModifier(TowerModifierSO modifier)
	{
		_acquiredModifiers.Add(modifier);

		IReadOnlyList<Tower> towers = GridManager.Instance.ActiveTowers;
		for (int i = 0; i < towers.Count; i++)
		{
			if (IsTowerMatchingCategory(towers[i].Data.TowerType, modifier.Category))
			{
				towers[i].EquipModifier(modifier);
			}
		}

		OnModifierSelected?.Invoke(modifier);
	}

	private void HandleTowerSpawned(Tower tower)
	{
		for (int i = 0; i < _acquiredModifiers.Count; i++)
		{
			TowerModifierSO modifier = _acquiredModifiers[i];
			if (IsTowerMatchingCategory(tower.Data.TowerType, modifier.Category))
			{
				tower.EquipModifier(modifier);
			}
		}
	}

	private bool IsTowerMatchingCategory(TowerType towerType, ModifierTowerCategory category)
	{
		if (category == ModifierTowerCategory.Any)
		{
			return true;
		}

		return category switch
		{
			ModifierTowerCategory.Melee => towerType == TowerType.Melee,
			ModifierTowerCategory.Projectile => towerType == TowerType.Projectile,
			ModifierTowerCategory.Explosive => towerType == TowerType.Explosive,
			_ => false,
		};
	}
}
