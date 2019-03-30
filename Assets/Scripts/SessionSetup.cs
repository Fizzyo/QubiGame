using Fizzyo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SessionSetup : MonoBehaviour
{
    public Text BreathDisplay;
    public Text SetsDisplay;

    private void Start()
    {
        if (ScoreManager.Instance.currentStage != ScoreManager.GameStage.Paused)
        {
            BreathDisplay.text = FizzyoFramework.Instance.Session.SessionBreathCount.ToString();
            SetsDisplay.text = FizzyoFramework.Instance.Session.SessionSetCount.ToString();
        }
    }

    private void Update()
    {
        if (ScoreManager.Instance.currentStage != ScoreManager.GameStage.Paused)
        {
            BreathDisplay.text = FizzyoFramework.Instance.Session.SessionBreathCount.ToString();
            SetsDisplay.text = FizzyoFramework.Instance.Session.SessionSetCount.ToString();
        }
    }

    public void IncrementBreathCount(int increment)
    {
        FizzyoFramework.Instance.Session.SessionBreathCount += increment;
        FizzyoFramework.Instance.Session.SessionBreathCount = Mathf.Clamp(FizzyoFramework.Instance.Session.SessionBreathCount, 1, 90);
        BreathDisplay.text = FizzyoFramework.Instance.Session.SessionBreathCount.ToString();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void IncrementSetCount(int increment)
    {
        FizzyoFramework.Instance.Session.SessionSetCount += increment;
        FizzyoFramework.Instance.Session.SessionSetCount = Mathf.Clamp(FizzyoFramework.Instance.Session.SessionSetCount, 1, 90);
        SetsDisplay.text = FizzyoFramework.Instance.Session.SessionSetCount.ToString();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResumeGame()
    {
        ScoreManager.Instance.ResumeGame();
    }

    public void NewGame()
    {
        if (ScoreManager.Instance.currentStage == ScoreManager.GameStage.GameEnd)
        {
            ScoreManager.Instance.ButtonPressed();
        }
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}