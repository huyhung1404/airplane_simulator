using System;
using UnityEngine;

public class SimulatorFlight : MonoBehaviour
{
    private Rigidbody m_Rigidbody;
    public Vector2 m_Velocity;
    public float VanToc;
    private Vector3 m_LastPosition;
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_LastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        m_Rigidbody.velocity = m_Velocity;
        var updateRotation = Quaternion.Euler(Vector3.forward * Vector2.SignedAngle(Vector2.right,m_Velocity));
        m_Rigidbody.rotation = Quaternion.Lerp(m_Rigidbody.rotation,updateRotation,0.8f);

        // VanToc = (transform.position - m_LastPosition).sqrMagnitude / Time.fixedDeltaTime;
        m_LastPosition = transform.position;
    }
}
