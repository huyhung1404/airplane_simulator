using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationPlane : MonoBehaviour
{
    [SerializeField] private GameObject m_GearRoom;
    [SerializeField] private Transform m_LeftGearRoom;
    [SerializeField] private Transform m_RightGearRoom;
    [SerializeField] private Transform m_NoseGearRoom;
    [SerializeField] private float m_TimeCloseGearRoom;

    public void StartFly()
    {
        m_LeftGearRoom.DORotate(new Vector3(0, 0, 90), m_TimeCloseGearRoom);
        m_RightGearRoom.DORotate(new Vector3(0, 0, -90), m_TimeCloseGearRoom);
        m_NoseGearRoom.DORotate(new Vector3(0, -90, 0), m_TimeCloseGearRoom).OnComplete(() =>
        {
            m_GearRoom.gameObject.SetActive(false); 
        });
    }
}
