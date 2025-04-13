using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace YourNamespace.ApiClient
{
    public class Object2DApiClient : MonoBehaviour
    {
        public WebClient webClient;

        public async Awaitable<IWebRequestReponse> ReadObject2Ds(string environmentId)
        {
            string route = $"/api/object2d/environment/{environmentId}"; // Use environmentId in the route
            Debug.Log($"Sending GET request to: {route}");
            IWebRequestReponse webRequestResponse = await webClient.SendGetRequest(route);
            Debug.Log($"Received response: {webRequestResponse}");
            return ParseObject2DListResponse(webRequestResponse);
        }

        public async Awaitable<IWebRequestReponse> CreateObject2D(Object2D object2D)
        {
            string route = $"/api/object2d"; // Use the base route for creating a new object2D
            string data = JsonUtility.ToJson(object2D);
            Debug.Log($"Sending POST request to: {route} with data: {data}");
            try
            {
                IWebRequestReponse webRequestResponse = await webClient.SendPostRequest(route, data);
                Debug.Log($"Received response: {webRequestResponse}");
                return ParseObject2DResponse(webRequestResponse);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while sending POST request: {ex.Message}");
                throw;
            }
        }


        public async Awaitable<IWebRequestReponse> DeleteObject2D(string object2DId)
        {
            string route = $"/api/object2d/{object2DId}"; // Use the base route for deleting an object2D
            Debug.Log($"Sending DELETE request to: {route}");
            return await webClient.SendDeleteRequest(route);
        }

        public async Awaitable<IWebRequestReponse> UpdateObject2D(string object2DId, Object2D object2D)
        {
            string route = $"/api/object2d/{object2DId}"; // Use the base route for updating an object2D
            string data = JsonUtility.ToJson(object2D);
            Debug.Log($"Sending PUT request to: {route} with data: {data}");
            IWebRequestReponse webRequestResponse = await webClient.SendPutRequest(route, data); // Use SendPutRequest instead of SendPostRequest
            Debug.Log($"Received response: {webRequestResponse}");
            return ParseObject2DResponse(webRequestResponse);
        }

        private IWebRequestReponse ParseObject2DResponse(IWebRequestReponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    Debug.Log("Response data raw: " + data.Data);
                    Object2D object2D = JsonUtility.FromJson<Object2D>(data.Data);
                    WebRequestData<Object2D> parsedWebRequestData = new WebRequestData<Object2D>(object2D);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }

        private IWebRequestReponse ParseObject2DListResponse(IWebRequestReponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    Debug.Log("Response data raw: " + data.Data);
                    List<Object2D> object2Ds = JsonHelper.ParseJsonArray<Object2D>(data.Data);
                    WebRequestData<List<Object2D>> parsedWebRequestData = new WebRequestData<List<Object2D>>(object2Ds);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }

        public async 
        Task
GetAllObject2DInfo(string environmentId)
        {
            Debug.LogError("Object2Ds are being loaded");
            IWebRequestReponse response = await ReadObject2Ds(environmentId);
            if (response is WebRequestData<List<Object2D>> object2DsData)
            {
                List<Object2D> object2Ds = object2DsData.Data;
                foreach (var object2D in object2Ds)
                {
                    Debug.Log($"ID: {object2D.id}, PrefabId: {object2D.prefabId}, PositionX: {object2D.positionX}, PositionY: {object2D.positionY},  EnvironmentId: {object2D.environmentId}");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve object2Ds.");
            }
        }
    }
}