using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public GameObject UISideMenu;
    public List<GameObject> prefabObjects;
    private List<GameObject> placedObjects = new List<GameObject>();

    [SerializeField] private Button resetButton; // Reference to the reset button
    [SerializeField] private Button backButton;  // Reference to the back button
    [SerializeField] private GameObject currentScene; // Reference to the current scene GameObject
    [SerializeField] private GameObject scene2; // Reference to the Scene2 GameObject

    private string currentSaveFile = "defaultSave"; // Default save file
    internal object object2DApiClient;

    private void Start()
    {
        ShowMenu();

        // Set up the reset button's OnClick event
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(Reset);
        }
        else
        {
            Debug.LogError("Reset button reference not set.");
        }

        // Set up the back button's OnClick event
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToScene2);
        }
        else
        {
            Debug.LogError("Back button reference not set.");
        }
    }

    public void PlaceNewObject2D(int index)
    {
        if (index >= 0 && index < prefabObjects.Count)
        {
            UISideMenu.SetActive(false);
            Vector3 mousePosition = GetMousePosition();
            GameObject instanceOfPrefab = Instantiate(prefabObjects[index], mousePosition, Quaternion.identity);

            IsDragging isDragging = instanceOfPrefab.GetComponent<IsDragging>();
            if (isDragging != null)
            {
                isDragging.objectManager = this;
                isDragging.isDragging = true;
                placedObjects.Add(instanceOfPrefab);
                Debug.Log("Object placed and dragging started.");
            }
            else
            {
                Debug.LogError("IsDragging component not found on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogError("Invalid index for prefabObjects.");
        }
    }

    public void ShowMenu()
    {
        if (UISideMenu != null)
        {
            UISideMenu.SetActive(true);
        }
        else
        {
            Debug.LogError("UISideMenu reference not set.");
        }
    }

    public void Reset()
    {
        DestroyAllPlacedObjects(); // Remove all placed animals
    }

    internal void DestroyAllPlacedObjects()
    {
        foreach (var obj in placedObjects)
        {
            Destroy(obj);
        }
        placedObjects.Clear();
    }

    internal void SetActiveEnvironmentId(string environmentId)
    {
        Debug.Log($"Active environment ID set to: {environmentId}");
    }

    private Vector3 GetMousePosition()
    {
        Vector3 positionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        positionInWorld.z = 0;
        return positionInWorld;
    }

    private void GoBackToScene2()
    {
        SaveCurrentState(); // Save the current state before switching scenes

        if (currentScene != null)
        {
            currentScene.SetActive(false); // Deactivate the current scene
        }
        else
        {
            Debug.LogError("Current scene reference not set.");
        }

        if (scene2 != null)
        {
            scene2.SetActive(true); // Activate Scene2
        }
        else
        {
            Debug.LogError("Scene2 reference not set.");
        }
    }

    public void CreateWorld(int worldIndex)
    {
        currentSaveFile = $"saveFile{worldIndex}"; // Set the save file based on the selected world
        LoadSavedState(); // Load the saved state for the selected world
    }

    private void SaveCurrentState()
    {
        // Implement your save logic here, e.g., save the positions and states of placedObjects to PlayerPrefs or a file
        Debug.Log($"Saving current state to {currentSaveFile}");
    }

    private void LoadSavedState()
    {
        // Implement your load logic here, e.g., load the positions and states of placedObjects from PlayerPrefs or a file
        Debug.Log($"Loading saved state from {currentSaveFile}");
    }
}
