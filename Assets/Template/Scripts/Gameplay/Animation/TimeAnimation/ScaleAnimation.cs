using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class ScaleAnimation : TimeAnimationComponentBase<Transform>
	{
		[Tooltip("动画持续时间 (s)")]
		public float Duration;
		public Ease AnimationEasing;
		
		[Space]
		
		public Vector3 StartScale;
		
		[Tooltip("当 IsAdded 为 true 时该值为 StartScale + TargetScale\n否则作为为 EndScale 使用")] 
		public Vector3 TargetScale;
		
		[Space]
		
		public bool IsAdded;

		protected override void OnActiveAnimation()
		{
			var target = IsAdded ? StartScale + TargetScale : TargetScale;
			_tween = TargetObject.DOScale(target, Duration).SetEase(AnimationEasing);
		}

		protected override void OnResetAnimation()
		{
			Pause();
			_tween?.Kill();
			TargetObject.localScale = StartScale;
		}
		
		protected override void OnFinishAnimation()
		{
			var target = IsAdded ? StartScale + TargetScale : TargetScale;
			TargetObject.localScale = target;
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			float p = AnimLerpHelper.Evaluate(AnimationEasing, time, Duration);
			var target = IsAdded ? StartScale + TargetScale : TargetScale;
			TargetObject.localScale = Vector3.Lerp(StartScale, target, p);
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
	[CustomEditor(typeof(ScaleAnimation))]
	public class ScaleAnimationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (ScaleAnimation)target;

			if (!item.TargetObject) return;
			
			var transform = item.TargetObject.transform;
			
			if (GUILayout.Button("Preview Animation"))
			{
				item.PreviewAnimation();
			}
			
			if (GUILayout.Button("Get current local scale"))
			{
				item.StartScale = transform.localScale;
			}
		}
	}
#endif
}