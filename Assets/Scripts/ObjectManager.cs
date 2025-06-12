using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourNamespace.ApiClient;

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
    private string activeEnvironmentId; // Active environment ID
    public Object2DApiClient object2DApiClient;

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

        // Ensure object2DApiClient is assigned
        if (object2DApiClient == null)
        {
            object2DApiClient = FindObjectOfType<Object2DApiClient>();
            if (object2DApiClient == null)
            {
                Debug.LogError("Object2DApiClient not found in the scene.");
            }
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
                isDragging.activeEnvironmentId = activeEnvironmentId.ToString(); // Set the environment ID
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
        //if (Guid.TryParse(environmentId, out Guid parsedEnvironmentId))
        //{
            activeEnvironmentId = environmentId;
            Debug.Log($"Active environment ID set to: {activeEnvironmentId}");
        //}
        //else
        //{
        //    Debug.LogError("Invalid environment ID format.");
        //}
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane; // Set this to the distance from the camera to the object
        Vector3 positionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
        positionInWorld.z = 0; // Ensure the z position is set to 0 for 2D
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
        // Save the positions and states of placedObjects to PlayerPrefs
        PlayerPrefs.SetInt($"{currentSaveFile}_ObjectCount", placedObjects.Count);
        for (int i = 0; i < placedObjects.Count; i++)
        {
            GameObject obj = placedObjects[i];
            PlayerPrefs.SetString($"{currentSaveFile}_Object_{i}_PrefabId", obj.name);
            PlayerPrefs.SetFloat($"{currentSaveFile}_Object_{i}_PosX", obj.transform.position.x);
            PlayerPrefs.SetFloat($"{currentSaveFile}_Object_{i}_PosY", obj.transform.position.y);
        }
        PlayerPrefs.Save();
        Debug.Log($"Saved {placedObjects.Count} objects to {currentSaveFile}");
    }

    private void LoadSavedState()
    {
        // Load the positions and states of placedObjects from PlayerPrefs
        int objectCount = PlayerPrefs.GetInt($"{currentSaveFile}_ObjectCount", 0);
        for (int i = 0; i < objectCount; i++)
        {
            string prefabId = PlayerPrefs.GetString($"{currentSaveFile}_Object_{i}_PrefabId");
            float posX = PlayerPrefs.GetFloat($"{currentSaveFile}_Object_{i}_PosX");
            float posY = PlayerPrefs.GetFloat($"{currentSaveFile}_Object_{i}_PosY");

            GameObject prefab = prefabObjects.Find(p => p.name == prefabId);
            if (prefab != null)
            {
                Vector3 position = new Vector3(posX, posY, 0);
                GameObject instance = Instantiate(prefab, position, Quaternion.identity);
                placedObjects.Add(instance);
            }
            else
            {
                Debug.LogError($"Prefab with name {prefabId} not found.");
            }
        }
        Debug.Log($"Loaded {objectCount} objects from {currentSaveFile}");
    }

    public async void SaveObjects()
    {
        foreach (var obj in placedObjects)
        {
            IsDragging isDragging = obj.GetComponent<IsDragging>();
            if (isDragging != null)
            {
                Object2D object2D = new Object2D
                {
                    prefabId = obj.name.Replace("(Clone)", "").Trim(), // Ensure prefabId is set correctly
                    positionX = Mathf.RoundToInt(obj.transform.position.x), // Ensure int
                    positionY = Mathf.RoundToInt(obj.transform.position.y), // Ensure int
                    environmentId = activeEnvironmentId
                };
                Debug.Log($"Saving Object2D with data: {JsonUtility.ToJson(object2D)}");
                await object2DApiClient.CreateObject2D(object2D);
            }
        }
    }

    public async void LoadObjects()
    {
        DestroyAllPlacedObjects();
        if (object2DApiClient == null)
        {
            Debug.LogError("object2DApiClient is null.");
            return;
        }

        IWebRequestReponse response = await object2DApiClient.ReadObject2Ds(activeEnvironmentId.ToString());
        if (response is WebRequestData<List<Object2D>> object2DsData)
        {
            List<Object2D> object2Ds = object2DsData.Data;
            foreach (var object2D in object2Ds)
            {
                string prefabName = object2D.prefabId.Replace("(Clone)", "").Trim();
                GameObject prefab = prefabObjects.Find(p => p.name == prefabName);
                if (prefab != null)
                {
                    Vector3 position = new Vector3(object2D.positionX, object2D.positionY, 0);
                    GameObject instance = Instantiate(prefab, position, Quaternion.identity);
                    placedObjects.Add(instance);
                }
                else
                {
                    Debug.LogError($"Prefab with name {prefabName} not found.");
                }
            }
        }
        else if (response is WebRequestError error)
        {
            Debug.LogError($"Failed to load objects. Error: {error.ErrorMessage}");
        }
        else
        {
            Debug.LogError("Failed to load objects. Unknown error.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoadObjects();
        }
    }
}



