using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarController : MonoBehaviour
{
    private bool moveCar => Input.GetKey(clickKey);

    [SerializeField] private KeyCode clickKey = KeyCode.Mouse0;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxNavMeshProjectionDistance = 1f;
    [SerializeField] private float maxNavPathLength = 40f;

    private Vector3 target = Vector3.zero;


    private void Update()
    {
        if (moveCar) SetTarget();
        if (target == Vector3.zero) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(target - transform.position), rotationSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }

    private void SetTarget()
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
        if (!hasHit) return;
        
        target = hit.point;
        
        
        Debug.Log(target);
    }
    
    
    private static Ray GetMouseRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}

