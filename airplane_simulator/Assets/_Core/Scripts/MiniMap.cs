using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private RectTransform m_DotCaptain;
    [SerializeField] private Transform m_GraphicCaptain;
    [SerializeField] private RectTransform m_DotMemberLeft;
    [SerializeField] private Transform m_GraphicMemberLeft;
    [SerializeField] private RectTransform m_DotMemberRight;
    [SerializeField] private Transform m_GraphicMemberRight;
    [SerializeField] private RectTransform m_DotMemberBack;
    [SerializeField] private Transform m_GraphicMemberBack;
    
    private float Width = 5000;
    private float StartWidth = -100;
    private float Height = 1000;
    private float WidthMap;
    private float HeightMap;

    private void Start()
    {
        var rect = GetComponent<RectTransform>().sizeDelta;
        WidthMap = rect.x;
        HeightMap = rect.y;
    }

    private void Update()
    {
        m_DotCaptain.anchoredPosition = HandlePosition(m_GraphicCaptain.position);
        m_DotMemberLeft.anchoredPosition = HandlePosition(m_GraphicMemberLeft.position);
        m_DotMemberRight.anchoredPosition = HandlePosition(m_GraphicMemberRight.position);
        m_DotMemberBack.anchoredPosition = HandlePosition(m_GraphicMemberBack.position);
    }

    private Vector2 HandlePosition(Vector2 pos)
    {
        var ratioX = (pos.x - StartWidth) / Width;
        var ratioY = pos.y / Height;
        return new Vector2(ratioX * WidthMap,  ratioY * HeightMap);
    }
}
