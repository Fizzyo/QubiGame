using Fizzyo;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public bool GameStarted = false;
    public bool GameIsPaused = false;
    public bool GameIsPausedByBreath = false;
    public bool GamePausedDueToBreathingStop = false;

    public enum GameStage { SessionSetup, LevelPlaying, LevelEnd, GameEnd, Paused }
    public GameStage currentStage = GameStage.SessionSetup;


    private GameStage pausedStage;

    // player prefs to load
    public int CoinHighScore = 0;
    private string dateLastPlayed = string.Empty;
    private int daysPlayed = 0;
    // Levels
    public int CurrentLevelIndex;
    public PlatformLevel CurrentLevel;
    public List<PlatformLevel> Levels;

    // UI
    public GameObject HUD;
    public GameObject LevelSetupUI;
    public GameObject LevelEndUI;
    public GameObject GameEndUI;
    public GameObject PauseUI;
    public GameObject LevelEndTimer;
    public GameObject GameEndTimer;

    // In-Game objects
    public GameObject Player;
    public GameObject LevelEndPrefab;
    private GameObject levelEnd;
    private Text TimerText;

    // Particle Effects
    public GameObject GoodBreathParticles;
    public GameObject BadBreathParticles;

    // Audio
    public AudioSource GoodBreathSound;
    public AudioSource BadBreathSound;
    public AudioSource LevelEndSound;
    public AudioSource GameEndSound;
    public AudioSource CoinEffect;

    public AudioSource BackgroundMenuMusic;

    // Save keys
    private string coinHighScoreKey = "coinHighScore";
    private string dateLastPlayedKey = "dateLastPlayedKey";
    private string daysPlayedKey = "daysPlayed";


    // Events
    public delegate void LevelResetEventHandler();
    public event LevelResetEventHandler LevelStartEvent;
    public event LevelResetEventHandler LevelEndEvent;
    public event LevelResetEventHandler GameEndEvent;

    //In Game background music manager
    public BackgroundMusicManager backgroundMusicManager;

    //Game attributes
    private float pauseTimer = 0;
    public float pauseTime = 5f;
    private bool inputActive = false;

    private GameAchievements[] Achievements;

    // First thing to be called
    private void Awake()
    {
        Instance = this;
        Achievements = GetGameAchievements;
    }

    // Called at the start
    private void Start()
    {
        FizzyoFramework.Instance.Session.SessionPaused += Session_SessionPaused;
        FizzyoFramework.Instance.Session.SessionResumed += Session_SessionResumed;
        LoadPlayerHighScore();

        LevelSetupUI.SetActive(true);
        HUD.SetActive(false);
        LevelEndUI.SetActive(false);
        GameEndUI.SetActive(false);
        PauseUI.SetActive(false);
        StartCoroutine(AudioFader.FadeIn(BackgroundMenuMusic, 5f));

        CheckStartupAchievements();
    }

    private void CheckStartupAchievements()
    {
        long dateLastPlayedUTC = 0;
        long.TryParse(dateLastPlayed, out dateLastPlayedUTC);
        TimeSpan t = DateTime.Now - DateTime.FromFileTimeUtc(dateLastPlayedUTC);
        var daysSinceLastPlayed = t.Days;
        if (daysSinceLastPlayed > 0)
        {
            dateLastPlayed = DateTime.Now.ToFileTimeUtc().ToString();
            daysPlayed += 1;
        }
        for (int i = 0; i < Achievements.Length; i++)
        {
            //See if the user has surpassed any achievement requirements
            if (Achievements[i].DayRequirement > 0 && daysPlayed > Achievements[i].DayRequirement)
            {
               FizzyoFramework.Instance.Achievements.CheckAndUnlockAchivement(Achievements[i].AchievementName);
            }
        }
    }


    private void Session_SessionResumed(object sender, SessionEventArgs e)
    {
        GamePausedDueToBreathingStop = false;
    }

    private void Session_SessionPaused(object sender, SessionEventArgs e)
    {
        GamePausedDueToBreathingStop = true;
    }

    #region SaveLoad
    // Loads the player prefs
    public void LoadPlayerHighScore()
    {
        if (PlayerPrefs.HasKey(coinHighScoreKey))
        {
            CoinHighScore = PlayerPrefs.GetInt(coinHighScoreKey);
        }

        if (PlayerPrefs.HasKey(dateLastPlayedKey))
        {
            dateLastPlayed = PlayerPrefs.GetString(dateLastPlayedKey);
        }
        else
        {
            //If there is no date retrieved, this is the first time.
            FizzyoFramework.Instance.Achievements.CheckAndUnlockAchivement(Achievements[0].AchievementName);
        }

        if (PlayerPrefs.HasKey(daysPlayedKey))
        {
            daysPlayed = PlayerPrefs.GetInt(daysPlayedKey);
        }
    }

    // If it's a new high score, save it
    public void CheckHighScore()
    {
        //if this is our first day and we've completed the a session, give them a prize.
        if (daysPlayed == 0)
        {
            FizzyoFramework.Instance.Achievements.CheckAndUnlockAchivement(Achievements[1].AchievementName);
        }

        PlayerPrefs.SetString(dateLastPlayedKey, dateLastPlayed);
        PlayerPrefs.SetInt(daysPlayedKey, daysPlayed);

        if (TotalCoins() > CoinHighScore)
        {
            CoinHighScore = TotalCoins();
            PlayerPrefs.SetInt(coinHighScoreKey, CoinHighScore);
        }
        PlayerPrefs.Save();

        for (int i = 0; i < Achievements.Length; i++)
        {
            //See if the user has surpassed any achievement requirements
            if (Achievements[i].ScoreRequirement > 0 && TotalCoins() > Achievements[i].ScoreRequirement)
            {
                FizzyoFramework.Instance.Achievements.CheckAndUnlockAchivement(Achievements[i].AchievementName);
            }
        }
    }
    #endregion

    // Called once per frame
    private void Update()
    {
        if (currentStage == GameStage.SessionSetup && Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (currentStage == GameStage.LevelPlaying)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                PauseGame();
            }

            if (GamePausedDueToBreathingStop)
            {
                GameIsPausedByBreath = true;
                PauseGame();
            }

            CurrentLevel.LevelTime += Time.deltaTime;

            if (CurrentLevel.GoodBreathCount >= CurrentLevel.GoodBreathMax && levelEnd == null)
            {
                CreateLevelEnd();
            }
        }

        if (!inputActive && (currentStage == GameStage.LevelEnd || currentStage == GameStage.GameEnd))
        {
            GameStarted = false;
            inputActive = CheckPause();
            if (TimerText)
            {
                TimerText.gameObject.SetActive(true);
                TimerText.text = (Mathf.Floor(pauseTime) - Mathf.Floor(pauseTimer)).ToString();
            }
        }
        else if (TimerText)
        {
            TimerText.gameObject.SetActive(false);
        }

        if (Fizzyo.FizzyoFramework.Instance.Device.ButtonDown())
        {
            ButtonPressed();
        }

        if (GameIsPausedByBreath && !GamePausedDueToBreathingStop)
        {
            ResumeGame();
        }
    }

    #region Breaths

    public void GoodBreathAnimation()
    {
        GoodBreathSound.Stop();
        GoodBreathSound.Play();

        CurrentLevel.GoodBreathCount++;

        if (CurrentLevel.GoodBreathCount % 2 == 0)
        {
            backgroundMusicManager.PlayNextLevel(CurrentLevel.GoodBreathCount / 2);
        }

        GameObject particles = Instantiate(GoodBreathParticles);
        particles.transform.position = Player.transform.GetChild(0).position;
        particles.transform.parent = Player.transform;
        Destroy(particles, 2f);
    }

    public void BadBreathAnimation()
    {
        BadBreathSound.Stop();
        BadBreathSound.Play();

        CurrentLevel.BadBreathCount++;

        GameObject particles = Instantiate(BadBreathParticles);
        particles.transform.position = Player.transform.GetChild(0).position;
        particles.transform.parent = Player.transform;
        Destroy(particles, 2f);
    }

    #endregion

    #region LevelFunctions

    // handle button presses depending on current game stage
    public void ButtonPressed()
    {
        switch (currentStage)
        {
            case GameStage.SessionSetup:
                FizzyoFramework.Instance.Session.StartSession(true);
                CreateLevels();
                StartNewLevel();
                ScoreManager.Instance.GameStarted = true;
                break;

            case GameStage.LevelPlaying:
                break;

            case GameStage.LevelEnd:
                if (inputActive)
                {
                    IncrementLevel();
                    StartNewLevel();
                    FizzyoFramework.Instance.Session.StartSet();
                    inputActive = false;
                }

                break;

            case GameStage.GameEnd:
                if (inputActive)
                {
                    NewSession();
                    inputActive = false;
                }
                break;
        }
    }

    private bool CheckPause()
    {
        pauseTimer += Time.deltaTime;
        if (pauseTimer >= pauseTime)
        {
            pauseTimer = 0;
            return true;
        }
        return false;
    }

    // Called when a coin is touched
    public void GetCoin()
    {
        CurrentLevel.CoinCount++;
        CoinEffect.Stop();
        CoinEffect.Play();
    }

    // shows the new session ui
    public void NewSession()
    {
        currentStage = GameStage.SessionSetup;

        LevelSetupUI.SetActive(true);
        HUD.SetActive(false);
        LevelEndUI.SetActive(false);
        GameEndUI.SetActive(false);
    }

    // Creates the levels at the start of the game, after setting up number of breaths and sets/levels
    public void CreateLevels()
    {
        Levels = new List<PlatformLevel>();
        Levels.Clear();

        for (int i = 0; i < FizzyoFramework.Instance.Session.SessionSetCount; i++)
        {
            PlatformLevel newLevel = new PlatformLevel();
            newLevel.GoodBreathMax = FizzyoFramework.Instance.Session.SessionBreathCount;
            Levels.Add(newLevel);
        }

        CurrentLevelIndex = 0;
        CurrentLevel = Levels[CurrentLevelIndex];
    }

    // Increments the level
    public void IncrementLevel()
    {
        CurrentLevelIndex++;
        CurrentLevel = Levels[CurrentLevelIndex];
    }

    // Begin playing the current Level
    public void StartNewLevel()
    {
        currentStage = GameStage.LevelPlaying;
        GameStarted = true;


        if (BackgroundMenuMusic.isPlaying)
        {
            StartCoroutine(AudioFader.FadeOut(BackgroundMenuMusic, 0.5f));
        }
        backgroundMusicManager.StartBackgroundMusic();

        if (LevelStartEvent != null)
            LevelStartEvent();

        LevelSetupUI.SetActive(false);
        HUD.SetActive(true);
        LevelEndUI.SetActive(false);

        if (levelEnd != null)
        {
            Destroy(levelEnd);
        }

        Player.transform.position = Vector3.zero;
        GameEndSound.Stop();
    }

    // Shows the end of level scores and stops the game
    public void EndLevel()
    {
        if (CurrentLevelIndex == Levels.Count - 1)
        {
            TimerText = GameEndTimer.GetComponent<Text>();

            EndGame();
        }
        else
        {
            TimerText = LevelEndTimer.GetComponent<Text>();

            currentStage = GameStage.LevelEnd;

            if (LevelEndEvent != null)
                LevelEndEvent();

            LevelSetupUI.SetActive(false);
            HUD.SetActive(false);
            LevelEndUI.SetActive(true);
            GameEndUI.SetActive(false);

            LevelEndSound.Stop();
            LevelEndSound.Play();
        }
    }

    public void EndGame()
    {
        currentStage = GameStage.GameEnd;

        CheckHighScore();

        if (GameEndEvent != null)
            GameEndEvent();

        LevelSetupUI.SetActive(false);
        HUD.SetActive(false);
        LevelEndUI.SetActive(false);
        GameEndUI.SetActive(true);

        GameEndSound.Stop();
        GameEndSound.Play();

        backgroundMusicManager.StopBackgroundMusic();
    }

    public void PauseGame()
    {
        GameIsPaused = true;
        pausedStage = currentStage;
        currentStage = GameStage.Paused;

        HUD.SetActive(false);
        PauseUI.SetActive(true);

        backgroundMusicManager.StopBackgroundMusic();

    }

    public void ResumeGame()
    {
        GameIsPaused = false;
        GameIsPausedByBreath = false;
        FizzyoFramework.Instance.Session.ResumeSession();

        currentStage = pausedStage;

        HUD.SetActive(true);
        PauseUI.SetActive(false);

        backgroundMusicManager.StartBackgroundMusic();
    }

    public void CreateLevelEnd()
    {
        levelEnd = Instantiate(LevelEndPrefab);
        levelEnd.transform.position = Player.transform.position + Vector3.right * 30f;
    }
    #endregion

    public int TotalCoins()
    {
        int newCount = 0;

        foreach (PlatformLevel level in ScoreManager.Instance.Levels)
        {
            newCount += level.CoinCount;
        }

        return newCount;
    }

    public float LevelTimeTotal()
    {
        float newTimeTotal = 0f;

        foreach (PlatformLevel level in ScoreManager.Instance.Levels)
        {
            newTimeTotal += level.LevelTime;
        }

        return newTimeTotal;
    }

    public int TotalGoodBreathCount()
    {
        int newCount = 0;

        foreach (PlatformLevel level in ScoreManager.Instance.Levels)
        {
            newCount += level.GoodBreathCount;
        }

        return newCount;
    }

    public int TotalBadBreathCount()
    {
        int newCount = 0;

        foreach (PlatformLevel level in ScoreManager.Instance.Levels)
        {
            newCount += level.BadBreathCount;
        }

        return newCount;
    }

    private GameAchievements[] GetGameAchievements => new[]
    {
        new GameAchievements("Welcome to the Party","Started your first game"),
        new GameAchievements("First Rodeo", "Completed your first session"),
        new GameAchievements("Qubed", "You've been cubed/nCollected 27 coins - 3 x 3 x 3", 27),
        new GameAchievements("Qube Squared","Double that cube/nCollected 216 coins 6 x 6 x 6", 216),
        new GameAchievements("Qube Decade","Did you see that bird fly/nCollected 1000 coins 10 x 10 x 10", 1000),
        new GameAchievements("Sweet Qube","Coming of age/nCollected 4096 coins 16 x 16 x 16", 4096),
        new GameAchievements("Driving Qube","Learning to Drive?/nCollected 4913 coins 17 x 17 x 17", 4913),
        new GameAchievements("Prime Qube","Two primes make a whole?/nCollected 9261 coins (3x3x3) × (7x7x7)", 9261),
        new GameAchievements("Century Qube","Turn of the century/nCollected 1000000000 coins 100 x 100 x 100", 1000000000),
        new GameAchievements("Qubi Streak","7 day streak! Play qubi at least once a day for a week", 0, 7),
        new GameAchievements("Qubi Mega Streak","28 day streak! Play qubi at least once a day for 4 weeks", 0, 28)
    };
}

[System.Serializable]
public class PlatformLevel
{
    public int GoodBreathMax = 8;
    public int GoodBreathCount = 0;
    public int BadBreathCount = 0;

    public int CoinCount = 0;
    public float LevelTime = 0f;

    public float difficulty = 1f;

    public float MinPlayerSpeed = 8f;
    public float MaxPlayerSpeed = 16f;

    public PlatformLevel()
    {

    }
}

public struct GameAchievements
{
    public string AchievementName;
    public string AchievementTag;
    public int ScoreRequirement;
    public int DayRequirement;

    public GameAchievements(string Name, string Tag, int Score = 0, int Days = 0)
    {
        AchievementName = Name;
        AchievementTag = Tag;
        ScoreRequirement = Score;
        DayRequirement = Days;
    }
}