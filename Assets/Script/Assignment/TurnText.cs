using UnityEngine;

public class TurnText : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the object look at the camera
            transform.LookAt(transform.position + mainCamera.transform.forward);

        } 
    }
}
