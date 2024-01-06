using System;
using UnityEngine;

namespace DancingLineSample.Objects
{
	public class ObjectStatus<T>
	{
		public T StartValue = default;
		public T EndValue = default;
	}
	
	// Unity Vector
	[Serializable] public class Vector2Status : ObjectStatus<Vector2> { }
	[Serializable] public class Vector3Status : ObjectStatus<Vector3> { }
	[Serializable] public class Vector4Status : ObjectStatus<Vector4> { }
	
	// Number
	[Serializable] public class FloatStatus : ObjectStatus<float> { }
	
	// Unity Objects
	[Serializable] public class ColorStatus : ObjectStatus<Color> { }
	
}