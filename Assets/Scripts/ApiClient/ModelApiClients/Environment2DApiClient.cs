using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Environment2DApiClient : MonoBehaviour
{
    public WebClient webClient;

    public async Task<IWebRequestReponse> ReadEnvironment2Ds()
    {
        string route = "/environment2Ds";

        IWebRequestReponse webRequestResponse = await webClient.SendGetRequest(route);
        return ParseEnvironment2DListResponse(webRequestResponse);
    }

    public async Task<IWebRequestReponse> CreateEnvironment(Environment2D environment)
    {
        string route = "/environment2Ds";
        string data = JsonUtility.ToJson(environment);

        IWebRequestReponse webRequestResponse = await webClient.SendPostRequest(route, data);
        return ParseEnvironment2DResponse(webRequestResponse);
    }

    public async Task<IWebRequestReponse> DeleteEnvironment(string environmentId)
    {
        string route = "/environment2Ds/" + environmentId;
        return await webClient.SendDeleteRequest(route);
    }

    private IWebRequestReponse ParseEnvironment2DResponse(IWebRequestReponse webRequestResponse)
    {
        switch (webRequestResponse)
        {
            case WebRequestData<string> data:
                Debug.Log("Response data raw: " + data.Data);
                Environment2D environment = JsonUtility.FromJson<Environment2D>(data.Data);
                WebRequestData<Environment2D> parsedWebRequestData = new WebRequestData<Environment2D>(environment);
                return parsedWebRequestData;
            default:
                return webRequestResponse;
        }
    }

    private IWebRequestReponse ParseEnvironment2DListResponse(IWebRequestReponse webRequestResponse)
    {
        switch (webRequestResponse)
        {
            case WebRequestData<string> data:
                Debug.Log("Response data raw: " + data.Data);
                List<Environment2D> environment2Ds = JsonHelper.ParseJsonArray<Environment2D>(data.Data);
                WebRequestData<List<Environment2D>> parsedWebRequestData = new WebRequestData<List<Environment2D>>(environment2Ds);
                return parsedWebRequestData;
            default:
                return webRequestResponse;
        }
    }

    public async void GetAllEnvironmentInfo()
    {
        Debug.LogError("Environments are being loaded");
        IWebRequestReponse response = await ReadEnvironment2Ds();
        if (response is WebRequestData<List<Environment2D>> environment2DsData)
        {
            List<Environment2D> environment2Ds = environment2DsData.Data;
            foreach (var environment2D in environment2Ds)
            {
                Debug.Log($"ID: {environment2D.id}, Name: {environment2D.name}");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve environments.");
        }
    }
}
