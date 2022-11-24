using System;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material breakLightMaterial;

    CarController carController;

    private void Start()
    {
        carController = GetComponent<CarController>();
    }


    private void Update()
    {
        HandleBreakLights();
    }

    private void HandleBreakLights()
    {
        // Used to simulate the break lights (When car has no target then its not moving)
        if (carController.GetHasTarget())
            breakLightMaterial.DisableKeyword("_EMISSION");
        else
            breakLightMaterial.EnableKeyword("_EMISSION");
    }
}
