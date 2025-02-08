using UnityEngine;

public class ComponentSwitcher : MonoBehaviour
{
    [Header("Third-Person Mode Components (to be enabled in Third-Person mode)")]
    public Component[] thirdPersonComponents;
    
    [Header("Skateboard Mode Components (to be enabled in Skateboard mode)")]
    public Component[] skateboardComponents;

    /// <summary>
    /// Enables all third-person components and disables all skateboard components.
    /// </summary>
    public void SwitchToThirdPerson()
    {
        foreach (Component comp in thirdPersonComponents)
        {
            SetComponentEnabled(comp, true);
        }
        foreach (Component comp in skateboardComponents)
        {
            SetComponentEnabled(comp, false);
        }
    }

    /// <summary>
    /// Enables all skateboard components and disables all third-person components.
    /// </summary>
    public void SwitchToSkateboard()
    {
        foreach (Component comp in thirdPersonComponents)
        {
            SetComponentEnabled(comp, false);
        }
        foreach (Component comp in skateboardComponents)
        {
            SetComponentEnabled(comp, true);
        }
    }

    /// <summary>
    /// Helper function to toggle the enabled property on supported components.
    /// </summary>
    private void SetComponentEnabled(Component comp, bool enabled)
    {
        if (comp == null)
            return;

        // If it is a Behaviour (e.g., MonoBehaviour, CharacterController, etc.)
        if (comp is Behaviour)
        {
            ((Behaviour)comp).enabled = enabled;
        }
        // If it's a Renderer (like MeshRenderer, SpriteRenderer, etc.)
        else if (comp is Renderer)
        {
            ((Renderer)comp).enabled = enabled;
        }
        // If it's a Collider (BoxCollider, SphereCollider, etc.)
        else if (comp is Collider)
        {
            ((Collider)comp).enabled = enabled;
        }
        else
        {
            Debug.LogWarning("Component " + comp.GetType().Name + " does not support enabling/disabling via this script.");
        }
    }
}

