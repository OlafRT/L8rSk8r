using UnityEngine;

public class EnableOnTrigger : MonoBehaviour
{
    [Tooltip("Array of GameObjects to enable when the player enters the trigger.")]
    public GameObject[] objectsToEnable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}

