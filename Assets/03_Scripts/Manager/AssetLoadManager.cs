using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.InteropServices;

public class AssetLoadManager : SingletonBase<AssetLoadManager>
{
	public delegate void LoadComplete<TObject>(bool isSucceeded, TObject result);

	private Dictionary<string, AddressableInfo> _LoadHandleList;

	protected override void doAwake()
	{
		_LoadHandleList = new Dictionary<string, AddressableInfo>();
	}

	protected override void doDestroy()
	{
		ReleaseAll();
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void LoadAsset<TObject>(string key, string path, LoadComplete<TObject> loadEndEvent)
	{
		if (_LoadHandleList == null) return;
		// if ( !_LoadHandleList.ContainsKey(key) ) ロード済みの場合でも呼ぶ
		{
			StartCoroutine(LoadAssetAsync<TObject>(key, path, loadEndEvent));
		}
	}

	/// <summary>
	/// ロード
	/// </summary>
	private IEnumerator LoadAssetAsync<TObject>(string key, string path, LoadComplete<TObject> loadEndEvent)
	{
		var handle = Addressables.LoadAssetAsync<TObject>(path);
		yield return handle;
		bool isSucceeded = handle.Status == AsyncOperationStatus.Succeeded;
		if (isSucceeded)
		{
			_LoadHandleList.Add(key, new AddressableInfo( path, handle ));
		}
		loadEndEvent?.Invoke(isSucceeded, handle.Result);
	}

	/// <summary>
	/// 解放
	/// </summary>
	public void ReleaseAsset(string key)
	{
		if (_LoadHandleList == null) return;
		if ( _LoadHandleList.ContainsKey(key) )
		{
			if( _LoadHandleList[key].Release() )
			{
				// 参照カウンタが0になったら削除
				_LoadHandleList.Remove(key);
			}
		}
	}

	/// <summary>
	/// 全解放
	/// </summary>
	public void ReleaseAll()
	{
		if (_LoadHandleList == null) return;
		foreach (var key in _LoadHandleList.Keys)
		{
			_LoadHandleList[key].ForceRelease();
		}
		_LoadHandleList.Clear();
	}

#if UNITY_EDITOR
	/// <summary>
	/// ロード済みアセットリストをトレースログに書き出す
	/// 未完成
	/// </summary>
	public void TraceLoadedAssetList()
	{
		if (_LoadHandleList == null) return;
		List<AsyncOperationHandle<long>> coroutines = new List<AsyncOperationHandle<long>>();
		foreach (var key in _LoadHandleList.Keys)
		{
			string path = _LoadHandleList[key].Path;
			var coroutine = Addressables.GetDownloadSizeAsync(path);
			coroutine.Completed += (c) => { ComputeUnitSizeAsync(path, c); };
			coroutines.Add(coroutine);
		}
		StartCoroutine(ComputeSizeAsync(coroutines));
	}
	private void ComputeUnitSizeAsync(string path, AsyncOperationHandle<long> handle)
	{
		long size = handle.Result;
		Debug.Log($"{path} : {size}");
	}
	private IEnumerator ComputeSizeAsync(List<AsyncOperationHandle<long>> handles)
	{
		bool isEnd = true;
		long allSize = 0;
		foreach(var h in handles)
		{
			if(!h.IsDone)
			{
				isEnd = false;
				break;
			}
			allSize += h.Result;
		}
		if (!isEnd) yield break;
		Debug.Log($"AllSize = {allSize}");
	}
#endif

	private class AddressableInfo
	{
		public string Path { private set; get; }
		public AsyncOperationHandle Handle { private set; get; }

		public AddressableInfo(string path, AsyncOperationHandle handle)
		{
			Path = path;
			Handle = handle;
		}

		public bool Release()
		{
			bool isDependenciesNone = false;
			Addressables.Release(Handle);
			// 参照がなくなったかどうか
			if (!Handle.IsValid())
			{
				isDependenciesNone = true;
			}
			return isDependenciesNone;
		}

		public void ForceRelease()
		{
			bool isDependenciesNone = true;
			while(!isDependenciesNone)
			{
				Addressables.Release(Handle);
				// handleの参照カウンタが0かどうか todoこの処理であってるかわからない
				List<AsyncOperationHandle> dependencies = new List<AsyncOperationHandle>();
				Handle.GetDependencies(dependencies);
				if (dependencies.Count == 0)
				{
					isDependenciesNone = true;
				}
			}
		}
	}
}
