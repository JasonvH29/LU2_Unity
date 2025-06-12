using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

public class AccountManagerScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button togglePasswordButton;
    [SerializeField] private TMP_Text errorMessage; // UI Text for error messages

    public GameObject Scene1;
    public GameObject Scene2;
    public GameObject Scene3;

    public UserApiClient userApiClient;
    public CreateWorldScript createWorldScript; // Reference to CreateWorldScript

    private bool isPasswordVisible = false;

    void Start()
    {
        createAccountButton.onClick.AddListener(async () => await CreateAccount());
        loginButton.onClick.AddListener(async () => await Login());
        togglePasswordButton.onClick.AddListener(TogglePasswordVisibility);

        errorMessage.text = ""; // Clear error message on start
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();

        // Make sure Scene1 starts first
        Scene1.SetActive(true);
        Scene2.SetActive(false);
        Scene3.SetActive(false); // Ensure Scene3 is hidden at the start
    }

    async Task CreateAccount()
    {
        string email = userNameInput.text;
        string password = passwordInput.text;

        // Validate username and password
        if (!ValidateUsername(email))
        {
            DisplayErrorMessage("Username is already taken or invalid.");
            return;
        }

        if (!ValidatePassword(password))
        {
            DisplayErrorMessage("Password must be at least 10 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one non-alphanumeric character.");
            return;
        }

        User user = new User
        {
            email = email,
            password = password
        };

        try
        {
            IWebRequestReponse webRequestResponse = await userApiClient.Register(user);

            switch (webRequestResponse)
            {
                case WebRequestData<string> dataResponse:
                    Debug.Log("Register success! Response: " + dataResponse.Data);
                    string token = dataResponse.Data;
                    PlayerPrefs.SetString("authToken", token); // Save the token
                    PlayerPrefs.Save();
                    break;
                case WebRequestError errorResponse:
                    string error = errorResponse.ErrorMessage;
                    Debug.Log("Register error: " + error);
                    DisplayErrorMessage(MapErrorMessage(error));
                    break;
                default:
                    throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Register exception: " + ex.Message);
            DisplayErrorMessage("An error occurred during registration.");
        }
    }

    public async Task Login()
    {
        string email = userNameInput.text;
        string password = passwordInput.text;

        User user = new User
        {
            email = email,
            password = password
        };

        try
        {
            IWebRequestReponse webRequestResponse = await userApiClient.Login(user);

            switch (webRequestResponse)
            {
                case WebRequestData<string> dataResponse:
                    Debug.Log("Login success! Response: " + dataResponse.Data);
                    string token = dataResponse.Data;
                    PlayerPrefs.SetString("authToken", token); // Save the token
                    PlayerPrefs.Save();
                    SwitchToScene2(); // Switch to Scene2 on successful login

                    if (createWorldScript != null)
                    {
                        createWorldScript.FetchEnvironments(); // Fetch environments after switching to Scene2
                    }
                    else
                    {
                        Debug.LogError("createWorldScript reference is not set.");
                    }
                    break;
                case WebRequestError errorResponse:
                    string error = errorResponse.ErrorMessage;
                    Debug.Log("Login error: " + error);
                    DisplayErrorMessage(MapErrorMessage(error));
                    break;
                default:
                    throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Login exception: " + ex.Message);
            DisplayErrorMessage("An error occurred during login.");
        }
    }

    void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;
        passwordInput.contentType = isPasswordVisible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();
    }

    void DisplayErrorMessage(string message)
    {
        errorMessage.text = message;
        Debug.Log(message);
    }

    void ClearErrorMessage()
    {
        errorMessage.text = "";
    }

    void SwitchToScene2()
    {
        Scene1.SetActive(false);
        Scene2.SetActive(true);
        Scene3.SetActive(false); // Ensure Scene3 is hidden when switching to Scene2
    }

    bool ValidateUsername(string username)
    {
        // Check if the username is unique (this should be done on the server-side, but for simplicity, we assume it's unique here)
        // Add your server-side check here if needed
        return !string.IsNullOrEmpty(username);
    }

    bool ValidatePassword(string password)
    {
        if (password.Length < 10)
        {
            return false;
        }

        bool hasLowerChar = false;
        bool hasUpperChar = false;
        bool hasDigit = false;
        bool hasNonAlphaNum = false;

        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLowerChar = true;
            if (char.IsUpper(c)) hasUpperChar = true;
            if (char.IsDigit(c)) hasDigit = true;
            if (!char.IsLetterOrDigit(c)) hasNonAlphaNum = true;
        }

        return hasLowerChar && hasUpperChar && hasDigit && hasNonAlphaNum;
    }

    string MapErrorMessage(string error)
    {
        if (error.Contains("401"))
        {
            return "Password or e-mail incorrect. Please try again.";
        }
        // Add more error mappings as needed
        return "An error occurred. Please try again.";
    }
}

