using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameVideoManager : MonoBehaviour
{
    [SerializeField] private TMP_Text m_TimeText;
    private float m_TimeScale = 1;
    private bool m_Pause;
    public GameObject m_Button;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (m_Pause)
            {
                Time.timeScale = m_TimeScale;
                m_Pause = false;
            }
            else
            {
                Time.timeScale = 0;
                m_Pause = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            m_TimeScale -= 0.5f;
            if (m_TimeScale < 0) m_TimeScale = 0;
            Time.timeScale = m_TimeScale;
            CheckText();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            m_TimeScale += 0.5f;
            Time.timeScale = m_TimeScale;
            CheckText();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_TimeScale = 1;
            Time.timeScale = m_TimeScale;
            CheckText();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_Button.SetActive(true);
            m_TimeScale = 1;
            Time.timeScale = m_TimeScale;
            CheckText();
            SceneManager.LoadScene(0);
        }
    }

    private void CheckText()
    {
        if (m_TimeScale == 0 || m_TimeScale == 1)
        {
            m_TimeText.text = string.Empty;
            return;
        }
        m_TimeText.text = $"X{m_TimeScale}";
    }

    public void OnFlyForwardButtonTap()
    {
        m_Button.SetActive(false);
        SceneManager.LoadScene(1);
    }

    public void OnFlyBackButtonTap()
    {
        m_Button.SetActive(false);
        SceneManager.LoadScene(2);
    }
}
