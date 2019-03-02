using UnityEngine;
using UnityEngine.UI;

public class AchievementAnimation : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator anim;
    public Text achievementName;
    public Text achievementDescription;

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
}