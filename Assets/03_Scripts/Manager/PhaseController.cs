using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseController
{
	public enum PhaseState
	{
		Continue,
		Next,
		Back,
	}
	public delegate PhaseState PhaseUpdator(float dt);
	public delegate void PhaseStart();

	private class PhaseItem
	{
		public PhaseItem (PhaseUpdator upd, PhaseStart stt)
		{
			IsStart = false;
			Starter = stt;
			Updator = upd;
		}
		public bool IsStart;
		public PhaseStart Starter;
		public PhaseUpdator Updator;
	}

	private List<PhaseItem> _PhaseList;
	private PhaseItem _PrevPhase;
	private PhaseItem _CurrentPhase;
	private PhaseItem _NextPhase;
	public PhaseController()
	{
		_PhaseList = new List<PhaseItem>();
		_PrevPhase = null;
		_CurrentPhase = null;
		_NextPhase = null;
	}
	public void registerPhase(PhaseUpdator phase, PhaseStart start = null)
	{
		_PhaseList.Add(new PhaseItem(phase, start));
	}
	public void startPhase()
	{
		if (_PhaseList == null) return;
		if (_PhaseList.Count < 1) return;
		_PrevPhase = null;
		_CurrentPhase = _PhaseList[0];
		_NextPhase = getNextPhase(_CurrentPhase);
	}
	public void update(float dt)
	{
		if (_CurrentPhase != null)
		{
			if (_CurrentPhase.IsStart == false)
			{
				_CurrentPhase.Starter?.Invoke();
				_CurrentPhase.IsStart = true;
			}
			if (_CurrentPhase.Updator != null)
			{
				var state = _CurrentPhase.Updator.Invoke(dt);
				switch (state)
				{
					case PhaseState.Continue:
						break;
					case PhaseState.Next:
						_CurrentPhase.IsStart = false;
						_PrevPhase = _CurrentPhase;
						_CurrentPhase = _NextPhase;
						_NextPhase = getNextPhase(_CurrentPhase);
						break;
					case PhaseState.Back:
						_CurrentPhase.IsStart = false;
						var temp = _PrevPhase;
						_PrevPhase = _CurrentPhase;
						_CurrentPhase = temp;
						_NextPhase = getNextPhase(_CurrentPhase);
						break;
				}
			}
		}
	}
	public void setNextPhase(PhaseUpdator phase)
	{
		if (_PhaseList == null) return;
		int phaseId = _PhaseList.FindIndex( (p) => p.Updator == phase );
		if (phaseId < 0) return;
		if (phaseId >= _PhaseList.Count) return;
		_NextPhase = _PhaseList[phaseId];
	}

	private PhaseItem getNextPhase(PhaseItem cur)
	{
		if (_PhaseList == null) return null;
		int currentId = _PhaseList.IndexOf(cur);
		if (currentId < 0) return null;
		++currentId;
		if (currentId >= _PhaseList.Count) return null;
		return _PhaseList[currentId];
	}
}
