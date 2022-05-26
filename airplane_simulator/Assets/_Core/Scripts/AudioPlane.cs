using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlane : MonoBehaviour
{
    [SerializeField] private AudioClip m_StartClip;
    [SerializeField] private AudioClip m_FlyClip;
    [SerializeField] private AudioClip m_Audio1;
    [SerializeField] private AudioClip m_Audio2;
    
    private AudioSource m_Audio;

    public static AudioPlane instance;

    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        instance = this;
    }

    public void PlayAudioStart()
    {
        m_Audio.PlayOneShot(m_StartClip);
    }

    public void PlayAudioFly()
    {
        m_Audio.PlayOneShot(m_FlyClip);
        StartCoroutine(LoopAudio());
    }

    private IEnumerator LoopAudio()
    {
        yield return new WaitForSeconds(30);
        m_Audio.PlayOneShot(m_FlyClip);
        StartCoroutine(LoopAudio());
    }
}
