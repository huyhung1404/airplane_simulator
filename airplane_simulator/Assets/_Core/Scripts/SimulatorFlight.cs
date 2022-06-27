using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public enum State
{
    GetAltitude,
    Stable,
    Turning,
    Back,
    None,
    Assemble
}

public class SimulatorFlight : MonoBehaviour
{
    public string fileName;
    private FlightData m_Data;
    [Header("Graphic")] [SerializeField] private Transform m_Graphic;
    [SerializeField] private AnimationPlane m_Animation;
    [Header("Phase 0")]
    [SerializeField] private AnimationCurve Curve0;
    [Header("Phase 1")]
    [SerializeField] private AnimationCurve Curve1;
    [Header("Phase 2")]
    [SerializeField] private AnimationCurve Curve2;
    [Header("Phase 3")]
    [SerializeField] private AnimationCurve Curve3;
    [Header("Phase 4")]
    [SerializeField] private AnimationCurve Curve5;
    [SerializeField] private AnimationCurve Curve6;
    [SerializeField] private AnimationCurve Curve7;
    [SerializeField] private AnimationCurve m_StartCurve;

    private Vector3 m_LastVelocity;

    [SerializeField] private AnimationCurve m_AssemblyCurve;
    private Rigidbody m_Rigidbody;
    private State m_CurrentState;
    private Vector3 m_LastGraphicPosition;
    private Quaternion m_NextGraphicRotation;
    public static Vector3 m_HoldingVelocity;
    private Vector3 m_BackVelocity;
    private Role m_Role;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        ReadJson();
        m_CurrentState = State.None;
    }
    
    public void ReadJson(){
        string path = $"{Application.streamingAssetsPath}/{fileName}.json";
        string contents = File.ReadAllText(path);
        m_Data = JsonUtility.FromJson<FlightData>(contents);
        m_LastVelocity = new Vector3(m_Data.lastVelocity,0,0);
    }

    public void StartSimulator(Role _role,float _timeStart)
    {
        m_Role = _role;
        var plane = transform.parent;
        var _pos1 = new Vector3(plane.position.x, 0, plane.position.z + 10);
        var _pos2 = new Vector3(-90.5f, 0, -5.5f);
        var _pos3 = new Vector3(-90.5f, 0, 21f);
        var _pos4 = new Vector3(-82.5f, 0, 25.38f);
        var _pos5 = new Vector3(-80.5f, 0, 25.38f);
        var path = new[]
        {
            _pos1, _pos1, _pos1,

            _pos2, m_Role == Role.Captain ? new Vector3(_pos1.x, 0, -8f) : new Vector3(_pos1.x, 0, -5.5f),
            m_Role == Role.Captain ? new Vector3(_pos2.x, 0, -8f) : new Vector3(_pos2.x, 0, _pos1.z),

            _pos3, _pos3, _pos3,

            _pos4, new Vector3(_pos3.x, 0, 24.5f), new Vector3(-85.5f, 0, _pos4.z),

            _pos5, _pos5, _pos5
        };
        plane.DOPath(path, _timeStart, PathType.CubicBezier).SetEase(m_StartCurve).OnComplete(
            () =>
            {
                m_CurrentState = State.GetAltitude;
                StartFlight();
            }).SetLookAt(0.01f);
    }

    private void StartFlight()
    {
        if(m_Role == Role.Captain) AudioPlane.instance.PlayAudioCatCanh();
        m_Rigidbody.DOMoveX(m_Rigidbody.position.x + m_Data.S0, m_Data.T0).SetEase(Curve0).OnComplete(() =>
        {
            m_Animation.StartFly();
            if(m_Role == Role.Captain) AudioPlane.instance.PlayAudioBay();
            m_Rigidbody
                .DOMove(new Vector3(m_Rigidbody.position.x + m_Data.Sat, m_Rigidbody.position.y + m_Data.h, m_Rigidbody.position.z),
                    m_Data.T1).SetEase(Curve1).OnComplete(() =>
                {
                    m_Rigidbody.DOMoveX(m_Rigidbody.position.x + m_Data.Sbb, m_Data.T2).SetEase(Curve2).OnComplete(() =>
                    {
                        m_Rigidbody.DOMove(
                            new Vector3(m_Rigidbody.position.x + m_Data.SH, m_Rigidbody.position.y + m_Data.H,
                                m_Rigidbody.position.z), m_Data.T3).SetEase(Curve3).OnComplete(() =>
                        {
                            m_CurrentState = State.Stable;
                            if (m_Role == Role.Captain)
                            {
                                FindObjectOfType<MiniMap>().ChangeWidth();
                                m_HoldingVelocity = m_LastVelocity;
                            }
                            GameManager.Instance.PlaneStable();
                        });
                    });
                });
        });
    }

    private void FixedUpdate()
    {
        switch (m_CurrentState)
        {
            case State.None:
                m_LastGraphicPosition = m_Graphic.position;
                return;
            case State.Stable:
                m_Rigidbody.velocity = m_HoldingVelocity;
                break;
            case State.Back:
                m_Rigidbody.velocity = m_BackVelocity;
                m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
                m_LastGraphicPosition = m_Graphic.position;
                return;
            case State.Turning:
                m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
                m_LastGraphicPosition = m_Graphic.position;
                return;
        }

        m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
        m_NextGraphicRotation = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right, m_Graphic.position - m_LastGraphicPosition));
        m_Graphic.rotation = Quaternion.Lerp(m_Graphic.rotation, m_NextGraphicRotation, 0.15f);
        m_LastGraphicPosition = m_Graphic.position;
    }

    public void TurnBack(Vector3 _pos,float _timeStart,float _timeDuration,float _timeTurnBack,float _offsetX,float _offsetZ)
    {
        StartCoroutine(COTurnBack(_pos,_timeStart,_timeDuration,_timeTurnBack,_offsetX,_offsetZ));
    }

    private IEnumerator COTurnBack(Vector3 _pos,float _timeStart,float _timeDuration,float _timeTurnBack,float _offsetX,float _offsetZ)
    {
        yield return new WaitForSeconds(_timeStart);
        m_CurrentState = State.Turning;
        var currentPosition = transform.position;
        Vector3[] path;
        if (m_Role == Role.Captain)
        {
            path = new[]
            {
                new Vector3(currentPosition.x + 100, currentPosition.y + 100, currentPosition.z + 100),
                new Vector3(currentPosition.x + 50, currentPosition.y, currentPosition.z),
                new Vector3(currentPosition.x + 100, currentPosition.y + 25, currentPosition.z),
            
                new Vector3(currentPosition.x + _offsetX, currentPosition.y + 200, currentPosition.z + 200 + _offsetZ),
                new Vector3(currentPosition.x + _offsetX + 100, currentPosition.y + 150, currentPosition.z + 200 + _offsetZ),
                new Vector3(currentPosition.x + _offsetX + 50, currentPosition.y + 200, currentPosition.z + 200 + _offsetZ),
            };
        }
        else
        {
            path = new[]
            {
                new Vector3(currentPosition.x + 200, currentPosition.y + 100, currentPosition.z + 100),
                new Vector3(currentPosition.x + 100, currentPosition.y, currentPosition.z),
                new Vector3(currentPosition.x + 200, currentPosition.y + 25, currentPosition.z),
            
                new Vector3(currentPosition.x + _offsetX, currentPosition.y + 200, currentPosition.z + 200 + _offsetZ),
                new Vector3(currentPosition.x + _offsetX + 200, currentPosition.y + 150, currentPosition.z + 200 + _offsetZ),
                new Vector3(currentPosition.x + _offsetX + 100, currentPosition.y + 200, currentPosition.z + 200 + _offsetZ),
            };
        }
        
        
        m_Rigidbody.DOPath(path, _timeTurnBack, PathType.CubicBezier).SetEase(Curve5).OnComplete(() =>
        {
            m_Rigidbody.DOMove(_pos, _timeDuration - _timeStart  - _timeTurnBack).SetEase(m_AssemblyCurve).OnComplete(() =>
            {
                m_BackVelocity = m_HoldingVelocity * -1;
                m_CurrentState = State.Back;
            });
        });
        m_Graphic.DORotate(new Vector3(40, -90, 0), _timeTurnBack * 0.5f).SetEase(Curve6).OnComplete(() =>
        {
            m_Graphic.DORotate(new Vector3(10, -180, 0), _timeTurnBack * 0.5f).SetEase(Curve7).OnComplete(() =>
            {
                m_Graphic.DORotate(new Vector3(0, -180, 0), _timeTurnBack);
            });
        });
    }

    public void Assemble(Vector3 position, float time)
    {
        m_CurrentState = State.Assemble;
        m_Rigidbody.DOMove(position, time).SetEase(m_AssemblyCurve).OnComplete(() =>
        {
            m_CurrentState = State.Stable;
        });
    }
}

public class FlightData
{
    public float S0;
    public float T0;
    public float h;
    public float Sat;
    public float T1;
    public float Sbb;
    public float T2;
    public float H;
    public float SH;
    public float T3;
    public float lastVelocity;
}