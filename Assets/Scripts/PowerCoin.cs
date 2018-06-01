using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PowerCoin : MonoBehaviour {

	Rigidbody m_Rigidbody;
	Transform m_Transform;

	[SerializeField] float m_TranslationSpeed;
	[SerializeField] float m_AngAmpForMirror;

	Vector3 m_TranslationDir;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = GetComponent<Transform>();
	}

	// Use this for initialization
	void Start () {
		m_TranslationDir = Quaternion.AngleAxis(45+90 * Random.Range(0, 4), Vector3.forward)* Vector3.right;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		m_Rigidbody.MovePosition(m_Rigidbody.position + m_TranslationDir * m_TranslationSpeed*Time.fixedDeltaTime);
		m_Rigidbody.velocity = Vector3.zero;
		m_Rigidbody.angularVelocity = Vector3.zero;
		//Debug.Break();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (GameManager.Instance.IsPlaying)
		{
			if ((collision.gameObject.CompareTag("Ground")
			|| collision.gameObject.CompareTag("Limit")
			|| collision.gameObject.CompareTag("Platform")))
			{
				Vector3 localContactPt = m_Transform.InverseTransformPoint(collision.contacts[0].point);

				float angle = Mathf.Atan2(localContactPt.y, localContactPt.x);
				if (angle < 0) angle += Mathf.PI * 2;

				angle *= Mathf.Rad2Deg;

				if (angle <= m_AngAmpForMirror / 2
					|| angle >= 360 - m_AngAmpForMirror / 2
					|| Mathf.Abs(angle - 180) <= m_AngAmpForMirror / 2)
					m_TranslationDir = Vector3.Reflect(m_TranslationDir, Vector3.right);
				else if (Mathf.Abs(angle - 90) <= m_AngAmpForMirror / 2
					|| Mathf.Abs(angle - 270) <= m_AngAmpForMirror / 2)
					m_TranslationDir = Vector3.Reflect(m_TranslationDir, Vector3.up);
				else m_TranslationDir *= -1;
			}

			if (collision.gameObject.CompareTag("Player"))
			{
				EventManager.Instance.Raise(new PowerCoinHasBeenHitEvent());
				Destroy(gameObject);
			}
		}

	}
}
