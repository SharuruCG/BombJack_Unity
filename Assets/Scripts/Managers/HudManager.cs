using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDD.Events;

public class HudManager : Manager<HudManager> {

	[Header("HudManager")]
	#region Labels & Values
	[Header("Texts")]
	[SerializeField] private Text m_TxtBestScore;
	[SerializeField] private Text m_TxtScore;
	[SerializeField] private Text m_TxtNLives;
	[SerializeField] private Text m_TxtNEnemiesLeftBeforeVictory;
	[SerializeField] private Text m_TxtNPointsGainedForPowerCoin;
	#endregion

	#region Manager implementation
	protected override IEnumerator InitCoroutine()
	{
		yield break;
	}
	#endregion

	#region Events subscription
	public override void SubscribeEvents()
	{
		base.SubscribeEvents();

		//level
		EventManager.Instance.AddListener<BombPointsForPowerCoinsChangedEvent>(BombPointsForPowerCoinsChanged);
		
	}
	public override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();

		//level
		EventManager.Instance.RemoveListener<BombPointsForPowerCoinsChangedEvent>(BombPointsForPowerCoinsChanged);

	}
	#endregion

	#region Callbacks to Level events
	private void BombPointsForPowerCoinsChanged(BombPointsForPowerCoinsChangedEvent e)
	{
		m_TxtNPointsGainedForPowerCoin.text = e.ePoints.ToString("N01");
	}
	#endregion

	#region Callbacks to GameManager events
	protected override void GameStatisticsChanged(GameStatisticsChangedEvent e)
	{
		m_TxtBestScore.text = e.eBestScore.ToString();
		m_TxtScore.text = e.eScore.ToString();
		m_TxtNLives.text = e.eNLives.ToString();
		m_TxtNEnemiesLeftBeforeVictory.text = e.eNEnemiesLeftBeforeVictory.ToString();
	}
	#endregion
}
