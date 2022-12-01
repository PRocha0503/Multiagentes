using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

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

[Serializable]
public class LightJSON
{
    public string id;
    public bool status;
    public int x;
    public int y;
    public int z;
}

[Serializable]
public class TrafficLightData
{
    public List<LightJSON> positions;
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
    Dictionary<string, Vector3> oldCarPositions;
    Dictionary<string, Vector3> newCarPositions;
    Dictionary<string, GameObject> carObjects;
    Dictionary<string, GameObject> trafficLightObjects;
    Dictionary<string, bool> trafficLightStatus;
    // Pause the simulation while we get the update from the server
    bool hold = false;

    public bool betterCarMovement;
    public GameObject[] carPrefabs; 
    public GameObject lightPrefab;
    public float timeToUpdate = 1.0f, timer, dt;
    
    // Start is called before the first frame update
    private void Start()
    {
        //Initialize variables
        carObjects = new Dictionary<string, GameObject>();
        carData = new CarData();
        trafficLightData = new TrafficLightData();
        oldCarPositions = new Dictionary<string, Vector3>();
        newCarPositions = new Dictionary<string, Vector3>();
        trafficLightObjects = new Dictionary<string, GameObject>();
        trafficLightStatus = new Dictionary<string, bool>();

        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());

    }
    
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
            if (!betterCarMovement)
            {
                if (oldCarPositions[newCarPosition.Key] == newCarPosition.Value) continue;
                
                Vector3 interpolated = Vector3.Lerp(oldCarPositions[newCarPosition.Key],
                    newCarPositions[newCarPosition.Key], dt);
                carObjects[newCarPosition.Key].transform.localPosition = interpolated;

                Vector3 dir = oldCarPositions[newCarPosition.Key] - newCarPositions[newCarPosition.Key];
                carObjects[newCarPosition.Key].transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                if (oldCarPositions[newCarPosition.Key] == newCarPosition.Value) continue;
                
                Vector3 nextPosition = newCarPositions[newCarPosition.Key];
                carObjects[newCarPosition.Key].GetComponent<CarController>().MoveTo(nextPosition, timeToUpdate);
            }
        }

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

            // Store the old positions for each agentF
            oldCarPositions = new Dictionary<string, Vector3>(newCarPositions);

            newCarPositions.Clear();
            

            foreach (CarJSON data in carData.cars)
            {
                if (carObjects.ContainsKey(data.id))
                {
                    newCarPositions[data.id] = new Vector3(data.x,data.y,data.z);
                }
                else
                {
                    GameObject car = Instantiate(carPrefabs[Random.Range(0,carPrefabs.Length)], new Vector3(data.x,data.y,data.z), Quaternion.identity);
                    car.transform.parent = transform;
                    carObjects.Add(data.id, car);
                    newCarPositions.Add(data.id, new Vector3(data.x,data.y,data.z));
                    oldCarPositions.Add(data.id, new Vector3(data.x,data.y,data.z));
                }
            }

            foreach (KeyValuePair<string,GameObject> carObject in carObjects.ToList())
            {
                if (!newCarPositions.ContainsKey(carObject.Key))
                {
                    Destroy(carObject.Value, 2f);
                    carObjects.Remove(carObject.Key);
                    oldCarPositions.Remove(carObject.Key);
                    newCarPositions.Remove(carObject.Key);
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

            trafficLightStatus.Clear();

            foreach (LightJSON lightData in trafficLightData.positions)
            {
                if (trafficLightObjects.Count < trafficLightData.positions.Count)
                {
                    GameObject lightObject = Instantiate(lightPrefab,
                        new Vector3(lightData.x, lightData.y, lightData.z), Quaternion.identity);
                    lightObject.transform.parent = transform;
                    trafficLightObjects.Add(lightData.id, lightObject);
                }
                trafficLightStatus.Add(lightData.id, lightData.status);
            }

            foreach (KeyValuePair<string,GameObject> trafficLightObject in trafficLightObjects)
            {
                Debug.Log(trafficLightStatus[trafficLightObject.Key]);
                trafficLightObject.Value.GetComponent<TrafficSignController>().SetLight(trafficLightStatus[trafficLightObject.Key]);
            }
            
            

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
