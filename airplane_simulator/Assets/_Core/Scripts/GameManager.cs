using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private int m_StablePlane = 0;
    public static GameManager Instance;
    [Header("Simulator")] [SerializeField] private TypeSimulator m_TypeSimulator;

    [Header("Clouds")] [SerializeField] private Transform m_Clouds;
    [SerializeField] private float m_CloudsHeight;

    [Header("Plane")] [SerializeField] private float m_TimeWaitPlane = 20;
    [SerializeField] private SimulatorFlight m_Captain;
    [SerializeField] private SimulatorFlight m_MemberLeft;
    [SerializeField] private SimulatorFlight m_MemberRight;
    [SerializeField] private SimulatorFlight m_MemberBack;
    [Header("Assemble")] [SerializeField] private float Ttc = 10;

    [Header("TurnBack")] [SerializeField] private float TimeTurnBack = 10;
    [SerializeField] private float TdlLeft;
    [SerializeField] private float TdlRight;
    [SerializeField] private float TdlBack;

    [Header("Camera")] [SerializeField] private CinemachineVirtualCamera m_MainCamera;
    [SerializeField] private CinemachineVirtualCamera m_StartCamera;
    [SerializeField] private GameObject m_BackCamera;
    [SerializeField] private AnimationCurve m_StartCameraAnimation;

    [Header("Canvas")] [SerializeField] private CanvasGroup m_MapView;
    [SerializeField] private CanvasGroup m_DisplayView;

    private Transform m_CaptainTransform;
    private CinemachineTransposer m_MainCameraComposer;
    public float m_TimeScale;
    
    private void Awake()
    {
        Time.timeScale = m_TimeScale;
        Instance = this;
        Application.targetFrameRate = 60;
        m_MainCameraComposer = m_MainCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_Clouds.position = new Vector3(m_Clouds.position.x, m_CloudsHeight, m_Clouds.position.z);
        m_CaptainTransform = m_Captain.transform;
    }

    private IEnumerator Start()
    {
        AudioPlane.instance.PlayAudioKhoiDong();
        yield return new WaitForSeconds(1);
        var delay = new WaitForSeconds(m_TimeWaitPlane);
        StartCoroutine(CameraManager());
        m_Captain.StartSimulator(Role.Captain, m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberLeft.StartSimulator(Role.MemberLeft, m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberRight.StartSimulator(Role.MemberRight, m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberBack.StartSimulator(Role.MemberBack, m_TimeWaitPlane + 10);
        m_DisplayView.DOFade(0, 1).SetDelay(m_TimeWaitPlane + 5).OnComplete(() => m_BackCamera.SetActive(false));
    }

    private IEnumerator CameraManager()
    {
        yield return new WaitForSeconds(m_TimeWaitPlane + 10);
        m_MainCamera.gameObject.SetActive(true);
        m_StartCamera.gameObject.SetActive(false);
        m_MapView.DOFade(1, 1);
        m_DisplayView.DOFade(1, 1);
    }
    
    public void PlaneStable()
    {
        switch (++m_StablePlane)
        {
            case 1:
                if (m_TypeSimulator == TypeSimulator.Straight)
                {
                    DOTween.To(() => m_MainCameraComposer.m_FollowOffset,
                        value => m_MainCameraComposer.m_FollowOffset = value,
                        new Vector3(30, 40, -20), 10).SetEase(m_StartCameraAnimation).SetDelay(20);
                }

                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                Vector3 _positionAssemble;
                AudioPlane.instance.PlayAudioAssembly();
                Time.timeScale = 1;
                switch (m_TypeSimulator)
                {
                    case TypeSimulator.Straight:
                        _positionAssemble =
                            new Vector3(m_CaptainTransform.position.x + Ttc * SimulatorFlight.m_HoldingVelocity.x,
                                m_CaptainTransform.position.y, m_CaptainTransform.position.z);
                        m_Captain.Assemble(_positionAssemble, Ttc);
                        m_MemberLeft.Assemble(
                            new Vector3(_positionAssemble.x - 15, _positionAssemble.y, _positionAssemble.z - 15), Ttc);
                        m_MemberRight.Assemble(
                            new Vector3(_positionAssemble.x - 15, _positionAssemble.y, _positionAssemble.z + 15), Ttc);
                        m_MemberBack.Assemble(
                            new Vector3(_positionAssemble.x - 30, _positionAssemble.y, _positionAssemble.z - 30), Ttc);
                        m_MapView.DOFade(0, 1).SetDelay(Ttc + 5).OnComplete(() =>
                        {
                            FindObjectOfType<CloudsFollow>().End();
                            m_MainCamera.Follow = null;
                        });
                        break;
                    case TypeSimulator.Back:
                        _positionAssemble = new Vector3(m_MemberBack.transform.position.x - 400,
                            m_MemberBack.transform.position.y + 200, m_MemberBack.transform.position.z + 200);
                        m_Captain.TurnBack(_positionAssemble, 0, Ttc, TimeTurnBack,0,0);
                        DOTween.To(() => m_MainCameraComposer.m_FollowOffset,
                            value => m_MainCameraComposer.m_FollowOffset = value,
                            new Vector3(30, 80, -20), 10).SetEase(m_StartCameraAnimation).SetDelay(15);
                        m_MemberLeft.TurnBack(
                            new Vector3(_positionAssemble.x + 15, _positionAssemble.y, _positionAssemble.z + 15),
                            TdlLeft, Ttc, TimeTurnBack,15,15);
                        m_MemberRight.TurnBack(
                            new Vector3(_positionAssemble.x + 15, _positionAssemble.y, _positionAssemble.z - 15),
                            TdlRight, Ttc, TimeTurnBack,15,-15);
                        m_MemberBack.TurnBack(
                            new Vector3(_positionAssemble.x + 30, _positionAssemble.y, _positionAssemble.z + 30),
                            TdlBack, Ttc, TimeTurnBack,30,30);
                        m_MapView.DOFade(0, 1).SetDelay(Ttc - 10).OnComplete(() =>
                        {
                            EndGame();
                        });
                        break;
                }

                break;
        }
    }

    private async void EndGame()
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
        FindObjectOfType<CloudsFollow>().End();
        m_MainCamera.Follow = null;
    }
}