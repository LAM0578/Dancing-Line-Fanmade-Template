using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Gameplay.Animation
{
	public class RotateAnimation : TimeAnimationComponentBase<Transform>
	{
		[Tooltip("动画持续时间 (s)")]
		public float Duration;
		public Ease AnimationEasing;
		
		[Space]
		
		public Vector3 StartRotation;
		
		[Tooltip("当 IsAdded 为 true 时该值为 StartRotation + TargetRotation\n否则作为为 EndRotation 使用")] 
		public Vector3 TargetRotation;
		
		[Space]
		
		public bool IsLocal;
		public bool IsAdded;

		protected override void OnActiveAnimation()
		{
			var target = IsAdded ? StartRotation + TargetRotation : TargetRotation;
			_tween = IsLocal ?
				AnimTweenHelper.DOLocalRotate(TargetObject, target, Duration).SetEase(AnimationEasing) :
				AnimTweenHelper.DORotate(TargetObject, target, Duration).SetEase(AnimationEasing);
		}

		protected override void OnResetAnimation()
		{
			Pause();
			_tween?.Kill();
			if (IsLocal) TargetObject.localRotation = Quaternion.Euler(StartRotation);
			else TargetObject.rotation = Quaternion.Euler(StartRotation);
		}

		protected override void OnFinishAnimation()
		{
			var target = IsAdded ? StartRotation + TargetRotation : TargetRotation;
			if (IsLocal) TargetObject.localRotation = Quaternion.Euler(target);
			else TargetObject.rotation = Quaternion.Euler(target);
		}

		protected override void OnSetAnimationStatusByTime(float time)
		{
			float p = AnimLerpHelper.Evaluate(AnimationEasing, time, Duration);
			var target = IsAdded ? StartRotation + TargetRotation : TargetRotation;
			if (IsLocal) TargetObject.localRotation = Quaternion.Euler(Vector3.Lerp(StartRotation, target, p));
			else TargetObject.rotation = Quaternion.Euler(Vector3.Lerp(StartRotation, target, p));
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
	[CustomEditor(typeof(RotateAnimation))]
	public class RotateAnimationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var item = (RotateAnimation)target;

			if (!item.TargetObject) return;
			
			var transform = item.TargetObject.transform;
			
			if (GUILayout.Button("Preview Animation"))
			{
				item.PreviewAnimation();
			}

			if (GUILayout.Button("Get current rotation euler angles"))
			{
				item.StartRotation = transform.rotation.eulerAngles;
			}
			if (GUILayout.Button("Get current local rotation euler angles"))
			{
				item.StartRotation = transform.localRotation.eulerAngles;
			}
		}
	}
#endif
}