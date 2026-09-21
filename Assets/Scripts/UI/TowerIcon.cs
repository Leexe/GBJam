using UnityEngine;
using UnityEngine.UI;

public class TowerIcon : MonoBehaviour
{
	[SerializeField]
	private TowerSO _tower;

	[SerializeField]
	private TowerType _towerType;

	[SerializeField]
	private Image _iconImage;

	public TowerSO Tower => _tower;
	public TowerType TowerType => _tower != null ? _tower.TowerType : _towerType;

	private void Awake()
	{
		if (_tower != null)
		{
			SetTower(_tower);
		}
	}

	public void SetTower(TowerSO tower)
	{
		_tower = tower;
		_towerType = tower.TowerType;
		_iconImage.sprite = tower.Icon;
	}

	public void SetActive(bool active)
	{
		_iconImage.gameObject.SetActive(active);
	}

	private void OnValidate()
	{
		if (_tower != null && _iconImage != null)
		{
			SetTower(_tower);
		}
	}
}
