using UnityEngine;

public class Vibration : MonoBehaviour
{
    readonly float speed = 5.0f;
    readonly float intensity = 0.1f;
    private float randomSpeed;
        
    // Random Speed

    private void Start()
    {
        randomSpeed = Random.Range(speed - 1f, speed);
    }

    void Update()
    {
        
 
        transform.localPosition = intensity * new Vector3(
            Mathf.PerlinNoise(randomSpeed * Time.time, 1),
            Mathf.PerlinNoise(randomSpeed * Time.time, 2),
            Mathf.PerlinNoise(randomSpeed * Time.time, 3));
    }
}
