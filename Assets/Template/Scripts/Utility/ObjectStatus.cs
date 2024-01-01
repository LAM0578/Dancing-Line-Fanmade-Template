using System;
using UnityEngine;

namespace DancingLineSample.Utility
{
	public class ObjectStatus<T>
	{
		public T StartValue = default;
		public T EndValue = default;
	}
	
	[Serializable] public class Vector2Status : ObjectStatus<Vector2> { }
	[Serializable] public class Vector3Status : ObjectStatus<Vector3> { }
	
	[Serializable] public class FloatStatus : ObjectStatus<float> { }
	[Serializable] public class ColorStatus : ObjectStatus<Color> { }
	[Serializable] public class Vector4Status : ObjectStatus<Vector4> { }
	
}