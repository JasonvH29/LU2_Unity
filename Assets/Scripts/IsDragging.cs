using System;
using UnityEngine;

public class IsDragging : MonoBehaviour
{
    public ObjectManager objectManager;
    public string environmentId;
    public bool isDragging = false;

    private void Update()
    {
        if (isDragging)
        {
            this.transform.position = GetMousePosition();
        }
    }

    private void OnMouseUpAsButton()
    {
        if (objectManager != null)
        {
            isDragging = !isDragging;

            if (!isDragging)
            {
                objectManager.ShowMenu();
            }
        }
        else
        {
            Debug.LogError("ObjectManager reference not set.");
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector3 positionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        positionInWorld.z = 0;
        return positionInWorld;
    }
}
