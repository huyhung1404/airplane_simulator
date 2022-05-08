using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public enum State
{
    GetAltitude,
    Stable,
    Turning,
    Back
}
public class SimulatorFlight : MonoBehaviour
{
    [SerializeField] private float Velocity;
    [Header("Graphic")] 
    [SerializeField] private Transform m_Graphic;
    [SerializeField] private AnimationPlane m_Animation;
    [Header("Phase 0")] 
    [SerializeField] private float S0;
    [SerializeField] private float T0;
    [SerializeField] private AnimationCurve Curve0;
    [Header("Phase 1")] 
    [SerializeField] private float h;
    [SerializeField] private float Sat;
    [SerializeField] private float T1;
    [SerializeField] private AnimationCurve Curve1;
    [Header("Phase 2")] 
    [SerializeField] private float Sbb;
    [SerializeField] private float T2;
    [SerializeField] private AnimationCurve Curve2;
    [Header("Phase 3")] 
    [SerializeField] private float H;
    [SerializeField] private float SH;
    [SerializeField] private float T3;
    [SerializeField] private AnimationCurve Curve3;
    [Header("Phase 4")] 
    [SerializeField] private bool UsePhase4;
    [SerializeField] private float m_TimeStartPhase;
    [SerializeField] private float R;
    [SerializeField] private float T4;
    [SerializeField] private float Depth;
    [SerializeField] private AnimationCurve Curve5;
    [SerializeField] private AnimationCurve Curve6;
    [SerializeField] private AnimationCurve Curve7;

    private Rigidbody m_Rigidbody;
    private State m_CurrentState;
    private Vector3 m_LastGraphicPosition;
    private Quaternion m_NextGraphicRotation;
    private Vector3 m_HoldingVelocity;
    private IEnumerator Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_LastGraphicPosition = m_Graphic.position;
        m_CurrentState = State.GetAltitude;
        yield return new WaitForSeconds(1);
        StartFlight();
    }

    private void StartFlight()
    {
        m_Rigidbody.DOMoveX(m_Rigidbody.position.x + S0, T0).SetEase(Curve0).OnComplete(() =>
        {
            m_Animation.StartFly();
            m_Rigidbody.DOMove(new Vector3(m_Rigidbody.position.x + Sat,m_Rigidbody.position.y + h,m_Rigidbody.position.z), T1).SetEase(Curve1).OnComplete(() =>
            {
                m_Rigidbody.DOMoveX(m_Rigidbody.position.x + Sbb, T2).SetEase(Curve2).OnComplete(() =>
                {
                    m_Rigidbody.DOMove(new Vector3(m_Rigidbody.position.x + SH,m_Rigidbody.position.y + H,m_Rigidbody.position.z), T3).SetEase(Curve3).OnComplete(() =>
                    {
                        m_CurrentState = State.Stable;
                        m_HoldingVelocity = Velocity * Vector3.right;
                        if (UsePhase4)
                        {
                            StartCoroutine(Phase5());
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
            case State.Stable:
                m_Rigidbody.velocity = m_HoldingVelocity;
                break;
            case State.Back:
                m_Rigidbody.velocity = m_HoldingVelocity;
                m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
                Velocity = (m_Graphic.position - m_LastGraphicPosition).magnitude / Time.fixedDeltaTime;
                m_LastGraphicPosition = m_Graphic.position;
                return;
            case State.Turning:
                m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
                Velocity = (m_Graphic.position - m_LastGraphicPosition).magnitude / Time.fixedDeltaTime;
                m_LastGraphicPosition = m_Graphic.position;
                return;
        }
        
        m_Graphic.position = Vector3.Lerp(m_LastGraphicPosition, transform.position, 0.05f);
        Velocity = (m_Graphic.position - m_LastGraphicPosition).magnitude / Time.fixedDeltaTime;
        m_NextGraphicRotation = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right,m_Graphic.position - m_LastGraphicPosition));
        m_Graphic.rotation = Quaternion.Lerp(m_Graphic.rotation,m_NextGraphicRotation,0.15f);
        m_LastGraphicPosition = m_Graphic.position;
    }

    private IEnumerator Phase5()
    {
        yield return new WaitForSeconds(m_TimeStartPhase);
        m_CurrentState = State.Turning;
        var currentPosition = transform.position;
        // m_Rigidbody.velocity = Vector3.zero;
        var path = new[]
        {
            currentPosition,
            new Vector3(currentPosition.x + R,currentPosition.y,Depth),
            new Vector3(currentPosition.x,currentPosition.y, Depth * 2)
        };

        m_Rigidbody.DOPath(path, T4, PathType.CatmullRom).SetEase(Curve5).OnComplete(() =>
        {
            m_CurrentState = State.Back;
            m_HoldingVelocity = Velocity * Vector3.left;
        });
        m_Graphic.DORotate(new Vector3(55, -90, 0), T4*0.5f).SetEase(Curve6).OnComplete(() =>
        {
            m_Graphic.DORotate(new Vector3(20, -180, 0), T4*0.5f).SetEase(Curve7).OnComplete(() =>
            {
                m_Graphic.DORotate(new Vector3(0, -180, 0), T4);
            });
        });
        
    }
}
