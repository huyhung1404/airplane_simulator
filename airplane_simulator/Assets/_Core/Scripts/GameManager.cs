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

    [Header("Camera")] 
    [SerializeField] private CinemachineVirtualCamera m_MainCamera;
    [SerializeField] private CinemachineVirtualCamera m_StartCamera;
    [SerializeField] private GameObject m_BackCamera;
    [SerializeField] private AnimationCurve m_StartCameraAnimation;

    [Header("Canvas")] 
    [SerializeField] private CanvasGroup m_MapView;
    [SerializeField] private CanvasGroup m_DisplayView;

    private Vector3 m_LastCaptainPosition;
    private Transform m_CaptainTransform;
    private CinemachineTransposer m_MainCameraComposer;
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        m_MainCameraComposer = m_MainCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_Clouds.position = new Vector3(m_Clouds.position.x, m_CloudsHeight, m_Clouds.position.z);
        m_CaptainTransform = m_Captain.transform;
        m_LastCaptainPosition = m_CaptainTransform.position;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        var delay = new WaitForSeconds(m_TimeWaitPlane);
        StartCoroutine(CameraManager());
        m_Captain.StartSimulator(Role.Captain,m_TimeWaitPlane);
        yield return delay;
        m_MemberLeft.StartSimulator(Role.MemberLeft,m_TimeWaitPlane);
        yield return delay;
        m_MemberRight.StartSimulator(Role.MemberRight,m_TimeWaitPlane);
        yield return delay;
        m_MemberBack.StartSimulator(Role.MemberBack,m_TimeWaitPlane);
        m_DisplayView.DOFade(0, 1).SetDelay(m_TimeWaitPlane + 5).OnComplete(() => m_BackCamera.SetActive(false));
    }

    private IEnumerator CameraManager()
    {
        yield return new WaitForSeconds(m_TimeWaitPlane);
        m_MainCamera.gameObject.SetActive(true);
        m_StartCamera.gameObject.SetActive(false);
        m_MapView.DOFade(1, 1);
        m_DisplayView.DOFade(1, 1);
    }

    private void FixedUpdate()
    {
        CaptainVelocity = (m_CaptainTransform.position - m_LastCaptainPosition).magnitude / Time.fixedDeltaTime;
        m_LastCaptainPosition = m_CaptainTransform.position;
    }

    public void PlaneStable()
    {
        switch (++m_StablePlane)
        {
            case 1:
                if (m_TypeSimulator == TypeSimulator.Straight)
                {
                    DOTween.To(() => m_MainCameraComposer.m_FollowOffset, value => m_MainCameraComposer.m_FollowOffset = value,
                        new Vector3(30, 40, -20), 10).SetEase(m_StartCameraAnimation).SetDelay(20);
                }
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                Vector3 _positionAssemble;
                switch (m_TypeSimulator)
                {
                    case TypeSimulator.Straight:
                        _positionAssemble = new Vector3(m_CaptainTransform.position.x + Ttc * SimulatorFlight.m_HoldingVelocity.x, m_CaptainTransform.position.y, m_CaptainTransform.position.z);
                        m_Captain.Assemble(_positionAssemble,Ttc);
                        m_MemberLeft.Assemble(new Vector3(_positionAssemble.x - 10,_positionAssemble.y,_positionAssemble.z - 10),Ttc);
                        m_MemberRight.Assemble(new Vector3(_positionAssemble.x - 10,_positionAssemble.y,_positionAssemble.z + 10),Ttc);
                        m_MemberBack.Assemble(new Vector3(_positionAssemble.x - 20,_positionAssemble.y,_positionAssemble.z - 20),Ttc);
                        m_MapView.DOFade(0, 1).SetDelay(Ttc + 5).OnComplete(() =>
                        {
                            m_MainCamera.Follow = null;
                        });
                        break;
                    case TypeSimulator.Back:
                        _positionAssemble = new Vector3(m_MemberBack.transform.position.x - 100, m_MemberBack.transform.position.y + 200, m_MemberBack.transform.position.z + 200);
                        m_Captain.TurnBack(_positionAssemble,0,50);
                        DOTween.To(() => m_MainCameraComposer.m_FollowOffset, value => m_MainCameraComposer.m_FollowOffset = value,
                            new Vector3(30, 40, -20), 10).SetEase(m_StartCameraAnimation).SetDelay(20);
                        // m_MemberLeft.TurnBack(new Vector3(_positionAssemble.x + 10,_positionAssemble.y,_positionAssemble.z +10),6,Ttc);
                        // m_MemberRight.TurnBack(new Vector3(_positionAssemble.x + 10,_positionAssemble.y,_positionAssemble.z - 10),12,Ttc);
                        // m_MemberBack.TurnBack(new Vector3(_positionAssemble.x + 20,_positionAssemble.y,_positionAssemble.z + 20),18,Ttc);
                        m_MapView.DOFade(0, 1).SetDelay(Ttc + 5).OnComplete(() =>
                        {
                            m_MainCamera.Follow = null;
                        });
                        break;
                }
                
                break;
        }
    }
    
}
