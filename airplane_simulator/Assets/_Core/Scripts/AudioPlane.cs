using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlane : MonoBehaviour
{
    [SerializeField] private AudioClip m_KhoiDong;
    [SerializeField] private AudioClip m_CatCanh;
    [SerializeField] private AudioClip m_Bay;
    [SerializeField] private AudioSource[] m_ChildAudio;
    
    private AudioSource m_Audio;

    public static AudioPlane instance;

    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        instance = this;
    }

    public void PlayAudioKhoiDong()
    {
        m_Audio.clip = m_KhoiDong;
        m_Audio.Play();
    }

    public void PlayAudioCatCanh()
    {
        m_Audio.clip = m_CatCanh;
        m_Audio.volume = 0.8f;
        m_Audio.Play();
    }

    public void PlayAudioBay()
    {
        m_Audio.clip = m_Bay;
        m_Audio.volume = 1f;
        m_Audio.Play();
        m_Audio.loop = true;
    }

    public void PlayAudioAssembly()
    {
        for (int i = m_ChildAudio.Length - 1; i >= 0; i--)
        {
            m_ChildAudio[i].Play();
        }
    }
}
