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

		_originX = _target.anchoredPosition.x;
	}

	private void OnEnable()
	{
		Play();
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
		_originX = _target.anchoredPosition.x;
		Play();
	}

	public void SetPosition(Vector2 position)
	{
		Stop();
		_target.anchoredPosition = position;
		_originX = position.x;
		Play();
	}

	public void Play()
	{
		if (_cursorTween.isAlive)
		{
			return;
		}

		Stop();
		_cursorTween = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo, useUnscaledTime: true)
			.Chain(Tween.UIAnchoredPositionX(_target, _originX - _distance, _duration, _ease))
			.Chain(Tween.UIAnchoredPositionX(_target, _originX, _duration, _ease));
	}

	public void Stop()
	{
		_cursorTween.Stop();
		_target.anchoredPosition = new Vector2(_originX, _target.anchoredPosition.y);
	}
}
