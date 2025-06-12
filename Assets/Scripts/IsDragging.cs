using System;
using UnityEngine;

public class IsDragging : MonoBehaviour
{
    public ObjectManager objectManager;
    public string activeEnvironmentId;
    public bool isDragging = false;

    private void Update()
    {
        if (isDragging)
        {
            this.transform.position = GetMousePosition();
        }
    }

    private async void OnMouseUpAsButton()
    {
        if (objectManager != null)
        {
            isDragging = !isDragging;

            if (!isDragging)
            {
                objectManager.ShowMenu();

                // Save the object to the database when it is placed
                Object2D object2D = new Object2D
                {
                    prefabId = gameObject.name, // Ensure prefabId is set correctly
                    positionX = Mathf.RoundToInt(transform.position.x), // Ensure int
                    positionY = Mathf.RoundToInt(transform.position.y), // Ensure int
                    environmentId = activeEnvironmentId
                };
                Debug.Log($"Creating Object2D with data: {JsonUtility.ToJson(object2D)}");
                await objectManager.object2DApiClient.CreateObject2D(object2D);
            }
        }
        else
        {
            Debug.LogError("ObjectManager reference not set.");
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane; // Set this to the distance from the camera to the object
        Vector3 positionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        positionInWorld.z = 0; // Ensure the z position is set to 0 for 2D
        return positionInWorld;
    }
}
