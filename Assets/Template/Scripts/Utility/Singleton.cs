using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineSample.Utility
{
	
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		public static T Instance { get; private set; }
		
		private void Awake()
		{
			Instance = this as T;
			OnAwake();
		}

		/// <summary>
		/// 当加载脚本实例时调用
		/// </summary>
		protected virtual void OnAwake() {}
	}
}
