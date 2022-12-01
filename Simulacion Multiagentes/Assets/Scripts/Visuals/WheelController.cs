using System;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    private CarController carController;
    [SerializeField] private float wheelRadius = 1f;
    
    private void Start()
    {
        carController = GetComponentInParent<CarController>();
    }


    private void Update()
    {
        /*float distanceTraveled = carController.GetCurrentSpeed() * Time.deltaTime;
        float rotationInRadians = distanceTraveled / wheelRadius;
        float rotationInDegrees = rotationInRadians * Mathf.Rad2Deg;
        
        
        transform.Rotate(0, -rotationInDegrees, 0);*/
    }
}