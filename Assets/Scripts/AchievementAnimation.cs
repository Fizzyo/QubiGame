using UnityEngine;
using UnityEngine.UI;

public class AchievementAnimation : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator anim;
    public Text achievementName;
    public Text achievementDescription;
    public ParticleSystem achievementParticles;
    public AudioSource achievementSound;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void UnlockAchievmentUI(string Name, string Description)
    {
        achievementName.text = Name;
        achievementDescription.text = Description;
        anim.SetTrigger("Start");
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UnlockAchievmentUI("This is a test", "TestingTesting\n1 + 2 = 3");
        }
    }
#endif

    public void ActivateParticles()
    {
        achievementParticles.Play();
    }

    public void PlayAchievementSound()
    {
        achievementSound.Stop();
        achievementSound.Play();
    }

}