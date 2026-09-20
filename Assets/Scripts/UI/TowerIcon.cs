using UnityEngine;
using UnityEngine.UI;

public class TowerIcon : MonoBehaviour
{
	[SerializeField]
	private TowerType _towerType;

	[SerializeField]
	private Image _iconImage;

	private TowerSO _tower;
	public TowerSO Tower => _tower;
	public TowerType TowerType => _tower != null ? _tower.TowerType : _towerType;

	public void SetTower(TowerSO tower)
	{
		_tower = tower;
		_towerType = tower.TowerType;
		_iconImage.sprite = tower.Icon;
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}
}
