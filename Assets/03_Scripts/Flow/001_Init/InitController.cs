using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitController : SceneControllerBase
{
	private bool _IsLoadedSaveData = false;

	protected override void doStart()
	{
		_Phase.registerPhase(updateLoadSaveData, startLoadSaveData);
		_Phase.registerPhase(updateLoadFade);
		_Phase.registerPhase(updateFadeOut, startFadeOut);
		_Phase.registerPhase(update, startUpdate);
	}

	private void startLoadSaveData()
	{
		_IsLoadedSaveData = false;
		// セーブデータなし
		if ( !SaveDataManager.Instance.IsExistSaveData() )
		{
			// データ作成
			SaveDataManager.Instance.makeSaveData();
			// データの初期化
			SaveDataManager.Instance.Data = new SaveData();
		}
		// セーブデータ読み込み
		SaveDataManager.Instance.StartLoad(
			(isSuceed)=>
			{
				_IsLoadedSaveData = true;
			});
	}
	private PhaseController.PhaseState updateLoadSaveData(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if(_IsLoadedSaveData)
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}
	private PhaseController.PhaseState updateLoadFade(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if (FadeManager.Instance.IsLoaded)
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}
	private void startFadeOut()
	{
		FadeManager.Instance.StartFade( FadeManager.FadeType.Out, 0.1f, null);
	}
	private PhaseController.PhaseState updateFadeOut(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		if( FadeManager.Instance.State == FadeManager.FadeState.FadeFull )
		{
			result = PhaseController.PhaseState.Next;
		}

		return result;
	}
	private void startUpdate()
	{
		// タイトルへ
		FlowManager.Instance.Next( FlowManager.Flow.Title );
	}
	private PhaseController.PhaseState update(float dt)
	{
		PhaseController.PhaseState result = PhaseController.PhaseState.Continue;

		return result;
	}
}
