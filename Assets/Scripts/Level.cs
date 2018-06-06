﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class Level : MonoBehaviour,IEventHandler {

	enum LevelState { none, enemiesAreEnemies,enemiesAreCoins};

	LevelState m_LevelState;

	List<Enemy> m_Enemies = new List<Enemy>();
    //[SerializeField] int m_LevelNumber;
    [SerializeField] float m_WaitDurationBeforeLightFirstWick;

	[SerializeField] float m_NBombPointsToBeCollectedForPowerCoin;
    [SerializeField] float m_NBombPointsToBeCollectedForLifeCoin;
    float m_CollectedBombPoints = 0;

	[SerializeField] GameObject m_PowerCoinPrefab;
    [SerializeField] Transform[] m_PowerCoinSpawnPoints;

    [SerializeField] GameObject m_LifeCoinPrefab;  //Khady
    [SerializeField] Transform[] m_LifeCoinSpawnPoints; //Khady


    [SerializeField] float m_EnemiesBecomeCoinDuration;
    private int m_LevelIndex; //khady
    public int LevelIndex {get { return m_LevelIndex; } set { m_LevelIndex = value; } }//khady

    [SerializeField] float m_MinSpeedCoef;//khady
    [SerializeField] float m_MaxSpeedCoef;//khady
    [SerializeField] float m_MaxLevelIndex;//khady

    Vector3 RandomSpawnPos { get {
			List<Vector3> spawnPositions = m_PowerCoinSpawnPoints.Select(item => item.position).Where(item=>!Physics.CheckSphere(item,m_PowerCoinPrefab.GetComponent<SphereCollider>().radius)).ToList();
			spawnPositions.Sort((a, b) => Random.value.CompareTo(.5f));

			return spawnPositions[Random.Range(0, spawnPositions.Count)]; }
	}


    Vector3 RandomSpawnPosLifeCoin  //Khady
    {
        get
        {
            List<Vector3> spawnPositions = m_LifeCoinSpawnPoints.Select(item => item.position).Where(item => !Physics.CheckSphere(item, m_LifeCoinPrefab.GetComponent<SphereCollider>().radius)).ToList();
            spawnPositions.Sort((a, b) => Random.value.CompareTo(.5f));

            return spawnPositions[Random.Range(0, spawnPositions.Count)];
        }
    }


    public void SubscribeEvents()
	{
		EventManager.Instance.AddListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
		EventManager.Instance.AddListener<BombHasBeenDestroyedEvent>(BombHasBeenDestroyed);
		EventManager.Instance.AddListener <PowerCoinHasBeenHitEvent>(PowerCoinHasBeenHit);
        EventManager.Instance.AddListener<LifeCoinHasBeenHitEvent>(LifeCoinHasBeenHit);
    }

	public void UnsubscribeEvents()
	{
		EventManager.Instance.RemoveListener<EnemyHasBeenDestroyedEvent>(EnemyHasBeenDestroyed);
		EventManager.Instance.RemoveListener<BombHasBeenDestroyedEvent>(BombHasBeenDestroyed);
		EventManager.Instance.RemoveListener<PowerCoinHasBeenHitEvent>(PowerCoinHasBeenHit);
        EventManager.Instance.RemoveListener<LifeCoinHasBeenHitEvent>(LifeCoinHasBeenHit);
    }

	private void OnDestroy()
	{
		UnsubscribeEvents();
	}

	private void Awake()
	{
		SubscribeEvents();
	}

	private void Start()
	{
		//enemies
		m_Enemies = GetComponentsInChildren<Enemy>().ToList();
        foreach(var item in m_Enemies)
        {
            item.SpeedCoef = Mathf.Lerp(m_MinSpeedCoef, m_MaxSpeedCoef, m_LevelIndex / m_MaxLevelIndex);
        }
		m_LevelState = LevelState.enemiesAreEnemies;

		Bomb.LightTheWickOfRandomBomb(m_WaitDurationBeforeLightFirstWick);

		SetBombPoints(0);
	}

	void SetBombPoints(float bombPoints)
	{
		m_CollectedBombPoints = bombPoints;
		EventManager.Instance.Raise(new BombPointsForPowerCoinsChangedEvent() { ePoints = m_CollectedBombPoints });
	}

	void BombHasBeenDestroyed(BombHasBeenDestroyedEvent e)
	{
		if (Bomb.AreAllBombsDestroyed)
			EventManager.Instance.Raise(new AllBombsHaveBeenDestroyedEvent());
		else if(m_LevelState == LevelState.enemiesAreEnemies
			&& m_CollectedBombPoints < m_NBombPointsToBeCollectedForPowerCoin)
		{
			SetBombPoints(Mathf.Clamp(m_CollectedBombPoints + e.eBomb.PointsForPowerCoin, 0, m_NBombPointsToBeCollectedForPowerCoin));
			if (m_CollectedBombPoints == m_NBombPointsToBeCollectedForPowerCoin)
				Instantiate(m_PowerCoinPrefab, RandomSpawnPos,Quaternion.identity,  transform);
		}
	}

	IEnumerator EnemiesBecomeCoinsCoroutine(float duration)
	{
		m_LevelState = LevelState.enemiesAreCoins;
		foreach (var item in m_Enemies)
		{
			item.BeACoin(m_EnemiesBecomeCoinDuration);
		}

		yield return new WaitForSeconds(duration);
		m_LevelState = LevelState.enemiesAreEnemies;
		SetBombPoints(0);
	}


	void PowerCoinHasBeenHit(PowerCoinHasBeenHitEvent e)
	{
		StartCoroutine(EnemiesBecomeCoinsCoroutine(m_EnemiesBecomeCoinDuration));
	}

    void LifeCoinHasBeenHit(LifeCoinHasBeenHitEvent e)
    {

        StartCoroutine(EnemiesBecomeCoinsCoroutine(m_EnemiesBecomeCoinDuration));
    }
void EnemyHasBeenDestroyed(EnemyHasBeenDestroyedEvent e)
	{
		m_Enemies.RemoveAll(item => item.Equals(null));
		m_Enemies.Remove(e.eEnemy);
	}
}
