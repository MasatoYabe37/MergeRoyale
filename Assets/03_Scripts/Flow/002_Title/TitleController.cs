using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : SceneControllerBase
{
	private bool _IsStarted = false;

	protected override void doStart()
	{
		_Phase.registerPhase(updateFadeIn, startFadeIn);
		_Phase.registerPhase(waitTouch);
		_Phase.registerPhase(updateFadeOut, startFadeOut);
		_Phase.registerPhase(updateTransition, startTransition);

		// ゲームオブジェクトインスタンスを取得
		var canvas = GameObject.Find("Canvas");
		var startBtnObj = canvas.transform.Find("StartButton");
		var startBtn = startBtnObj.GetComponent<Button>();
		startBtn.onClick.AddListener(onTouch);
	}
	/// <summary>
	/// フェードイン
	/// </summary>
	private void startFadeIn()
	{
		if( FadeManager.Instance.State != FadeManager.FadeState.FadeNone )
		{
			FadeManager.Instance.StartFade( FadeManager.FadeType.In, 0.5f, null);
		}
	}
	private PhaseController.PhaseState updateFadeIn(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if (FadeManager.Instance.State == FadeManager.FadeState.FadeNone)
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}

	/// <summary>
	/// タッチ待機
	/// </summary>
	private void onTouch()
	{
		_IsStarted = true;
	}
	private PhaseController.PhaseState waitTouch(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if (_IsStarted)
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}

	/// <summary>
	/// フェードアウト
	/// </summary>
	private void startFadeOut()
	{
		if (FadeManager.Instance.State != FadeManager.FadeState.FadeFull)
		{
			FadeManager.Instance.StartFade(FadeManager.FadeType.Out, 0.5f, null);
		}
	}
	private PhaseController.PhaseState updateFadeOut(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if (FadeManager.Instance.State == FadeManager.FadeState.FadeFull)
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}
	private void startTransition()
	{
		// タイトルへ
		FlowManager.Instance.Next(FlowManager.Flow.Home);
	}
	private PhaseController.PhaseState updateTransition(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		return result;
	}
}
