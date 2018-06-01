using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomPan : SimpleGameStateObserver {

	Transform m_Transform;

	[Header("Zoom")]
	[SerializeField] float m_ZoomSpeed;
	[SerializeField] float m_ZoomLerpCoef;
	[SerializeField] float m_MinPosZ;
	[SerializeField] float m_MaxPosZ;
	[SerializeField] float m_StartPosZ;
	float m_PosZ;
	float m_TargetPosZ;

	//
	[Header("Pan")]
	[SerializeField] float m_PanSpeed;
	[SerializeField] float m_PanLerpCoef;
	[SerializeField] Vector2 m_MinPosXY;
	[SerializeField] Vector2 m_MaxPosXY;
	[SerializeField] Vector2 m_StartPosXY;
	Vector2 m_PosXY;
	Vector2 m_TargetPosXY;

	Vector3 m_PrevMousePosition;

	protected override void Awake()
	{
		base.Awake();
		m_Transform = GetComponent<Transform>();
	}

	protected void Start()
	{
		ResetCamera();
		m_PrevMousePosition = Input.mousePosition;
		Debug.Log("reset");
	}

	// Update is called once per frame
	void Update () {
		//zoom
		m_TargetPosZ = Mathf.Clamp(m_TargetPosZ + m_ZoomSpeed*Input.GetAxis("Mouse ScrollWheel")*Time.deltaTime, m_MinPosZ, m_MaxPosZ);
		m_PosZ = Mathf.Lerp(m_PosZ, m_TargetPosZ, Time.deltaTime * m_ZoomLerpCoef);
		SetCameraPositionZ(m_PosZ);

		//pan
		Vector2 mouseMove = (Input.mousePosition - m_PrevMousePosition)*m_PanSpeed*(Input.GetMouseButton(1)?1:0)*Time.deltaTime;
		m_TargetPosXY = m_TargetPosXY + mouseMove;
		m_TargetPosXY.x = Mathf.Clamp(m_TargetPosXY.x, m_MinPosXY.x, m_MaxPosXY.x);
		m_TargetPosXY.y = Mathf.Clamp(m_TargetPosXY.y, m_MinPosXY.y, m_MaxPosXY.y);
		m_PosXY = Vector2.Lerp(m_PosXY, m_TargetPosXY, Time.deltaTime * m_PanLerpCoef);
		SetCameraPositionXY(m_PosXY);

		m_PrevMousePosition = Input.mousePosition;
	}

	void ResetCamera()
	{
		ResetPosZ();
		ResetPosXY();
	}

	void ResetPosZ()
	{
		m_PosZ = m_StartPosZ;
		m_TargetPosZ = m_PosZ;
		SetCameraPositionZ(m_PosZ);
	}

	void ResetPosXY()
	{
		m_PosXY = m_StartPosXY;
		m_TargetPosXY = m_PosXY;
		SetCameraPositionXY(m_PosXY);
	}

	void SetCameraPositionXY(Vector2 posXY)
	{
		m_Transform.position = new Vector3(m_PosXY.x, m_PosXY.y, m_Transform.position.z);
	}

	void SetCameraPositionZ(float posZ)
	{
		m_Transform.position = new Vector3(m_Transform.position.x, m_Transform.position.y, posZ);
	}

	//Game State Events
	protected override void GamePlay(GamePlayEvent e)
	{
		Debug.Log("GamePlay");
		ResetCamera();
	}
}