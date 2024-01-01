using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class MoveAnimation : TimeAnimationComponentBase<Transform>
	{
		[Tooltip("动画持续时间 (s)")]
		public float Duration;
		public Ease AnimationEasing;
		
		[Space]
		
		public Vector3 StartPosition;
		
		[Tooltip("当 IsAdded 为 true 时该值为 StartPosition + TargetPosition\n否则作为为 EndPosition 使用")] 
		public Vector3 TargetPosition;
		
		[Space]
		
		public bool IsLocal;
		public bool IsAdded;

		protected override void OnActiveAnimation()
		{
			var target = IsAdded ? StartPosition + TargetPosition : TargetPosition;
			_tween = IsLocal ?
				TargetObject.DOLocalMove(target, Duration).SetEase(AnimationEasing) :
				TargetObject.DOMove(target, Duration).SetEase(AnimationEasing);
		}

		protected override void OnResetAnimation()
		{
			Pause();
			_tween?.Kill();
			if (IsLocal) TargetObject.localPosition = StartPosition;
			else TargetObject.position = StartPosition;
		}
		
		protected override void OnFinishAnimation()
		{
			var target = IsAdded ? StartPosition + TargetPosition : TargetPosition;
			if (IsLocal) TargetObject.localPosition = target;
			else TargetObject.position = target;
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			float p = AnimLerpHelper.Evaluate(AnimationEasing, time, Duration);
			var target = IsAdded ? StartPosition + TargetPosition : TargetPosition;
			if (IsLocal) TargetObject.localPosition = Vector3.Lerp(StartPosition, target, p);
			else TargetObject.position = Vector3.Lerp(StartPosition, target, p);
		}

		protected override void OnContinueByElapsedTime()
		{
			OnActiveAnimation();
			if (_tween == null) return;
			_tween.fullPosition = _elapsedTime;
		}

		public override void Pause()
		{
			_tween?.Pause();
		}

		public override void Continue()
		{
			_tween?.Play();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MoveAnimation))]
	public class MoveAnimationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (MoveAnimation)target;

			if (!item.TargetObject) return;
			
			var transform = item.TargetObject.transform;
			
			if (GUILayout.Button("Preview Animation"))
			{
				item.PreviewAnimation();
			}

			if (GUILayout.Button("Get current position"))
			{
				item.StartPosition = transform.position;
			}
			if (GUILayout.Button("Get current local position"))
			{
				item.StartPosition = transform.localPosition;
			}
		}
	}
#endif
}