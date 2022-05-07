using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform m_Target;
    private Vector3 m_Offset;
    
    private void Start()
    {
        m_Offset = m_Target.position - transform.position;
    }

    private void LateUpdate()
    {
        transform.position = m_Target.position - m_Offset;
    }
}
