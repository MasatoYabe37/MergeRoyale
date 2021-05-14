using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : SingletonBase<T>
{
    private static T _Instance;
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                CreateInstance();
            }
            return _Instance;
        }
    }

    private static void CreateInstance()
    {
#if UNITY_EDITOR
        Debug.LogWarning("InstanceがNullです。StartUpProcess.csの初期化のところに追加してください。");
#endif
        var regident = GameObject.Find(StartUpProcess.MANAGER_OBJECT_NAME);
        if (regident == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Managerゲームオブジェクトがないため生成できませんでした");
#endif
            return;
        }
        _Instance = regident.AddComponent<T>();
    }

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_Instance != this)
        {
            Destroy(this);
        }
        doAwake();
    }

    void Start()
    {
        doStart();
    }

    void Update()
    {
        doUpdate();
    }

    void LateUpdate()
    {
        doLateUpdate();
    }

    private void OnDestroy()
    {
        doDestroy();
    }

    #region 継承
    protected virtual void doAwake()
    {

    }

	protected virtual void doStart()
    {

    }

	protected virtual void doUpdate()
    {

    }

	protected virtual void doLateUpdate()
    {

    }

	protected virtual void doDestroy()
    {

    }
    #endregion
}
