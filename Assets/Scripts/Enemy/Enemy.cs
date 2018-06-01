using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public abstract class Enemy : SimpleGameStateObserver,IScore {

	enum State { none, enemy,coin }
	State m_State;

	public bool IsEnemy { get { return m_State == State.enemy; } }
	public bool IsCoin { get { return m_State == State.coin; } }

	IEnumerator BeACoinCoroutine(float duration)
	{
		m_State = State.coin;
		yield return new WaitForSeconds(duration);
		m_State = State.enemy;
	}
	public void BeACoin(float duration)
	{
		StartCoroutine(BeACoinCoroutine(duration));
	}

	protected Rigidbody m_Rigidbody;
	protected Transform m_Transform;

	[Header("Movement")]
	[SerializeField] private float m_MaxTranslationSpeed;
	[SerializeField] private float m_MinTranslationSpeed;
	[SerializeField] private AnimationCurve m_TranslationSpeedProbaCurve;
	protected float m_TranslationSpeed;
	public float TranslationSpeed { get { return m_TranslationSpeed; } }

	protected abstract Vector3 MoveVect { get; }

	//Enemy Gfx 
	[SerializeField] Transform m_EnemyGfx;
	//Coin Gfx
	[SerializeField] Transform m_CoinGfx;


	[Header("Score")]
	[SerializeField] int m_ScoreCoin;
	public int Score { get { return m_ScoreCoin; } }

	protected bool m_Destroyed = false;

	protected override void Awake()
	{
		base.Awake();

		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();

		m_TranslationSpeed = Mathf.Lerp(m_MinTranslationSpeed, m_MaxTranslationSpeed, m_TranslationSpeedProbaCurve.Evaluate(Random.value));
	}

	protected virtual void Start()
	{
		m_State = State.enemy;
	}

	private void Update()
	{
		m_EnemyGfx.gameObject.SetActive(m_State == State.enemy);
		m_CoinGfx.gameObject.SetActive(m_State == State.coin);
	}

	public virtual void FixedUpdate()
	{
		if (!GameManager.Instance.IsPlaying) return;

		if(IsEnemy)
			m_Rigidbody.MovePosition(m_Rigidbody.position + MoveVect);
	}

	private void OnTriggerEnter(Collider col)
	{
		Debug.Log( name+" Collision with " + col.gameObject.name);
		//Debug.Break();
		if(GameManager.Instance.IsPlaying
			&& col.gameObject.CompareTag("Player"))
		{
			if (IsCoin)
			{
				EventManager.Instance.Raise(new ScoreItemEvent() { eScore = this as IScore });
				EventManager.Instance.Raise(new EnemyHasBeenDestroyedEvent() { eEnemy = this, eDestroyedByPlayer = true });
				m_Destroyed = true;
				Destroy(gameObject);
			}
		}
	}
}
