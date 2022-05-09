using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

[Serializable]
public enum TypeSimulator
{
    Straight,
    Back
}

public enum Role
{
    Captain,
    MemberLeft,
    MemberRight,
    MemberBack
}

public class GameManager : MonoBehaviour
{
    public static float CaptainVelocity;
    private int m_StablePlane = 0;
    public static GameManager Instance;
    [Header("Simulator")] [SerializeField] private TypeSimulator m_TypeSimulator;

    [Header("Clouds")] [SerializeField] private Transform m_Clouds;
    [SerializeField] private float m_CloudsHeight;

    [Header("Plane")] 
    [SerializeField] private float m_TimeWaitPlane = 20;
    [SerializeField] private SimulatorFlight m_Captain;
    [SerializeField] private SimulatorFlight m_MemberLeft;
    [SerializeField] private SimulatorFlight m_MemberRight;
    [SerializeField] private SimulatorFlight m_MemberBack;
    [Header("Assemble")] 
    [SerializeField] private float Ttc = 10;

    [Header("Camera")] [SerializeField] private CinemachineVirtualCamera m_MainCamera;

    private Vector3 m_LastCaptainPosition;
    private Transform m_CaptainTransform;
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        m_Clouds.position = new Vector3(m_Clouds.position.x, m_CloudsHeight, m_Clouds.position.z);
        m_CaptainTransform = m_Captain.transform;
        m_LastCaptainPosition = m_CaptainTransform.position;
    }

    private IEnumerator Start()
    {
        var delay = new WaitForSeconds(m_TimeWaitPlane - 10);
        yield return new WaitForSeconds(1);
        m_Captain.StartSimulator(m_TypeSimulator,Role.Captain);
        yield return delay;
        m_MemberLeft.StartSimulator(m_TypeSimulator,Role.MemberLeft);
        yield return delay;
        m_MemberRight.StartSimulator(m_TypeSimulator,Role.MemberRight);
        yield return delay;
        m_MemberBack.StartSimulator(m_TypeSimulator,Role.MemberBack);
    }

    private void FixedUpdate()
    {
        CaptainVelocity = (m_CaptainTransform.position - m_LastCaptainPosition).magnitude / Time.fixedDeltaTime;
        m_LastCaptainPosition = m_CaptainTransform.position;
    }

    public void PlaneStable()
    {
        if (++m_StablePlane == 4)
        {
            var captainPosition = m_CaptainTransform.position;
            var assembleX = captainPosition.x + Ttc * SimulatorFlight.m_HoldingVelocity.x;
            m_Captain.Assemble(new Vector3(assembleX,captainPosition.y,captainPosition.z),Ttc);
            m_MemberLeft.Assemble(new Vector3(assembleX - 10,captainPosition.y,captainPosition.z - 10),Ttc);
            m_MemberRight.Assemble(new Vector3(assembleX - 10,captainPosition.y,captainPosition.z + 10),Ttc);
            m_MemberBack.Assemble(new Vector3(assembleX - 20,captainPosition.y,captainPosition.z - 20),Ttc);
        }
    }
}
