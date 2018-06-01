using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class Bomb : MonoBehaviour,IScore {

	#region Bombs
	//Bombs static management
	private static List<Bomb> m_Bombs = new List<Bomb>();
	public static List<Bomb> Bombs { get { return m_Bombs; } }

	public static bool AreAllBombsDestroyed
	{
		get
		{
			Bomb nonDestroyedBomb = m_Bombs.Find(item => item != null && !item.m_Destroyed);
			return nonDestroyedBomb == null;
		}
	}

	public static Bomb RandomOffBomb
	{
		get {
			List<Bomb> offBombs = m_Bombs.FindAll(item => !item.IsOn);
			if (offBombs != null && offBombs.Count > 0)
				return offBombs[Random.Range(0, offBombs.Count)];
			else return null;
		}
	}

	public static void ExtinguishAllWicks()
	{
		foreach (Bomb bomb in m_Bombs)
		{
			bomb.m_State = State.off;
			bomb.StopAllCoroutines();
		}
	}

	public static void LightTheWickOfRandomBomb()
	{
		Bomb bomb = RandomOffBomb;
		if (bomb) bomb.WaitAndLightTheWick();
	}
	public static void LightTheWickOfRandomBomb(float waitDuration)
	{
		Bomb bomb = RandomOffBomb;
		if (bomb) bomb.WaitAndLightTheWick(waitDuration);
	}
	#endregion

	enum State { none,off,onSoon,on,coin}

	State m_State;
	public bool IsOn { get { return m_State == State.on; } }
	public bool IsOnSoon { get { return m_State == State.onSoon; } }

	[SerializeField] float m_WaitNominalDuration;
	[SerializeField] float m_WaitRandomCoef;

	float WaitDuration { get { return Mathf.Max(.1f, m_WaitNominalDuration * (1 + m_WaitRandomCoef * Random.value * Mathf.Sign(Random.value - .5f))); } }

	//Bomb 
	[SerializeField] Transform m_Bomb;
	//Wick 
	[SerializeField] Transform m_Wick;
	//Coin 
	[SerializeField] Transform m_Coin;

	[SerializeField] int m_ScoreBombOff;
	[SerializeField] int m_ScoreBombOn;
	[SerializeField] int m_ScoreCoin;
	public int Score {
		get {
			switch (m_State)
			{
				case State.coin:
					return m_ScoreCoin;
				case State.on:
					return m_ScoreBombOn;
				default:
					return m_ScoreBombOff;
			}
		}
	}

	public float PointsForPowerCoin
	{
		get
		{
			switch (m_State)
			{
				case State.coin:
					return 0;
				case State.on:
					return 2;
				default:
					return .5f;
			}
		}
	}

	protected bool m_Destroyed = false;

	public void WaitAndLightTheWick(float duration)
	{
		StartCoroutine(WaitAndLightTheWickCoroutine(duration));
	}

	public void WaitAndLightTheWick()
	{
		WaitAndLightTheWick(WaitDuration);
	}

	IEnumerator WaitAndLightTheWickCoroutine(float duration)
	{
		m_State = State.onSoon;

		// le fait d'utiliser une coroutine permet potentiellement de gérer un feedback dynamique graphique
		float elapsedTime = 0;
		while(elapsedTime<duration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		m_State = State.on;
	}

	private void OnEnable()
	{
		if (!m_Bombs.Contains(this))
			m_Bombs.Add(this);
	}

	private void OnDestroy()
	{
		m_Bombs.Remove(this);
	}

	// Use this for initialization
	void Start () {
		m_State = State.off;
	}
	
	// Update is called once per frame
	void Update () {
		m_Wick.gameObject.SetActive(m_State == State.on);
		m_Coin.gameObject.SetActive(m_State == State.coin);
		m_Bomb.gameObject.SetActive(m_State != State.coin);
		
	}

	private void OnTriggerEnter(Collider other)
	{
		if(GameManager.Instance.IsPlaying
			&& !m_Destroyed
			&& other.CompareTag("Player"))
		{
			m_Destroyed = true;

			EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
			EventManager.Instance.Raise(new BombHasBeenDestroyedEvent() { eBomb = this, eDestroyedByPlayer = true });
			Destroy(gameObject);

			if (IsOn || IsOnSoon) LightTheWickOfRandomBomb();
		}
	}
}
