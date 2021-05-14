using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpProcess
{
	public static string MANAGER_OBJECT_NAME = "Manager";

	private static Type[] ManagerTypes =
	{
		/* ここに、一番最初に生成するシングルトンのマネージャークラスを記入 */
		typeof(AssetLoadManager),
		typeof(SaveDataManager),
		typeof(FlowManager),
		typeof(FadeManager),
	};

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeApplication()
	{
		// Application設定
		Application.targetFrameRate = 60;

		// Managerオブジェクトを生成
		var mgrObj = new GameObject();
		mgrObj.name = MANAGER_OBJECT_NAME;
		UnityEngine.Object.DontDestroyOnLoad(mgrObj);
		for (int i = 0; i < ManagerTypes.Length; ++i)
		{
			mgrObj.AddComponent(ManagerTypes[i]);
		}
	}
}
