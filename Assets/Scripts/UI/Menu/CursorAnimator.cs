using PrimeTween;
using UnityEngine;

public class CursorAnimator : MonoBehaviour
{
	[SerializeField]
	private RectTransform _target;

	[SerializeField]
	private float _distance = 2f;

	[SerializeField]
	private float _duration = 0.35f;

	[SerializeField]
	private Ease _ease = Ease.InOutSine;

	private Sequence _cursorTween;
	private float _originX;

	public RectTransform Target => _target;

	private void Awake()
	{
		if (_target == null)
		{
			_target = GetComponent<RectTransform>();
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void MoveTo(RectTransform targetItem, float xOffset = 0f)
	{
		Stop();
		float leftEdge = targetItem.anchoredPosition.x - (targetItem.rect.width * targetItem.pivot.x);
		_target.anchoredPosition = new Vector2(leftEdge + xOffset, targetItem.anchoredPosition.y);
		Play();
	}

	public void SetPosition(Vector2 position)
	{
		Stop();
		_target.anchoredPosition = position;
		Play();
	}

	public void Play()
	{
		Stop();
		_originX = _target.anchoredPosition.x;
		_cursorTween = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo, useUnscaledTime: true)
			.Chain(Tween.UIAnchoredPositionX(_target, _originX - _distance, _duration, _ease))
			.Chain(Tween.UIAnchoredPositionX(_target, _originX, _duration, _ease));
	}

	public void Stop()
	{
		_cursorTween.Stop();
	}
}
