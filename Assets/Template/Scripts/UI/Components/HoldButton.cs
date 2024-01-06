using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DancingLineSample.UI.Components
{
	public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image TargetImage;

		[Space(10)]
		public Color DefaultColor = new Color(1f, 1f, 1f, 1);
		public Color PressingColor = new Color(1f, 1f, 1f, 0.5f);
		public Color DisableColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);
		public float ColorChangeDuration = 0.1f;

		[Space(10)]
		public bool Disable;
		public float HoldTargetTime = 0.5f;
		public float UpdateInterval = 0f;

		[Space(10)]
		public UnityEvent OnPressing = new UnityEvent();

		private bool _allowUpdate;
		private float _onPointerDownTime;
		private bool _lastDisableStatus;
		private float _lastUpdateTime;
		private bool _isEnterPointer;

		private void Update()
		{
			if (_allowUpdate && _isEnterPointer && OnPressing != null)
			{
				float pressTime = Time.time - _onPointerDownTime;
				if (pressTime > HoldTargetTime)
				{
					if (UpdateInterval > 0)
					{
						float interval = Time.time - _lastUpdateTime;
						if (interval >= UpdateInterval)
						{
							_lastUpdateTime = Time.time;
							OnPressing.Invoke();
						}
					}
					else OnPressing.Invoke();
				}
			}
			if (_lastDisableStatus != Disable)
			{
				_lastDisableStatus = Disable;
				TargetImage.DOColor(Disable ? DisableColor : DefaultColor, ColorChangeDuration);
			}
		}

		private void OnStatusChange()
		{
			TargetImage.DOColor(_allowUpdate && _isEnterPointer ?
				PressingColor : DefaultColor, ColorChangeDuration);
		}

		public void OnPointerDown(PointerEventData data)
		{
			_onPointerDownTime = Time.time;
			if (Disable) return;
			_allowUpdate = true;
			OnStatusChange();
			OnPressing?.Invoke();
		}
		public void OnPointerUp(PointerEventData data)
		{
			_allowUpdate = false;
			OnStatusChange();
		}

		public void OnPointerEnter(PointerEventData data)
		{
			_isEnterPointer = true;
			OnStatusChange();
		}
		public void OnPointerExit(PointerEventData data)
		{
			_isEnterPointer = false;
			OnStatusChange();
		}

	}
}