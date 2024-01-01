﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DancingLineSample.Gameplay.Objects;
using DancingLineSample.Gameplay.Trigger;
using DancingLineSample.UI;
using DancingLineSample.Utility;
using DG.Tweening;
using IngameDebugConsole;
using UnityEngine;
using UnityEngine.Serialization;

namespace DancingLineSample.Gameplay
{
	public enum PlayerStatus
	{
		Prepare,
		Ready,
		ReadyAtCheckpoint,
		Playing,
		Pause,
		Win,
		Dead
	}
	public class GameplayManager : Singleton<GameplayManager>
	{
		public MainLine Line;
		[Space]
		public AudioSource MusicSource;
		public int AudioOffset;

		private float _fTiming;
		public bool IsPlaying { get; private set; }
		public PlayerStatus LineStatus { get; internal set; }
		public bool AllowTurn { get; set; } = true;
		
		private const KeyCode TurnKeybordKey = KeyCode.Space;
		private const KeyCode TurnMouseKey = KeyCode.Mouse0;

		private static bool _isClick => Input.GetKeyDown(TurnKeybordKey) || Input.GetKeyDown(TurnMouseKey);
		
		private static CancellationTokenSource m_Restart_cts = new CancellationTokenSource();
		private static CancellationTokenSource m_Prepare_cts = new CancellationTokenSource();
		private static CancellationTokenSource m_Checkpoint_cts = new CancellationTokenSource();

		private List<CollectCubeTrigger> _currentLevelCollectCubes = new List<CollectCubeTrigger>();
		private List<CheckpointTrigger> _currentLevelCheckpoints = new List<CheckpointTrigger>();
		
		private ResetObjects _resetObjects;
			
		/// <summary>
		/// 检测并更新游玩状态
		/// </summary>
		private void Update()
		{
			if (LineStatus == PlayerStatus.Prepare || LineStatus == PlayerStatus.Win) return;

			if (_isClick && AllowTurn)
			{
				if (LineStatus == PlayerStatus.Ready)
				{
					Play();
					return;
				}
				
				if (LineStatus == PlayerStatus.ReadyAtCheckpoint)
				{
					Play(_fTiming);
					if (!LastCheckpoint) return;
					LastCheckpoint.CheckpointObject.DoLostEffect();
					return;
				}
				
				if (LineStatus == PlayerStatus.Pause)
				{
					Continue();
					return;
				}

				if (!AutoPlayManager.Instance.EnableAuto)
				{
					Line.Turn();
				}
			}

			if (LineStatus == PlayerStatus.Playing)
			{
				if (!Application.isFocused)
				{
					Pause();
				}
				_fTiming += Time.deltaTime;
			}
		}

		public int Timing
		{
			get => (int)(_fTiming * 1000f);
			set => _fTiming = value / 1000f;
		}
		
		public float CurrentProgress => Mathf.Clamp01(_fTiming / MusicSource.clip.length);
		public int CurrentTiming => Timing - AudioOffset;
		
		private LevelGameplayData CurrentGameplayData { get; set; }
		public LevelGameplayData PlayingGameplayData = new LevelGameplayData();

		public CheckpointTrigger LastCheckpoint { get; set; }
		

		private void ResetCollectCubeTriggers()
		{
			foreach (var trigger in _currentLevelCollectCubes)
			{
				trigger.ResetTrigger();
			}
		}
		
		private void ResetCheckpointTriggers()
		{
			foreach (var trigger in _currentLevelCheckpoints)
			{
				trigger.ResetTrigger();
			}
		}

		private void GetCollectItemsInScene()
		{
			_currentLevelCollectCubes = Resources.FindObjectsOfTypeAll<CollectCubeTrigger>().ToList();
			_currentLevelCheckpoints = Resources.FindObjectsOfTypeAll<CheckpointTrigger>().ToList();
		}

		private void OnRestart()
		{
			IsPlaying = false;
			Line.AllowDead = true;
			AllowTurn = true;
			LineStatus = PlayerStatus.Prepare;
			
			Line.Restart();
			MusicSource.Stop();
			_fTiming = 0;
			MusicSource.time = _fTiming;
			_resetObjects?.ApplyReset();
			
			CameraManager.Instance.ResetStatus();
			AutoPlayManager.Instance.ResetAutoHitDataStatus();
			AnimationManager.Instance.ResetAnimations(false, true);

			ResetCollectCubeTriggers();
			ResetCheckpointTriggers();
		}

		private void OnPlay()
		{
			Line.AllowDead = true;
			AllowTurn = true;
			MusicSource.volume = 1;
		}

		private async UniTaskVoid PrepareTask()
		{
			bool isCanceled = await UniTask
				.Delay(0, cancellationToken: m_Prepare_cts.Token)
				.SuppressCancellationThrow();
			
			if (isCanceled) return;

			LastCheckpoint = null;
			UIManager.Instance.ChangeResultUI(false);
			UIManager.Instance.DOFakeFog();
			
			await UniTask.Delay(500); // 0.5s
			
			OnRestart();
			LineStatus = PlayerStatus.Prepare;
			UIManager.Instance.ChangeReadyUI(true);
			UIManager.Instance.ResultUI.ChangeButtonStatus(false);
		}

		private async UniTaskVoid RestartTask()
		{
			bool isCanceled = await UniTask
				.Delay(0, cancellationToken: m_Restart_cts.Token)
				.SuppressCancellationThrow();
			
			if (isCanceled) return;
			
			PlayingGameplayData = new LevelGameplayData();

			LastCheckpoint = null;
			UIManager.Instance.ChangeResultUI(false);
			UIManager.Instance.DOFakeFog();
			
			await UniTask.Delay(500); // 0.5s
			
			OnRestart();

			await UniTask.Delay(500); // 0.5s

			LineStatus = PlayerStatus.Ready;
		}

		private async UniTaskVoid CheckpointTask(CheckpointTrigger checkpoint)
		{
			bool isCanceled = await UniTask
				.Delay(0, cancellationToken: m_Checkpoint_cts.Token)
				.SuppressCancellationThrow();
			
			if (isCanceled) return;
			
			IsPlaying = false;
			
			UIManager.Instance.ChangeResultUI(false);
			UIManager.Instance.DOFakeFog();
			
			await UniTask.Delay(500); // 0.5s
			
			Line.Pause();
			OnPlay();
			
			AutoPlayManager.Instance.ResetAutoHitDataStatus();
			AnimationManager.Instance.ResetAnimations(true, false);
			AnimationManager.Instance.SetAnimationStatusByTiming(checkpoint.CheckpointTime);

			AutoPlayManager.Instance.ActiveAutoHitDatasByTiming(checkpoint.CheckpointTime);

			PlayingGameplayData.CollectCount = 0;
			ResetCollectCubeTriggers();
			
			Line.ClearLineObjects();
			Line.ChangeStatusByCheckpoint(checkpoint);
			_fTiming = checkpoint.CheckpointTime / 1000f;
			MusicSource.time = _fTiming;
			
			if (checkpoint.OverrideCameraResetStatus)
				CameraManager.Instance.ResetStatus(checkpoint.CameraResetStatus);
			else
				CameraManager.Instance.ResetStatus();
			
			checkpoint.ResetObjects.ApplyReset();
			
			LineStatus = PlayerStatus.ReadyAtCheckpoint;
		}

		/// <summary>
		/// 设置为已准备状态
		/// </summary>
		public void Ready()
		{
			LineStatus = PlayerStatus.Ready;
			UIManager.Instance.ChangeReadyUI(false);
		}

		/// <summary>
		/// 设置为待准备状态
		/// </summary>
		public void Prepare()
		{
			m_Prepare_cts.Cancel();
			m_Prepare_cts.Dispose();
			m_Prepare_cts = new CancellationTokenSource();
			PrepareTask().Forget();
		}

		/// <summary>
		/// 返回待准备状态
		/// </summary>
		public void BackToPrepare()
		{
			Prepare();
			PlayingGameplayData = new LevelGameplayData();
		}

		/// <summary>
		/// 开始游戏
		/// </summary>
		/// <param name="fTiming">内置时间</param>
		public void Play(float fTiming = 0)
		{
			OnPlay();
			_fTiming = fTiming;
			IsPlaying = true;
			LineStatus = PlayerStatus.Playing;
			Line.Play();
			MusicSource.PlayDelayed(AudioOffset / 1000f);
		}

		/// <summary>
		/// 暂停游戏 (一般和 GameplayManager.Continue() 组合)
		/// </summary>
		public void Pause()
		{
			IsPlaying = false;
			LineStatus = PlayerStatus.Pause;
			Line.Pause();
			MusicSource.Pause();
			CameraManager.Instance.PauseTriggers();
			AnimationManager.Instance.Pause();
		}

		/// <summary>
		/// 重试
		/// </summary>
		public void Restart()
		{
			m_Restart_cts.Cancel();
			m_Restart_cts.Dispose();
			m_Restart_cts = new CancellationTokenSource();
			RestartTask().Forget();
		}

		/// <summary>
		/// 继续游戏 (一般和 GameplayManager.Pause() 组合)
		/// </summary>
		public void Continue()
		{
			IsPlaying = true;
			LineStatus = PlayerStatus.Playing;
			Line.Continue();
			MusicSource.Play();
			MusicSource.time = _fTiming;
			CameraManager.Instance.ContinueTriggers();
			AnimationManager.Instance.Continue();
		}

		/// <summary>
		/// 在指定检查点继续游戏
		/// </summary>
		/// <param name="checkpoint">指定检查点</param>
		public void ContinueByCheckpoint(CheckpointTrigger checkpoint)
		{
			m_Checkpoint_cts.Cancel();
			m_Checkpoint_cts.Dispose();
			m_Checkpoint_cts = new CancellationTokenSource();
			CheckpointTask(checkpoint).Forget();
		}

		/// <summary>
		/// 在上个检查点继续游戏
		/// </summary>
		public void ContinueAtLastCheckpoint()
		{
			if (LastCheckpoint == null) return;
			LastCheckpoint.ContinueInThisCheckpoint();
		}

		/// <summary>
		/// 载入关卡
		/// </summary>
		/// <param name="data">关卡数据</param>
		public void LoadLevel(LevelData data)
		{
			// TODO: 载入场景并加载关卡数据
			// 载入场景暂时还没写
			CurrentGameplayData = data.GameplayData ?? new LevelGameplayData();
			MusicSource.clip = data.Music;
			Line.StartForward = data.StartForward;
			Line.TurnForward = data.TurnForward;
			Line.ResetPosition = data.ResetPosition;
			Line.transform.position = data.ResetPosition;
			_resetObjects = data.ResetObjects;
			data.ResetObjects.ApplyReset();
			ResultManager.Instance.LoadLevel(data);
			GetCollectItemsInScene();
		}
		
		/// <summary>
		/// 结束游戏并结算结果
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="status"></param>
		public void ChangeToResult(float progress, PlayerStatus status)
		{
			PlayingGameplayData.Progress = progress;
			
			ResultManager.Instance.SetResult(PlayingGameplayData);
			UpdateLevelData();

			AllowTurn = false;
			Line.AllowDead = false;
			LineStatus = status;
			
			CameraManager.Instance.UpdateFollowPos = false;
			UIManager.Instance.ChangeResultUI(true);
			FadeOutMusic();
		}

		/// <summary>
		/// 计算结果并设置结算 UI 元素
		/// </summary>
		public void CalculateResult()
		{
			float progress = Mathf.Clamp01(_fTiming / MusicSource.clip.length);
			PlayingGameplayData.Progress = progress;
			ResultManager.Instance.SetResult(PlayingGameplayData);
			UpdateLevelData();
		}

		/// <summary>
		/// 更新当前游玩数据到当前关卡数据
		/// </summary>
		public void UpdateLevelData()
		{
			var currentData = CurrentGameplayData;
			var playingData = PlayingGameplayData;

			if (playingData.CollectCount > currentData.CollectCount) 
				currentData.CollectCount = playingData.CollectCount;
			
			if (playingData.CheckpointCount > currentData.CheckpointCount) 
				currentData.CheckpointCount = playingData.CheckpointCount;
			
			if (playingData.Progress > currentData.Progress) 
				currentData.Progress = playingData.Progress;
		}

		public void FadeOutMusic()
		{
			MusicSource.DOFade(0, 1f).OnComplete(() =>
			{
				MusicSource.Pause();
				MusicSource.Stop();
			});
		}
	}
}