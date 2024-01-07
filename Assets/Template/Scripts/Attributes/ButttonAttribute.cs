using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DancingLineSample.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MethodButtonAttribute : PropertyAttribute
	{
		public string Name { get; }
		public bool RunInEditorMode { get; }

		public MethodButtonAttribute(string name, bool runInEditorMode = false)
		{
			Name = name;
			RunInEditorMode = runInEditorMode;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MonoBehaviour), true)]
	public class ButtonEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var mono = target as MonoBehaviour;

			var methods = mono.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			foreach (var method in methods)
			{
				var ba = (MethodButtonAttribute)System.Attribute.GetCustomAttribute(method, typeof(MethodButtonAttribute));
				if (ba != null && GUILayout.Button(ba.Name))
				{
					if (!ba.RunInEditorMode && !Application.isPlaying) return;
					method.Invoke(mono, null);
				}
			}
		}
	}
#endif
}