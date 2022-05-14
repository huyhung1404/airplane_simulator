using System;
using System.Collections;
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
    [Header("Graphic")] [SerializeField] private Transform m_Graphic;
    [SerializeField] private AnimationPlane m_Animation;
    [Header("Phase 0")] [SerializeField] private float S0;
    [SerializeField] private float T0;
    [SerializeField] private AnimationCurve Curve0;
    [Header("Phase 1")] [SerializeField] private float h;
    [SerializeField] private float Sat;
    [SerializeField] private float T1;
    [SerializeField] private AnimationCurve Curve1;
    [Header("Phase 2")] [SerializeField] private float Sbb;
    [SerializeField] private float T2;
    [SerializeField] private AnimationCurve Curve2;
    [Header("Phase 3")] [SerializeField] private float H;
    [SerializeField] private float SH;
    [SerializeField] private float T3;
    [SerializeField] private AnimationCurve Curve3;
    [Header("Phase 4")] [SerializeField] private float m_TimeStartPhase;
    [SerializeField] private float R;
    [SerializeField] private float T4;
    [SerializeField] private float Depth;
    [SerializeField] private AnimationCurve Curve5;
    [SerializeField] private AnimationCurve Curve6;
    [SerializeField] private AnimationCurve Curve7;
    [SerializeField] private AnimationCurve m_StartCurve;


    [SerializeField] private AnimationCurve m_AssemblyCurve;
    private Rigidbody m_Rigidbody;
    private State m_CurrentState;
    private Vector3 m_LastGraphicPosition;
    private Quaternion m_NextGraphicRotation;
    public static Vector3 m_HoldingVelocity;
    private TypeSimulator m_TypeSimulator;
    private Role m_Role;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentState = State.None;
    }

    public void StartSimulator(TypeSimulator _type, Role _role)
    {
        m_TypeSimulator = _type;
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
        plane.DOPath(path, 20, PathType.CubicBezier).SetEase(m_StartCurve).OnComplete(
            () =>
            {
                m_CurrentState = State.GetAltitude;
                StartFlight();
            }).SetLookAt(0.01f);
    }

    private void StartFlight()
    {
        m_Rigidbody.DOMoveX(m_Rigidbody.position.x + S0, T0).SetEase(Curve0).OnComplete(() =>
        {
            m_Animation.StartFly();
            m_Rigidbody
                .DOMove(new Vector3(m_Rigidbody.position.x + Sat, m_Rigidbody.position.y + h, m_Rigidbody.position.z),
                    T1).SetEase(Curve1).OnComplete(() =>
                {
                    m_Rigidbody.DOMoveX(m_Rigidbody.position.x + Sbb, T2).SetEase(Curve2).OnComplete(() =>
                    {
                        m_Rigidbody.DOMove(
                            new Vector3(m_Rigidbody.position.x + SH, m_Rigidbody.position.y + H,
                                m_Rigidbody.position.z), T3).SetEase(Curve3).OnComplete(() =>
                        {
                            m_CurrentState = State.Stable;
                            if (m_Role == Role.Captain)
                            {
                                m_HoldingVelocity = GameManager.CaptainVelocity * Vector3.right;
                            }

                            switch (m_TypeSimulator)
                            {
                                case TypeSimulator.Straight:
                                    GameManager.Instance.PlaneStable();
                                    break;
                                case TypeSimulator.Back:
                                    StartCoroutine(Phase5());
                                    break;
                            }
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
                m_Rigidbody.velocity = m_HoldingVelocity;
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

    private IEnumerator Phase5()
    {
        yield return new WaitForSeconds(m_TimeStartPhase);
        m_CurrentState = State.Turning;
        var currentPosition = transform.position;
        var path = new[]
        {
            currentPosition,
            new Vector3(currentPosition.x + R, currentPosition.y, Depth),
            new Vector3(currentPosition.x, currentPosition.y, Depth * 2)
        };

        m_Rigidbody.DOPath(path, T4, PathType.CatmullRom).SetEase(Curve5).OnComplete(() =>
        {
            m_CurrentState = State.Back;
            m_HoldingVelocity = GameManager.CaptainVelocity * Vector3.left;
        });
        m_Graphic.DORotate(new Vector3(55, -90, 0), T4 * 0.5f).SetEase(Curve6).OnComplete(() =>
        {
            m_Graphic.DORotate(new Vector3(20, -180, 0), T4 * 0.5f).SetEase(Curve7).OnComplete(() =>
            {
                m_Graphic.DORotate(new Vector3(0, -180, 0), T4);
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