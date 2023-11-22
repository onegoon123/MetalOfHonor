using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    public static Option Instance {
        get {
            if (instance == null) instance = FindObjectOfType<Option>();
            DontDestroyOnLoad(instance);
            return instance;
        }
    }

    private static Option instance;

    public float mouseSenservity;
    public float stickSenservity;
    public Slider ms;
    public Slider ss;
    public Dropdown gd;

    private void Awake() {
        ms.value = PlayerPrefs.GetFloat("mouse", 0.5f);
        mouseSenservity = PlayerPrefs.GetFloat("mouse", 0.5f);

        ss.value = PlayerPrefs.GetFloat("stick", 2f);
        stickSenservity = PlayerPrefs.GetFloat("stick", 2f);

        gd.value = PlayerPrefs.GetInt("graphic", 1);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("graphic", 1));

    }
    public void mouseSet(float s) {
        PlayerPrefs.SetFloat("mouse", s);
        mouseSenservity = s;
    }
    public void stickSet(float s) {
        PlayerPrefs.SetFloat("stick", s);
        stickSenservity = s;
    }
    public void Graphic(int g) {
        PlayerPrefs.SetInt("graphic", g);
        QualitySettings.SetQualityLevel(g);
    }
}
