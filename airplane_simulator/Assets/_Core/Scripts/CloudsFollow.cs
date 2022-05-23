using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsFollow : MonoBehaviour
{
    public Transform m_Captain;
    private bool end;
    private void LateUpdate()
    {
        if(end) return;
        transform.position = new Vector3(m_Captain.transform.position.x, 386, 0);
    }

    public void End()
    {
        end = true;
    }
}
