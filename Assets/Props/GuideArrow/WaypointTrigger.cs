using UnityEngine;
using TMPro;

public class WaypointTrigger : MonoBehaviour
{
    // Assign the next destination in the Inspector.
    public Transform nextTarget;
    
    // Instruction/message to display when this waypoint is triggered.
    public string instructionMessage;
    
    // Reference to the UI TextMeshPro element that will display the message.
    public TextMeshProUGUI instructionText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NavigationGuide guide = NavigationGuide.CurrentGuide;
            if (guide != null)
            {
                // Set the new navigation target.
                guide.SetTarget(nextTarget);
                
                // Update the UI text with the instruction message.
                if (instructionText != null)
                {
                    instructionText.text = instructionMessage;
                }
                
                // Destroy this trigger so it cannot be triggered again.
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("No active NavigationGuide found on " + gameObject.name);
            }
        }
    }
}






