using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControllerBase : MonoBehaviour
{
	protected PhaseController _Phase;
	
	void Start()
    {
		_Phase = new PhaseController();
		doStart();
		_Phase.startPhase();
	}

    void Update()
    {
		_Phase.update(Time.fixedDeltaTime);
		doUpdate();
    }

	void LateUpdate()
	{
		doLateUpdate();
	}

	void onDestroy()
	{
		doDestroy();
	}

	void OnEnable()
	{
		doEnabled();
	}

	void OnDisable()
	{
		doDisabled();	
	}

	#region 継承
	protected virtual void doStart() { }
	protected virtual void doUpdate() { }
	protected virtual void doLateUpdate() { }
	protected virtual void doDestroy() { }
	protected virtual void doEnabled() { }
	protected virtual void doDisabled() { }
	#endregion
}
