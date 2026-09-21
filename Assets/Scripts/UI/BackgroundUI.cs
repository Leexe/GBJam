using Animancer;
using UnityEngine;

public class BackgroundUI : MonoBehaviour
{
	[SerializeField]
	private AnimancerComponent _animancerComponent;

	private void OnEnable()
	{
		_animancerComponent.Play(GameManager.Instance.Level.BackgroundClip);
	}
}
