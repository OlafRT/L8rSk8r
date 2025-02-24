using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDeathTransition : MonoBehaviour
{
    // Assign the boss (the BossController component) in the Inspector.
    public BossController boss;

    private bool transitionStarted = false;

    void Update()
    {
        // Check if the boss is dead and we haven't started the transition yet.
        if (boss != null && boss.IsDead && !transitionStarted)
        {
            transitionStarted = true;
            StartCoroutine(TransitionAfterDelay());
        }
    }

    IEnumerator TransitionAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Credits");
    }
}
