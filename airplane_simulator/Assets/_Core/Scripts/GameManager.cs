using System;
using System.Collections;
using System.IO;
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
    [SerializeField] private SimulatorFlight m_Captain;
    [SerializeField] private SimulatorFlight m_MemberLeft;
    [SerializeField] private SimulatorFlight m_MemberRight;
    [SerializeField] private SimulatorFlight m_MemberBack;
    [Header("Camera")] [SerializeField] private CinemachineVirtualCamera m_MainCamera;
    [SerializeField] private CinemachineVirtualCamera m_StartCamera;
    [SerializeField] private GameObject m_BackCamera;
    [SerializeField] private AnimationCurve m_StartCameraAnimation;

    [Header("Canvas")] [SerializeField] private CanvasGroup m_MapView;
    [SerializeField] private CanvasGroup m_DisplayView;

    private Transform m_CaptainTransform;
    private CinemachineTransposer m_MainCameraComposer;
    public string fileName;
    private GameManagerData m_GameDatas;
    
    private void Awake()
    {
        ReadJson();
        Instance = this;
        Application.targetFrameRate = 60;
        m_MainCameraComposer = m_MainCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_Clouds.position = new Vector3(m_Clouds.position.x, m_GameDatas.m_CloudsHeight, m_Clouds.position.z);
        m_CaptainTransform = m_Captain.transform;
    }
    
    public void ReadJson(){
        string path = $"{Application.streamingAssetsPath}/{fileName}.json";
        string contents = File.ReadAllText(path);
        m_GameDatas = JsonUtility.FromJson<GameManagerData>(contents);
    }

    public void StartGame()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        AudioPlane.instance.PlayAudioKhoiDong();
        yield return new WaitForSeconds(2);
        var delay = new WaitForSeconds(m_GameDatas.m_TimeWaitPlane);
        StartCoroutine(CameraManager());
        m_Captain.StartSimulator(Role.Captain,  m_GameDatas.m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberLeft.StartSimulator(Role.MemberLeft,  m_GameDatas.m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberRight.StartSimulator(Role.MemberRight,  m_GameDatas.m_TimeWaitPlane + 10);
        yield return delay;
        m_MemberBack.StartSimulator(Role.MemberBack,  m_GameDatas.m_TimeWaitPlane + 10);
        m_DisplayView.DOFade(0, 1).SetDelay(m_GameDatas. m_TimeWaitPlane + 5).OnComplete(() => m_BackCamera.SetActive(false));
    }

    private IEnumerator CameraManager()
    {
        yield return new WaitForSeconds(m_GameDatas.m_TimeWaitPlane + 10);
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
                switch (m_TypeSimulator)
                {
                    case TypeSimulator.Straight:
                        _positionAssemble =
                            new Vector3(m_CaptainTransform.position.x + m_GameDatas.Ttc * SimulatorFlight.m_HoldingVelocity.x,
                                m_CaptainTransform.position.y, m_CaptainTransform.position.z);
                        m_Captain.Assemble(_positionAssemble, m_GameDatas.Ttc);
                        m_MemberLeft.Assemble(
                            new Vector3(_positionAssemble.x - m_GameDatas.DistanceAssemble, _positionAssemble.y, _positionAssemble.z - m_GameDatas.DistanceAssemble), m_GameDatas.Ttc);
                        m_MemberRight.Assemble(
                            new Vector3(_positionAssemble.x - m_GameDatas.DistanceAssemble, _positionAssemble.y, _positionAssemble.z + m_GameDatas.DistanceAssemble), m_GameDatas.Ttc);
                        m_MemberBack.Assemble(
                            new Vector3(_positionAssemble.x - m_GameDatas.DistanceAssemble * 2, _positionAssemble.y, _positionAssemble.z - m_GameDatas.DistanceAssemble * 2), m_GameDatas.Ttc);
                        m_MapView.DOFade(0, 1).SetDelay(m_GameDatas.Ttc + 5).OnComplete(() =>
                        {
                            FindObjectOfType<CloudsFollow>().End();
                            m_MainCamera.Follow = null;
                        });
                        break;
                    case TypeSimulator.Back:
                        _positionAssemble = new Vector3(m_MemberBack.transform.position.x + m_GameDatas._positionAssembleX,
                            m_MemberBack.transform.position.y + m_GameDatas._positionAssembleY, m_MemberBack.transform.position.z + m_GameDatas._positionAssembleZ);
                        m_Captain.TurnBack(_positionAssemble, 0, m_GameDatas.Ttc, m_GameDatas.TimeTurnBack,0,0);
                        DOTween.To(() => m_MainCameraComposer.m_FollowOffset,
                            value => m_MainCameraComposer.m_FollowOffset = value,
                            new Vector3(30, 80, -20), 10).SetEase(m_StartCameraAnimation).SetDelay(15);
                        m_MemberLeft.TurnBack(
                            new Vector3(_positionAssemble.x + m_GameDatas.DistanceAssemble, _positionAssemble.y, _positionAssemble.z + m_GameDatas.DistanceAssemble),
                            m_GameDatas.TdlLeft, m_GameDatas.Ttc, m_GameDatas.TimeTurnBack,m_GameDatas.DistanceAssemble,m_GameDatas.DistanceAssemble);
                        m_MemberRight.TurnBack(
                            new Vector3(_positionAssemble.x + m_GameDatas.DistanceAssemble, _positionAssemble.y, _positionAssemble.z - m_GameDatas.DistanceAssemble),
                            m_GameDatas.TdlRight, m_GameDatas.Ttc, m_GameDatas.TimeTurnBack,m_GameDatas.DistanceAssemble,-m_GameDatas.DistanceAssemble);
                        m_MemberBack.TurnBack(
                            new Vector3(_positionAssemble.x + m_GameDatas.DistanceAssemble * 2, _positionAssemble.y, _positionAssemble.z + m_GameDatas.DistanceAssemble * 2),
                            m_GameDatas.TdlBack, m_GameDatas.Ttc, m_GameDatas.TimeTurnBack,m_GameDatas.DistanceAssemble*2,m_GameDatas.DistanceAssemble*2);
                        m_MapView.DOFade(0, 1).SetDelay(m_GameDatas.TdlBack + 5).OnComplete(EndGame);
                        break;
                }

                break;
        }
    }

    private void EndGame()
    {
        StartCoroutine(COEndGame());
    }

    private IEnumerator COEndGame()
    {
        yield return new WaitForSeconds(15);
        FindObjectOfType<CloudsFollow>().End();
        m_MainCamera.Follow = null;
    }
    
    public class GameManagerData
    {
        public float m_CloudsHeight;
        public float m_TimeWaitPlane;
        public float Ttc;
        public float TimeTurnBack;
        public float TdlLeft;
        public float TdlRight;
        public float TdlBack;
        public float DistanceAssemble;
        public float _positionAssembleX;
        public float _positionAssembleY;
        public float _positionAssembleZ;
    }
}