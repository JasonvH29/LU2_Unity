using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using YourNamespace.ApiClient;

public class CreateWorldScript : MonoBehaviour
{
    [SerializeField] private GameObject Scene2;
    [SerializeField] private GameObject Scene3;

    [SerializeField] private Button CreateEnvironment1;
    [SerializeField] private Button CreateEnvironment2;
    [SerializeField] private Button CreateEnvironment3;

    [SerializeField] private TMP_InputField environmentNameInputField1;
    [SerializeField] private TMP_InputField environmentNameInputField2;
    [SerializeField] private TMP_InputField environmentNameInputField3;

    [SerializeField] private TMP_Text Env1Text;
    [SerializeField] private TMP_Text Env2Text;
    [SerializeField] private TMP_Text Env3Text;

    [SerializeField] private Button deleteButton1;
    [SerializeField] private Button deleteButton2;
    [SerializeField] private Button deleteButton3;

    [SerializeField] private Button enterButton1;
    [SerializeField] private Button enterButton2;
    [SerializeField] private Button enterButton3;

    public Environment2DApiClient environment2DApiClient;
    public Object2DApiClient object2DApiClient;
    public UserApiClient userApiClient;

    private string environmentName1;
    private string environmentName2;
    private string environmentName3;

    private Guid environmentId1;
    private Guid environmentId2;
    private Guid environmentId3;

    private int currentEnvironmentIndex;
    private ObjectManager objectManager;

    void Start()
    {
        objectManager = FindObjectOfType<ObjectManager>();

        CreateEnvironment1.onClick.AddListener(() =>
        {
            currentEnvironmentIndex = 1;
            CreateEnvironment(environmentNameInputField1.text);
        });

        CreateEnvironment2.onClick.AddListener(() =>
        {
            currentEnvironmentIndex = 2;
            CreateEnvironment(environmentNameInputField2.text);
        });

        CreateEnvironment3.onClick.AddListener(() =>
        {
            currentEnvironmentIndex = 3;
            CreateEnvironment(environmentNameInputField3.text);
        });

        enterButton1.onClick.AddListener(() => SetActiveEnvironment(1));
        enterButton2.onClick.AddListener(() => SetActiveEnvironment(2));
        enterButton3.onClick.AddListener(() => SetActiveEnvironment(3));

        deleteButton1.onClick.AddListener(() => DeleteEnvironment(1));
        deleteButton2.onClick.AddListener(() => DeleteEnvironment(2));
        deleteButton3.onClick.AddListener(() => DeleteEnvironment(3));

        enterButton1.gameObject.SetActive(false);
        enterButton2.gameObject.SetActive(false);
        enterButton3.gameObject.SetActive(false);

        UpdateUI();
    }

    void OnEnable()
    {
        if (Scene2.activeSelf)
        {
            FetchEnvironments(); // Fetch environments when Scene2 becomes active
        }
    }

    private void UpdateUI()
    {
        if (!string.IsNullOrEmpty(environmentName1))
        {
            Env1Text.text = "Environment 1: " + environmentName1;
            Env1Text.gameObject.SetActive(true);
            environmentNameInputField1.gameObject.SetActive(false);
            deleteButton1.interactable = true;
            enterButton1.gameObject.SetActive(true);
        }
        else
        {
            Env1Text.text = "No Environment";
            Env1Text.gameObject.SetActive(true);
            environmentNameInputField1.gameObject.SetActive(true);
            deleteButton1.interactable = false;
            enterButton1.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(environmentName2))
        {
            Env2Text.text = "Environment 2: " + environmentName2;
            Env2Text.gameObject.SetActive(true);
            environmentNameInputField2.gameObject.SetActive(false);
            deleteButton2.interactable = true;
            enterButton2.gameObject.SetActive(true);
        }
        else
        {
            Env2Text.text = "No Environment";
            Env2Text.gameObject.SetActive(true);
            environmentNameInputField2.gameObject.SetActive(true);
            deleteButton2.interactable = false;
            enterButton2.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(environmentName3))
        {
            Env3Text.text = "Environment 3: " + environmentName3;
            Env3Text.gameObject.SetActive(true);
            environmentNameInputField3.gameObject.SetActive(false);
            deleteButton3.interactable = true;
            enterButton3.gameObject.SetActive(true);
        }
        else
        {
            Env3Text.text = "No Environment";
            Env3Text.gameObject.SetActive(true);
            environmentNameInputField3.gameObject.SetActive(true);
            deleteButton3.interactable = false;
            enterButton3.gameObject.SetActive(false);
        }
    }

    private bool IsUserLoggedIn()
    {
        return !string.IsNullOrEmpty(userApiClient.webClient.GetToken());
    }

    private async void CreateEnvironment(string environmentName)
    {
        if (!IsUserLoggedIn())
        {
            Debug.LogWarning("User is not logged in. Please log in first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(environmentName) || environmentName.Length > 25)
        {
            Debug.LogWarning("Environment name must be 1–25 characters.");
            return;
        }

        if (environmentName == environmentName1 || environmentName == environmentName2 || environmentName == environmentName3)
        {
            Debug.LogWarning("Environment name must be unique.");
            return;
        }

        Environment2D environment2D = new Environment2D { name = environmentName };

        try
        {
            IWebRequestReponse webRequestResponse = await environment2DApiClient.CreateEnvironment(environment2D);

            switch (webRequestResponse)
            {
                case WebRequestData<Environment2D> dataResponse:
                    Environment2D created = dataResponse.Data;
                    Debug.Log("Environment creation success!");

                    switch (currentEnvironmentIndex)
                    {
                        case 1:
                            environmentName1 = created.name;
                            environmentId1 = Guid.Parse(created.id);
                            break;
                        case 2:
                            environmentName2 = created.name;
                            environmentId2 = Guid.Parse(created.id);
                            break;
                        case 3:
                            environmentName3 = created.name;
                            environmentId3 = Guid.Parse(created.id);
                            break;
                    }

                    PlayerPrefs.SetString("EnvironmentName" + currentEnvironmentIndex, created.name);
                    PlayerPrefs.SetString("EnvironmentId" + currentEnvironmentIndex, created.id);

                    UpdateUI();
                    BackToScene2();
                    objectManager?.SetActiveEnvironmentId(created.id);
                    break;

                case WebRequestError errorResponse:
                    Debug.Log("Environment creation error: " + errorResponse.ErrorMessage);
                    break;

                default:
                    throw new NotImplementedException("Unhandled response type: " + webRequestResponse.GetType());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Environment creation exception: " + ex.Message);
        }
    }

    public async void FetchEnvironments()
    {
        if (!IsUserLoggedIn())
        {
            Debug.LogWarning("User is not logged in. Please log in first.");
            return;
        }

        Debug.Log("Fetching environments...");

        IWebRequestReponse webRequestResponse = await environment2DApiClient.ReadEnvironment2Ds();

        switch (webRequestResponse)
        {
            case WebRequestData<List<Environment2D>> dataResponse:
                List<Environment2D> environments = dataResponse.Data;
                DisplayEnvironments(environments);
                break;
            case WebRequestError errorResponse:
                Debug.LogError("Read environments error: " + errorResponse.ErrorMessage);
                break;
            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    private void DisplayEnvironments(List<Environment2D> environments)
    {
        environmentName1 = null;
        environmentName2 = null;
        environmentName3 = null;
        environmentId1 = Guid.Empty;
        environmentId2 = Guid.Empty;
        environmentId3 = Guid.Empty;

        for (int i = 0; i < environments.Count; i++)
        {
            switch (i)
            {
                case 0:
                    environmentName1 = environments[i].name;
                    environmentId1 = Guid.Parse(environments[i].id);
                    break;
                case 1:
                    environmentName2 = environments[i].name;
                    environmentId2 = Guid.Parse(environments[i].id);
                    break;
                case 2:
                    environmentName3 = environments[i].name;
                    environmentId3 = Guid.Parse(environments[i].id);
                    break;
            }
        }

        UpdateUI();
    }

    private async void DeleteEnvironment(int index)
    {
        if (!IsUserLoggedIn())
        {
            Debug.LogWarning("User is not logged in. Please log in first.");
            return;
        }

        Guid environmentId = Guid.Empty;
        switch (index)
        {
            case 1:
                environmentId = environmentId1;
                environmentName1 = null;
                environmentId1 = Guid.Empty;
                break;
            case 2:
                environmentId = environmentId2;
                environmentName2 = null;
                environmentId2 = Guid.Empty;
                break;
            case 3:
                environmentId = environmentId3;
                environmentName3 = null;
                environmentId3 = Guid.Empty;
                break;
        }

        if (environmentId != Guid.Empty)
        {
            try
            {
                Debug.Log($"Attempting to delete environment with ID: {environmentId}");

                IWebRequestReponse objectResponse = await object2DApiClient.ReadObject2Ds(environmentId.ToString());
                if (objectResponse is WebRequestData<List<Object2D>> object2DListResponse)
                {
                    List<Object2D> object2DList = object2DListResponse.Data;
                    foreach (var object2D in object2DList)
                    {
                        Debug.Log($"Attempting to delete Object2D with ID: {object2D.id}");
                        IWebRequestReponse deleteObjectResponse = await object2DApiClient.DeleteObject2D(object2D.id.ToString());
                        if (deleteObjectResponse is WebRequestError deleteObjectError)
                        {
                            Debug.LogError($"Failed to delete Object2D with ID: {object2D.id}. Error: {deleteObjectError.ErrorMessage}");
                        }
                    }
                }
                else if (objectResponse is WebRequestError objectError)
                {
                    Debug.LogError($"Failed to fetch Object2Ds for environment ID: {environmentId}. Error: {objectError.ErrorMessage}");
                }

                IWebRequestReponse webRequestResponse = await environment2DApiClient.DeleteEnvironment(environmentId.ToString());

                switch (webRequestResponse)
                {
                    case WebRequestData<string> dataResponse:
                        Debug.Log("Environment deletion success!");
                        PlayerPrefs.DeleteKey("EnvironmentName" + index);
                        PlayerPrefs.DeleteKey("EnvironmentId" + index);
                        UpdateUI();
                        break;
                    case WebRequestError errorResponse:
                        string errorMessage = errorResponse.ErrorMessage;
                        Debug.LogError($"Environment deletion error: {errorMessage}");
                        break;
                    default:
                        throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Environment deletion exception: " + ex.Message);
            }
        }
    }

    public void SetActiveEnvironment(int index)
    {
        Debug.Log($"Environment {index} is now active.");

        switch (index)
        {
            case 1:
                objectManager?.SetActiveEnvironmentId(environmentId1.ToString());
                break;
            case 2:
                objectManager?.SetActiveEnvironmentId(environmentId2.ToString());
                break;
            case 3:
                objectManager?.SetActiveEnvironmentId(environmentId3.ToString());
                break;
        }

        objectManager?.LoadObjects();

        Scene3.SetActive(true);
        Scene2.SetActive(false);
    }

    public void GoToScene3() { Scene2.SetActive(false); Scene3.SetActive(true); }

    public void BackToScene2()
    {
        Scene3.SetActive(false); Scene2.SetActive(true);

        environmentNameInputField1.text = "";
        environmentNameInputField2.text = "";
        environmentNameInputField3.text = "";
    }
}



