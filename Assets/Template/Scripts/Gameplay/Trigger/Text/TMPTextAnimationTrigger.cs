using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// By NCat
public class TMPTextAnimationTrigger : MonoBehaviour
{
#pragma warning disable
	
	[SerializeField] private TMP_Text TextComponent;
	[Space]
	[SerializeField] private List<TextAnimationItem> AnimationList;
	
#pragma warning restore

	private float currentTime;
	private string lastContent;
	private bool actived;

	private float lastTriggerTime;
	
	[Serializable]
	// 这里使用 class 是因为我们需要设置 Actived 的状态用于判断
	public class TextAnimationItem
	{
		public float TriggerTime;
		public string Content;
		
		public bool Actived { get; set; }
		protected internal bool IsLast { get; set; }
	}

	private void Start()
	{
		lastContent = TextComponent.text;
		AnimationList = AnimationList.OrderBy(t => t.TriggerTime).ToList();
		if (AnimationList.Count <= 0) return;
		lastTriggerTime = AnimationList.Last().TriggerTime;
	}

	private void Update()
	{
		if (!actived) return;
		
		string content = lastContent;
		foreach (var anim in AnimationList)
		{
			// 判断状态
			if (anim.Actived || currentTime < anim.TriggerTime) break;
			anim.Actived = true;
			content = anim.Content;
		}
		
		TextComponent.text = content;

		if (currentTime > lastTriggerTime)
		{
			actived = false;
			return;
		}

		currentTime += Time.deltaTime;
	}

	/// <summary>
	/// 重置状态
	/// </summary>
	public void ResetStatus()
	{
		actived = false;
		TextComponent.text = lastContent;
	}

	/// <summary>
	/// 当碰撞体进入触发器（自身）时触发
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerEnter(Collider other)
	{
		// 这里的 Player 换成你线的标签
		if (!other.gameObject.CompareTag("Player")) return;
		actived = true;
	}
}
