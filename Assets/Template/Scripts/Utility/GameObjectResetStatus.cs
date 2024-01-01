using System;
using UnityEngine;

namespace DancingLineSample.Utility
{
	[Serializable]
	public class GameObjectResetStatus
	{
		public Vector3 ResetPosition;
		public Vector3 ResetRotation;
	}

	[Serializable]
	public class CameraResetStatus
	{
		public Vector3 ResetOffset;
		public Vector3 ResetRotation;
		public float ResetFieldOfView;
	}
}