using UnityEngine;
using UnityEngine.UI;

public class ModelPreviewer : MonoBehaviour
{
    [Header("References")]
    public RawImage previewImage;
    public Camera previewCamera;
    public Transform previewParent;

    private GameObject currentPreviewModel;

    /// <summary>
    /// Instantiates and displays a preview of the given model prefab,
    /// applying the previewScaleFactor to adjust its size.
    /// </summary>
    public void SetupPreview(GameObject modelPrefab, float previewScaleFactor)
    {
        if (modelPrefab == null)
        {
            Debug.LogError("ModelPreviewer: Provided modelPrefab is null!");
            return;
        }
        Debug.Log("ModelPreviewer: Instantiating preview model for " + modelPrefab.name);

        // Remove any previous preview model.
        if (currentPreviewModel != null)
        {
            Destroy(currentPreviewModel);
        }

        // Instantiate the new model as a child of the preview parent.
        currentPreviewModel = Instantiate(modelPrefab, previewParent);
        if (currentPreviewModel == null)
        {
            Debug.LogError("ModelPreviewer: Instantiation failed!");
            return;
        }
        Debug.Log("ModelPreviewer: Instantiated model as child of " + previewParent.name);

        // Adjust its transform so it's visible in the preview camera.
        currentPreviewModel.transform.localPosition = Vector3.zero;
        currentPreviewModel.transform.localRotation = Quaternion.Euler(0, 180, 0);
        // Apply the scale: original prefab scale multiplied by the previewScaleFactor.
        currentPreviewModel.transform.localScale = modelPrefab.transform.localScale * previewScaleFactor;

        // Setup the RenderTexture for the preview camera.
        RenderTexture rt = new RenderTexture(256, 256, 16);
        previewCamera.targetTexture = rt;
        previewImage.texture = rt;
    }
}




