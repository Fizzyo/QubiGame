using Fizzyo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FizzyoBreath : MonoBehaviour
{
    public float breathVolume;

    public Image OuterBar;
    public float OuterBarFill = 0f;
    public Image InnerBar;
    public float InnerBarFill = 0f;

    public float maxPressure = 0.4f;
    public float maxBreathLength = 3f;

    // Use this for initialization
    void Start()
    {
        //FizzyoFramework.Instance.Recogniser.MaxBreathLength = maxBreathLength;
        //FizzyoFramework.Instance.Recogniser.MaxPressure = maxPressure;
        FizzyoFramework.Instance.Recogniser.BreathComplete += BreathAnalyser_ExhalationComplete;
    }

    private void BreathAnalyser_ExhalationComplete(object sender, ExhalationCompleteEventArgs e)
    {
        if (ScoreManager.Instance.GameStarted)
        {
            if (e.IsBreathFull)
            {
                ScoreManager.Instance.GoodBreathAnimation();
            }
            else
            {
                ScoreManager.Instance.BadBreathAnimation();
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        FizzyoFramework.Instance.Recogniser.AddSample(Time.deltaTime, FizzyoFramework.Instance.Device.Pressure());

        breathVolume = FizzyoFramework.Instance.Recogniser.ExhaledVolume;

        // Set Visuals
        OuterBarFill = FizzyoFramework.Instance.Recogniser.BreathLength / FizzyoFramework.Instance.Recogniser.MaxBreathLength;
        OuterBar.fillAmount = OuterBarFill;

        InnerBarFill = (float)ScoreManager.Instance.CurrentLevel.GoodBreathCount / (float)ScoreManager.Instance.CurrentLevel.GoodBreathMax;
        InnerBar.fillAmount = InnerBarFill;
    }

    private void OnDestroy()
    {
        if (FizzyoFramework.Instance.Recogniser != null)
        {
            FizzyoFramework.Instance.Recogniser.BreathComplete -= BreathAnalyser_ExhalationComplete;
        }
    }
}
