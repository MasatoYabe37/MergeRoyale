using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class FadeManager : SingletonBase<FadeManager>
{
	public enum FadeState
	{
		FadeNone,
		FadeOut,
		FadeFull,
		FadeIn,
	}
	public enum FadeType
	{
		None,
		Full,

		In,
		Out,

		MAX,
	}
	private readonly string[] FadeAnim = new string[(int)FadeType.MAX]
	{
		"None",
		"Full",
		"NormalFadeIn",
		"NormalFadeOut",
	};
	public delegate void FadeEndEvent();
	private const float END_TIME = 1f - float.Epsilon;
	private const string OBJECT_PATH = "UI/Fade/FadeCanvas.prefab";

	public bool IsLoaded;
	public FadeState State;
	private FadeType _FadeType;
	private float _FadeTime;
	private Color _FadeColor;
	private GameObject _FadeObject;
	private Animator _FadeAnimator;
    private Image _FadeImage;
	private AnimatorStateInfo _FadeAnimatorState;
	private FadeEndEvent _FadeEndEvent;

	protected override void doStart()
	{
		IsLoaded = false;
		State = FadeState.FadeNone;
		_FadeType = FadeType.In;
		_FadeTime = 0f;
		_FadeColor = Color.black;
		_FadeEndEvent = null;

		AssetLoadManager.Instance.LoadAsset<GameObject>("Fade", OBJECT_PATH, onLoadedFadeObject);
	}
	protected override void doUpdate()
	{
		switch (State)
		{
			case FadeState.FadeNone:
			case FadeState.FadeFull:
				break;
			case FadeState.FadeIn:
			case FadeState.FadeOut:
				fadeUpdate();
				break;
		}
	}
	protected override void doDestroy()
	{
		GameObject.Destroy(_FadeObject);
	}

	/// <summary>
	/// フェード用オブジェクトをロード
	/// </summary>
	private void onLoadedFadeObject(bool isSucceed, Object fadeObject)
	{
		if (isSucceed)
		{
			IsLoaded = true;
			var prefab = fadeObject as GameObject;

			_FadeObject = Instantiate(prefab);
			DontDestroyOnLoad(_FadeObject);
			_FadeAnimator = _FadeObject.transform.Find("FadePanel").GetComponent<Animator>();
			_FadeImage = _FadeObject.transform.Find("FadePanel").GetComponent<Image>();
			StartFade(FadeType.None, 0f, null);

			AssetLoadManager.Instance.ReleaseAsset("Fade");
		}
	}

	/// <summary>
	/// フェード開始
	/// </summary>
	public void StartFade(FadeType type, float time, Color color, FadeEndEvent endEvent)
	{
		if (State == FadeState.FadeIn || State == FadeState.FadeOut) return;
		_FadeType = type;
		_FadeTime = time;
		_FadeColor = color;
		_FadeEndEvent = endEvent;
        _FadeImage.raycastTarget = true; // フェード中は画面をタッチできない
		if (time <= 0f)
		{
			fadeEnd();
			return;
		}
		setState();
		setFadeAnim(time);
	}
	public void StartFade(FadeType type, float time, FadeEndEvent endEvent)
	{
		StartFade(type, time, Color.black, endEvent);
	}
	/// <summary>
	/// FadeTypeに応じて　ステートをセット
	/// </summary>
	private void setState()
	{
		switch (_FadeType)
		{
			case FadeType.None: State = FadeState.FadeNone; break;
			case FadeType.Full: State = FadeState.FadeFull; break;
			case FadeType.In: State = FadeState.FadeIn; break;
			case FadeType.Out: State = FadeState.FadeOut; break;
		}
	}
	/// <summary>
	/// FadeTypeに応じて　ステートをセット
	/// </summary>
	private void setEndState()
	{
		switch (_FadeType)
		{
			case FadeType.None: State = FadeState.FadeNone; break;
			case FadeType.Full: State = FadeState.FadeFull; break;
			case FadeType.In: State = FadeState.FadeNone; break;
			case FadeType.Out: State = FadeState.FadeFull; break;
		}
	}
	/// <summary>
	/// フェードアニメーションをセット
	/// </summary>
	private void setFadeAnim(float time)
	{
		if (_FadeAnimator != null)
		{
			_FadeAnimator.Play(FadeAnim[(int)_FadeType]);
			_FadeAnimator.Update(0f);
			_FadeAnimator.speed = time == 0f ? 1f : 1f / time;
		}
	}
	/// <summary>
	/// フェード完了のアニメにする
	/// </summary>
	private void setFadeEndAnim()
	{
		if (_FadeAnimator != null)
		{
			_FadeAnimator.Play(FadeAnim[(int)_FadeType], 0, 1f);
			_FadeAnimator.Update(0f);
			_FadeAnimator.speed = 0f;
		}
	}
	/// <summary>
	/// フェードアニメ更新
	/// </summary>
	private void fadeUpdate()
	{
		_FadeAnimatorState = _FadeAnimator.GetCurrentAnimatorStateInfo(0);
		bool isEndAnim = _FadeAnimatorState.normalizedTime >= END_TIME;

		if (isEndAnim)
		{
			fadeEnd();
		}
	}
	/// <summary>
	/// フェード終了
	/// </summary>
	private void fadeEnd()
	{
		setEndState();
		setFadeEndAnim();
		_FadeEndEvent?.Invoke();
        if (State == FadeState.FadeNone)
        {
            _FadeImage.raycastTarget = false;
        }
    }
}
