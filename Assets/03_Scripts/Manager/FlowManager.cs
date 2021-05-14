using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : SingletonBase<FlowManager>
{
	private enum FlowState
	{
		Idle,
		NextAsyncLoading,
		BackAsyncLoading,
	}

	public enum Flow
	{
		/// <summary> なし </summary>
		None = -1,

		/// <summary> 初期化 </summary>
		Init,
		/// <summary> タイトル </summary>
		Title,
		/// <summary> ホーム </summary>
		Home,

		Max,
	}

	// シーン名を登録（Flowと同じ順番で）
	public readonly string[] SceneNames = new string[(int)Flow.Max]
	{
		"001_Init",						// Init
		"002_Title",					// Title
		"003_Home",						// Home
	};

	public delegate void onChangedFlow(bool isSuccess);

	private FlowState _State = FlowState.Idle;

	private const int _FLOW_HISTORY_NUM = 15;
	public List<Flow> FlowHistory { private set; get; }
	public Flow CurrentFlow { private set; get; }
	public Flow NextFlow { private set; get; }
	private onChangedFlow _OnChangeFlowEvent;


	protected override void doAwake()
    {
		_State = FlowState.Idle;
		FlowHistory = new List<Flow>();
		CurrentFlow = Flow.Init;
		NextFlow = Flow.None;
		_OnChangeFlowEvent = null;
		Next(CurrentFlow);
    }

	protected override void doUpdate()
    {
	}

	private IEnumerator loadScene()
	{
		yield return SceneManager.LoadSceneAsync(SceneNames[(int)NextFlow]);

		_OnChangeFlowEvent?.Invoke(true);

		switch (_State)
		{
			case FlowState.NextAsyncLoading:
				{
					FlowHistory.Add(NextFlow);
					if (FlowHistory.Count >= _FLOW_HISTORY_NUM)
					{
						FlowHistory.RemoveAt(0);
					}
				}
				break;
			case FlowState.BackAsyncLoading:
				{
					FlowHistory.RemoveAt(FlowHistory.Count - 1);
				}
				break;
		}

		_State = FlowState.Idle;
		CurrentFlow = NextFlow;
		NextFlow = Flow.None;
	}

	public void Next(Flow nextFlow)
	{
		if (_State != FlowState.Idle) return;
		if (((int)nextFlow < 0) || ((int)nextFlow >= SceneNames.Length)) return;
		SceneManager.LoadScene(SceneNames[(int)nextFlow], LoadSceneMode.Single);
		FlowHistory.Add(nextFlow);
		if(FlowHistory.Count >= _FLOW_HISTORY_NUM)
		{
			FlowHistory.RemoveAt(0);
		}
	}
	public void Back()
	{
		if (_State != FlowState.Idle) return;
		if (FlowHistory.Count < 1) return;
		int index = FlowHistory.Count - 1;
		var next = FlowHistory[index];
		if (((int)next < 0) || ((int)next >= SceneNames.Length)) return;
		SceneManager.LoadScene(SceneNames[(int)next]);
		FlowHistory.RemoveAt(index);
	}
	public void NextAsync(Flow nextFlow, onChangedFlow onChanged = null)
	{
		bool isFailed = false;
		isFailed = isFailed | _State != FlowState.Idle;
		isFailed = isFailed | (((int)nextFlow < 0) || ((int)nextFlow >= SceneNames.Length));
		if (isFailed)
		{
			onChanged?.Invoke(false);
			return;
		}
		_OnChangeFlowEvent = onChanged;
		_State = FlowState.NextAsyncLoading;
		NextFlow = nextFlow;
		StartCoroutine( loadScene() );
	}
	public void BackAsync(onChangedFlow onChanged = null)
	{
		bool isFailed = false;
		isFailed = isFailed | _State != FlowState.Idle;
		isFailed = isFailed | (FlowHistory.Count < 1);
		if (isFailed)
		{
			onChanged?.Invoke(false);
			return;
		}
		int index = FlowHistory.Count - 1;
		var next = FlowHistory[index];
		_OnChangeFlowEvent = onChanged;
		_State = FlowState.BackAsyncLoading;
		NextFlow = next;
		StartCoroutine( loadScene() );
	}
}
