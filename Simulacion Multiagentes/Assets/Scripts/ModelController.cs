using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
[Serializable]
public class CarJSON
{
    public string id;
    public int x;
    public int y;
    public int z;
}

[Serializable]

public class CarData {

    public List<CarJSON> cars;

}



public class TrafficLightData
{
    public List<Vector3> positions;
    public bool green;
}

public class DBInit
{
    public string filename;
    public int maxFrames;
}

public class Request  
{  
    public string error;
    public UnityEngine.Networking.UnityWebRequest.Result result;
    public string response; 
    public long responseCode;
}  

public class ModelController : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string sendConfigEndpoint = "/init";
    string getCarsEndpoint = "/cars";
    string getTrafficLightEndpoint = "/trafficLights";
    string updateEndpoint = "/step";
    TrafficLightData trafficLightData;
    CarData carData;
    GameObject[] cars;
    GameObject[] trafficLights;
    //GameObject[] walls;
    Dictionary<string, Vector3> oldCarPositions;
    Dictionary<string, Vector3> newCarPositions;
    Dictionary<string, GameObject> carObjects;
    List<Vector3> oldTrafficLightPositions;
    List<Vector3> newTrafficLightPositions;
    // Pause the simulation while we get the update from the server
    bool hold = false;

    public GameObject carPrefab, trafficLightPrefab;
    public int numberOfCars = 20;
    public float timeToUpdate = 5.0f, timer, dt;
    
    // Start is called before the first frame update
    private void Start()
    {
        //Initialize variables
        carData = new CarData();
        trafficLightData = new TrafficLightData();
        oldCarPositions = new Dictionary<string, Vector3>();
        newCarPositions = new Dictionary<string, Vector3>();
        oldTrafficLightPositions = new List<Vector3>();
        newTrafficLightPositions = new List<Vector3>();

        cars = new GameObject[numberOfCars];
        trafficLights = new GameObject[5];

        timer = timeToUpdate;

        // for(int i = 0; i < numberOfCars; i++) 
        //     cars[i] = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        // for(int i = 0; i < 5; i++)
        //     trafficLights[i] = Instantiate(trafficLightPrefab, Vector3.zero, Quaternion.identity);

        StartCoroutine(SendConfiguration());

    }

    // Update is called once per frame
    private void Update() 
    {
        float t = timer/timeToUpdate;
        // Smooth out the transition at start and end
        dt = t * t * ( 3f - 2f*t);

        if(timer >= timeToUpdate)
        {
            timer = 0;
            hold = true;
            StartCoroutine(UpdateSimulation());
        }

        if (hold) return;
        
        foreach (KeyValuePair<string, Vector3> newCarPosition in newCarPositions)
        {
            Vector3 interpolated = Vector3.Lerp(oldCarPositions[newCarPosition.Key], newCarPositions[newCarPosition.Key], dt);
            carObjects[newCarPosition.Key].transform.localPosition = interpolated;
                
            Vector3 dir = oldCarPositions[newCarPosition.Key] - newCarPositions[newCarPosition.Key];
            carObjects[newCarPosition.Key].transform.rotation = Quaternion.LookRotation(dir);
        }
        /*for (int s = 0; s < trafficLights.Length; s++)
        {
            Vector3 interpolated = Vector3.Lerp(oldTrafficLightPositions[s], newTrafficLightPositions[s], dt);
            trafficLights[s].transform.localPosition = interpolated;
            Vector3 dir = oldTrafficLightPositions[s] - newTrafficLightPositions[s];
            trafficLights[s].transform.rotation = Quaternion.LookRotation(dir);
                
        }*/

        // Move time from the last frame
        timer += Time.deltaTime;
    }

    private IEnumerator SendConfiguration()
    {
        DBInit dbInit = new DBInit();
        dbInit.filename = "base.txt";
        dbInit.maxFrames = 1000;
        string json = JsonUtility.ToJson(dbInit);
        using (UnityWebRequest www = UnityWebRequest.Put(serverUrl + sendConfigEndpoint,json))
        {
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            // Request and wait for the desired page.
            yield return www.SendWebRequest();
            Request req = new Request();
            req.result = www.result;
            req.error = www.error;
            req.response =  www.downloadHandler.text;
            req.responseCode= www.responseCode;
            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Configuration sent");
                StartCoroutine(GetCars());
                StartCoroutine(GetTrafficLights());
            }
            else
            {
                Debug.Log("Error sending configuration");
            }
        }
    }

    private IEnumerator GetCars()
    {   
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCarsEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            carData = JsonUtility.FromJson<CarData>(www.downloadHandler.text);

            Debug.Log(carData.cars.Count);

            // Store the old positions for each agentF
            oldCarPositions = new Dictionary<string, Vector3>(newCarPositions);

            newCarPositions.Clear();
            

            foreach (var data in carData.cars)
            {
                if (carObjects.ContainsKey(data.id))
                {
                    newCarPositions[data.id] = new Vector3(data.x,data.y,data.z);
                }
                else
                {
                    GameObject car = Instantiate(carPrefab,new Vector3(data.x,data.y,data.z), Quaternion.identity);
                    carObjects.Add(data.id, car);
                    newCarPositions.Add(data.id, new Vector3(data.x,data.y,data.z));
                    oldCarPositions.Add(data.id, new Vector3(data.x,data.y,data.z));
                }
            }
        }
    }

    private IEnumerator GetTrafficLights()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getTrafficLightEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        
        else 
        {
            trafficLightData = JsonUtility.FromJson<TrafficLightData>(www.downloadHandler.text);

            oldTrafficLightPositions = new List<Vector3>(newTrafficLightPositions);

            newTrafficLightPositions.Clear();

            foreach(Vector3 v in trafficLightData.positions)
                newTrafficLightPositions.Add(v);

            hold = false;
        }
    }

    private IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetCars());
            StartCoroutine(GetTrafficLights());
        }
    }


}
