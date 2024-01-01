using System;
using System.Collections.Generic;
using DancingLineSample.Utility;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay.Objects
{
	[Serializable]
	public class ResetFog
	{
#pragma warning disable
		
		[SerializeField] private bool m_Enable;
		
		[Space] 
		
		[SerializeField] private bool m_EnableFog;
		[SerializeField] private Color m_FogColor;
		[SerializeField] private FogMode m_FogMode = FogMode.Exponential;
		
		[PropertyActive("m_FogMode", FogMode.Linear, CompareType.NotEqul)] 
		[SerializeField]
		private float m_Start;
		
		[PropertyActive("m_FogMode", FogMode.Linear, CompareType.NotEqul)] 
		[SerializeField]
		private float m_End;

		[PropertyActive("m_FogMode", FogMode.Linear)] 
		[SerializeField]
		private float m_Density;
		
#pragma warning restore
		
		public void ApplyFog()
		{
			if (!m_Enable) return;

			RenderSettings.fog = m_EnableFog;
			
			RenderSettings.fogMode = m_FogMode;
			RenderSettings.fogColor = m_FogColor;
			
			if (m_FogMode == FogMode.Linear)
			{
				RenderSettings.fogStartDistance = m_Start;
				RenderSettings.fogEndDistance = m_End;
			}
			else
			{
				RenderSettings.fogDensity = m_Density;
			}
		}
	}
	
	[Serializable]
	public class ResetMaerial
	{
		public ResetMaerial() { }

		public ResetMaerial(Material mat)
		{
			m_TargetMaterial = mat;
		}

		public ResetMaerial(Material mat, Color col)
		{
			m_TargetMaterial = mat;
			m_MaterialColor = col;
		}
		
		[Tooltip("指定材质球")]
		[SerializeField] 
		private Material m_TargetMaterial;
		
		[Tooltip("重置颜色")]
		[SerializeField] 
		private Color m_MaterialColor;

		private static readonly int _m_Color = Shader.PropertyToID("_Color");
		
		/// <summary>
		/// 应用特性到材质球中
		/// </summary>
		public void ApplyProperty()
		{
			if (!m_TargetMaterial) return;
			m_TargetMaterial.SetColor(_m_Color, m_MaterialColor);
		}
	}
	
	[Serializable]
	public class ResetMaterialAdvanced
	{
		public ResetMaterialAdvanced() { }

		public ResetMaterialAdvanced(Material mat)
		{
			m_TargetMaterial = mat;
		}

		public ResetMaterialAdvanced(Material mat, List<MaterialProperty> properties)
		{
			m_TargetMaterial = mat;
			m_MaterialProperties = properties;
		}
		
		[Serializable]
		public class MaterialProperty
		{
			public enum PropertyType
			{
				Float,
				Int,
				Vector,
				Color,
			}

#pragma warning disable
			
			[Tooltip("特性名称")]
			[SerializeField] private string m_PropertyName;
			
			[Tooltip("特性类型")]
			[SerializeField] private PropertyType m_PropertyType;

			[PropertyActive("m_PropertyType", PropertyType.Float, CompareType.NotEqul)] 
			[Tooltip("浮点型特性值")]
			[SerializeField]
			private float m_FloatValue;
			
			[PropertyActive("m_PropertyType", PropertyType.Int, CompareType.NotEqul)] 
			[Tooltip("整型特性值")]
			[SerializeField]
			private int m_IntValue;
			
			[PropertyActive("m_PropertyType", PropertyType.Vector, CompareType.NotEqul)] 
			[Tooltip("向量型特性值")]
			[SerializeField]
			private Vector4 m_VectorValue;
			
			[PropertyActive("m_PropertyType", PropertyType.Color, CompareType.NotEqul)] 
			[Tooltip("颜色特性值")]
			[SerializeField]
			private Color m_ColorValue;
			
#pragma warning restore

			/// <summary>
			/// 应用特性到指定材质球上
			/// </summary>
			/// <param name="material">指定材质球</param>
			public void ApplyMaterialProperty(Material material)
			{
				if (m_PropertyType == PropertyType.Float)
					material.SetFloat(m_PropertyName, m_FloatValue);
				else if (m_PropertyType == PropertyType.Int)
					material.SetInt(m_PropertyName, m_IntValue);
				else if (m_PropertyType == PropertyType.Vector)
					material.SetVector(m_PropertyName, m_VectorValue);
				else if (m_PropertyType == PropertyType.Color)
					material.SetColor(m_PropertyName, m_ColorValue);
			}
		}
		
		[Tooltip("指定材质球")]
		[SerializeField] 
		private Material m_TargetMaterial;
		
		[Tooltip("需要重置的材质球特性")]
		[SerializeField] 
		private List<MaterialProperty> m_MaterialProperties = new List<MaterialProperty>();
		
		/// <summary>
		/// 应用特性到材质球中
		/// </summary>
		public void ApplyProperties()
		{
			if (!m_TargetMaterial) return;
			foreach (var property in m_MaterialProperties)
			{
				property.ApplyMaterialProperty(m_TargetMaterial);
			}
		}
	}

	[Serializable]
	public class ResetGameObject
	{
		[Serializable]
		public struct ResetVector3
		{
			public Vector3 Value;
			public bool IsLocal;
		}

#pragma warning disable
		
		[Tooltip("指定 GameObject 对象")]
		[SerializeField] 
		private GameObject m_GameObject;
		
		[Tooltip("是否在场景中显示")]
		[SerializeField] 
		private bool m_Active;
		
		[Tooltip("重置位置")]
		[SerializeField] 
		private ResetVector3 m_ResetPosition;
		
		[Tooltip("重置旋转")]
		[SerializeField] 
		private ResetVector3 m_ResetRotation;
		
		[Tooltip("重置缩放")]
		[SerializeField] 
		private Vector3 m_ResetScale;
		
#pragma warning restore
		
		/// <summary>
		/// 应用重置状态到指定 GameObject 中
		/// </summary>
		public void Apply()
		{
			if (!m_GameObject) return;
			m_GameObject.SetActive(m_Active);
			var trans = m_GameObject.transform;
			
			// Position
			if (m_ResetPosition.IsLocal)
				trans.localPosition = m_ResetPosition.Value;
			else
				trans.position = m_ResetPosition.Value;
			
			// Rotation
			if (m_ResetRotation.IsLocal)
				trans.localRotation = Quaternion.Euler(m_ResetRotation.Value);
			else
				trans.rotation = Quaternion.Euler(m_ResetRotation.Value);
			
			// Scale
			trans.localScale = m_ResetScale;
		}
		
	}
	
	[Serializable]
	public class ResetObjects
	{
#pragma warning disable
		
		[Tooltip("雾效重置")]
		[SerializeField] 
		private ResetFog m_Fog;
		
		[Tooltip("材质球重置 (仅颜色)")]
		[SerializeField] 
		private List<ResetMaerial> m_Materials = new List<ResetMaerial>();
		
		[Tooltip("材质球重置 (完整)")]
		[SerializeField] 
		private List<ResetMaterialAdvanced> m_AdvancedMaterials = new List<ResetMaterialAdvanced>();
		
		[Tooltip("GameObject 重置")]
		[SerializeField] 
		private List<ResetGameObject> m_GameObjects = new List<ResetGameObject>();
		
#pragma warning restore

		/// <summary>
		/// 重置所有指定对象的状态为对应状态
		/// </summary>
		public void ApplyReset()
		{
			m_Fog.ApplyFog();
			
			foreach (var mat in m_Materials)
				mat.ApplyProperty();
			
			foreach (var mat in m_AdvancedMaterials)
				mat.ApplyProperties();
			
			foreach (var obj in m_GameObjects)
				obj.Apply();
		}
		
	}
}