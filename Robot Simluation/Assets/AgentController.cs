using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AgentData
{
    public string id;
    public float x, y, z;
    
    public AgentData(string id, float x, float y, float z)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[Serializable]

public class AgentsData
{
    public List<AgentData> positions;
    
    public AgentsData() => this.positions = new List<AgentData>();
}

public class AgentController : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string getRobotEndpoint = "/getRobots";
    string getBoxEndpoint = "/getBoxes";
    string getDepositEndpoint = "/getDeposits";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";

    private AgentsData robotsData, boxData, depositData;
    private Dictionary<string, GameObject> robots;
    private Dictionary<string, GameObject> crates;
    private Dictionary<string, GameObject> deposits;
    Dictionary<string, Vector3> prevRobotPositions, currRobotPositions, prevCratePositions, currCratePositions;

    private bool updated = false, started = false;

    [SerializeField] private GameObject robotPrefab, boxPrefab, depositPrefab, floor;
    [SerializeField] private int numRobots, width, height;
    [SerializeField, Range(0f, 1f)] private float crateDensity;
    [SerializeField] private float timeToUpdate = 5f;
    private float timer, dt;

    private void Start()
    {
        robotsData = new AgentsData();
        boxData = new AgentsData();
        depositData = new AgentsData();
        
        robots = new Dictionary<string, GameObject>();
        crates = new Dictionary<string, GameObject>();
        deposits = new Dictionary<string, GameObject>();
        
        prevRobotPositions = new Dictionary<string, Vector3>();
        currRobotPositions = new Dictionary<string, Vector3>();
        prevCratePositions = new Dictionary<string, Vector3>();
        currCratePositions = new Dictionary<string, Vector3>();
        
        floor.transform.localScale = new Vector3((float)width/10, 1, (float)height/10);
        floor.transform.localPosition = new Vector3((float)width/2-0.5f, 0, (float)height/2-0.5f);


        timer = timeToUpdate;
        StartCoroutine(SendConfiguration());
    }

    private void Update()
    {
        if (timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);


            foreach (var robot in currRobotPositions)
            {
                Vector3 currentRobotPosition = robot.Value;
                Vector3 prevRobotPosition = prevRobotPositions[robot.Key];
                
                Vector3 interpolated = Vector3.Lerp(prevRobotPosition, currentRobotPosition, dt);
                Vector3 direction = currentRobotPosition - interpolated;
                
                robots[robot.Key].transform.localPosition = interpolated;
                if (direction != Vector3.zero)
                {
                    robots[robot.Key].transform.forward = direction;
                }
            }
            
            foreach (var crate in currCratePositions)
            {
                Vector3 currentCratePosition = crate.Value;
                Vector3 prevCratePosition = prevCratePositions[crate.Key];
                
                Vector3 interpolated = Vector3.Lerp(prevCratePosition, currentCratePosition, dt);
                Vector3 direction = currentCratePosition - interpolated;
                
                crates[crate.Key].transform.localPosition = interpolated;
                if (direction != Vector3.zero)
                {
                    crates[crate.Key].transform.forward = direction;
                }
            }
        }
    }


    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
            StartCoroutine(GetRobotsData());
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("numRobots", numRobots.ToString());
        form.AddField("boxDensity", crateDensity.ToString());
        form.AddField("width", width.ToString());
        form.AddField("height", height.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
            Debug.Log("Getting Robots Data");
            StartCoroutine(GetRobotsData());
            Debug.Log("Getting Crates Data");
            StartCoroutine(GetBoxesData());
            Debug.Log("Getting Deposits Data");
            StartCoroutine(GetDepositsData());
        }


    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator GetRobotsData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getRobotEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            robotsData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach (AgentData robot in robotsData.positions)
            {
                Vector3 newRobotPosition = new Vector3(robot.x, robot.y, robot.z);

                if (!started)
                {
                    prevRobotPositions[robot.id] = newRobotPosition;
                    robots[robot.id] = Instantiate(robotPrefab, newRobotPosition, Quaternion.identity);
                }
                else
                {
                    Vector3 currentRobotPosition = new Vector3();
                    if (currRobotPositions.TryGetValue(robot.id, out currentRobotPosition))
                        prevRobotPositions[robot.id] = currentRobotPosition;
                    currRobotPositions[robot.id] = newRobotPosition;
                }
            }

            updated = true;
            if (!started) started = true;
            print("Robots updated");
        }
    }

    // Get Crates Data
    // ReSharper disable Unity.PerformanceAnalysis
    // private IEnumerator GetBoxesData()
    // {
    //     UnityWebRequest www = UnityWebRequest.Get(serverUrl + getBoxEndpoint);
    //     yield return www.SendWebRequest();
    //
    //     if (www.result != UnityWebRequest.Result.Success)
    //         Debug.Log(www.error);
    //     else
    //     {
    //         crateData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);
    //
    //         foreach (AgentData crate in crateData.positions)
    //         {
    //             Vector3 newCratePosition = new Vector3(crate.x, crate.y, crate.z);
    //
    //             if (!started)
    //             {
    //                 prevCratePositions[crate.id] = newCratePosition;
    //                 crates[crate.id] = Instantiate(cratePrefab, newCratePosition, Quaternion.identity);
    //             }
    //             else
    //             {
    //                 Vector3 currentCratePosition = new Vector3();
    //                 if (currCratePositions.TryGetValue(crate.id, out currentCratePosition))
    //                     prevCratePositions[crate.id] = currentCratePosition;
    //                 currCratePositions[crate.id] = newCratePosition;
    //             }
    //         }
    //
    //         updated = true;
    //         if (!started) started = true;
    //         print("Boxes updated");
    //     }
    // }
    
    // Get Boxes Data
    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator GetBoxesData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getBoxEndpoint);
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            boxData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach (var box in boxData.positions)
            {
                Instantiate(boxPrefab, new Vector3(box.x, box.y, box.z), Quaternion.identity);
            }
        }
        print ("Boxes updated");
            
    }
    
    // Get Deposits Data
    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator GetDepositsData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getDepositEndpoint);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            depositData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach (AgentData deposit in depositData.positions)
            {
                Instantiate(depositPrefab, new Vector3(deposit.x, deposit.y, deposit.z), Quaternion.identity);
            }
        }
        print("Deposits updated");
    }
}