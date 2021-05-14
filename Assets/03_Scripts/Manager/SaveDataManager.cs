using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#region DataDefine
[System.Serializable]
public class SaveData
{
	public SaveData()
	{
		Player = new PlayerData();
		Setting = new SettingData();
	}

	public PlayerData Player;
	public SettingData Setting;

	public void DeepCopy(in SaveData src)
	{
		Player.DeepCopy( src.Player );
		Setting.DeepCopy( src.Setting );
	}
}
[System.Serializable]
public class PlayerData
{
	public PlayerData()
	{
		PlayerName = string.Empty;
		PlayerLevel = 0;
	}

	public string PlayerName;
	public int PlayerLevel;

	public void DeepCopy(in PlayerData src)
	{
		PlayerName = src.PlayerName.Clone() as string;
		PlayerLevel = src.PlayerLevel;
	}
}
[System.Serializable]
public class SettingData
{
	public SettingData()
	{
		MasterVolume = 0;
		BGMVolume = 0;
		SEVolume = 0;
	}

	public int MasterVolume;
	public int BGMVolume;
	public int SEVolume;

	public void DeepCopy(in SettingData src)
	{
		MasterVolume = src.MasterVolume;
		BGMVolume = src.BGMVolume;
		SEVolume = src.SEVolume;
	}
}
#endregion

#region SaveDataManager
public class SaveDataManager : SingletonBase<SaveDataManager>
{
	public static readonly string SAVE_FILE_PATH = "/savedata";
	public static readonly string FILE_NAME = "userdata";
	public static readonly string EXT = ".dat";
	public string DirectoryPath() { return Application.persistentDataPath + SAVE_FILE_PATH; }
	public string DataPath() { return Application.persistentDataPath + SAVE_FILE_PATH + "/" + FILE_NAME + EXT; }

	public delegate void SaveFinishEvent(bool isSuccess);
	public delegate void LoadFinishEvent(bool isSuccess);
	public delegate void DeleteFinishEvent(bool isSuccess);

	public SaveData Data;

	protected override void doAwake()
	{
		Data = new SaveData();
	}

	/// <summary>
	/// セーブデータがあるかどうか
	/// </summary>
	public bool IsExistSaveData()
	{
		return File.Exists(DataPath());
	}

	/// <summary>
	/// ディレクトリがあるかどうか
	/// </summary>
	public bool IsExistDirectory()
	{
		return Directory.Exists(DirectoryPath());
	}

	/// <summary>
	/// セーブデータを作成する
	/// </summary>
	public bool makeSaveData()
	{
		if ( !IsExistDirectory() )
		{
			makeDirectory();
		}
        var file = File.Create( DataPath() );
        bool isSucceed = file != null;

        file.Close();
        
		return isSucceed;
	}

	/// <summary>
	/// ディレクトリ作成
	/// </summary>
	public bool makeDirectory()
	{
        var dir = Directory.CreateDirectory( DirectoryPath() );
        bool isSuceed = dir != null;
		return isSuceed;
	}


	/// <summary>
	/// お手軽セーブ呼び出し
	/// </summary>
	public void StartSave(object data, SaveFinishEvent endCallback)
	{
		SynchronizationContext mainThread = SynchronizationContext.Current;
		Task.Run(() => SaveDataAsync(data, endCallback, mainThread));
	}

	/// <summary>
	/// お手軽ロード呼び出し
	/// </summary>
	public void StartLoad(LoadFinishEvent endCallback)
	{
		SynchronizationContext mainThread = SynchronizationContext.Current;
		string path = DataPath();
		Task.Run(() => LoadDataAsync(path, endCallback, mainThread));
	}
	//================================================================================================================
	// セーブ
	//================================================================================================================
	#region セーブ関連機能
	/// <summary>
	/// 非同期セーブ
	/// </summary>
	public async Task SaveDataAsync(object data, SaveFinishEvent endCallback, SynchronizationContext mainThread)
	{
		bool isSuccess = false;
		await Task.Run(() =>
		{
			// セーブデータにロックをかける
			lock (data)
			{
				isSuccess = Save(data);
			}
		});

		// 終了通知
		mainThread.Post((object state) => { endCallback?.Invoke(isSuccess); }, null);

	}

	/// <summary>
	/// セーブ
	/// </summary>
	private bool Save(object savedata)
	{
		string path = DataPath();
		// ファイルがあるかどうかチェック
		if (File.Exists(path) == false)
		{
			// ファイルがないので作る
			DebugLog.LogWarning("データファイルが見つからないので新規作成します。\ndataPath : " + path);

			// ディレクトリは存在するか
			int directorySplitCharIndex = path.LastIndexOf("/");
			if (directorySplitCharIndex >= 0)
			{
				string directoryPath = path.Remove(directorySplitCharIndex);
				if (Directory.Exists(directoryPath) == false)
				{
					DebugLog.LogWarning("ディレクトリもないっぽいんで作っておきますね\ndirectoryPath : " + directoryPath);
					Directory.CreateDirectory(directoryPath);
				}
			}
			using (FileStream fs = File.Create(path)) { }
		}
		// saveする文字列
		string saveStr = "";

		// json形式に変更
		saveStr = JsonUtility.ToJson(savedata);

		try
		{
			// 保存
			using (FileStream fs = new FileStream(path, FileMode.Create))
			{
				// UTF-8で書き込む
				using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
				{
					sw.Write(saveStr);
				}
			}
		}
		catch (Exception e)
		{
			DebugLog.LogError(e.Message);
			return false;
		}

		return true;
	}
	#endregion

	//================================================================================================================
	// ロード
	//================================================================================================================
	#region ロード関連機能
	/// <summary>
	/// 非同期ロード
	/// </summary>
	public async Task LoadDataAsync(string path, LoadFinishEvent endCallback, SynchronizationContext mainThread)
	{
		SaveData savedata = default;
		bool isSuccess = false;
		await Task.Run(() =>
		{
			isSuccess = Load(path, out savedata, mainThread);
		});

		// 終了通知
		mainThread.Post(
			(object state) => 
			{
				if (isSuccess)
				{
					Data = savedata;
				}
				endCallback?.Invoke(isSuccess);
			}, null);
	}

	/// <summary>
	/// ロード
	/// </summary>
	private bool Load(string path, out SaveData savedata, SynchronizationContext mainThread)
	{
		savedata = Activator.CreateInstance<SaveData>();

		// ファイルがあるかどうかチェック
		if (File.Exists(path) == false)
		{
            // ロードエラー
            UnityEngine.Debug.LogWarning("データファイルが見つかりませんでした。");
			return false;
		}
        
        DebugLog.LogTrace("データロード");

		// データ
		string dataStr = "";

        try
        {
            // ファイル読み込み
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                // UTF-8で読み込み
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    dataStr = sr.ReadToEnd();
                }
            }
        }
        catch (Exception e)
        {
            DebugLog.LogError(e.Message + "\n" + e.StackTrace);
        }

        // パースする(デシリアライズ)
        try
		{
			savedata = JsonUtility.FromJson<SaveData>(dataStr);
		}
		catch (Exception e)
		{
            DebugLog.LogError(e.Message + "\n" + e.StackTrace);
		}

		return true;
	}
	#endregion

	//================================================================================================================
	// データ削除
	//================================================================================================================
	public bool DeleteData()
	{
		// セーブデータなし
		if (IsExistSaveData() == false)
		{
			return false;
		}

		string path = DataPath();
		try
		{
			File.Delete(path);
		}
		catch (Exception e)
		{
			DebugLog.LogError("セーブデータ削除エラー : " + e.Message);
			return false;
		}
		return true;
	}

	private static class DebugLog
	{
		public static void LogTrace(string message)
		{
			Debug.Log(message);
		}
		public static void LogWarning(string message)
		{
			Debug.LogWarning(message);
		}
		public static void LogError(string message)
		{
			Debug.LogError(message);
		}
	}
}
#endregion
