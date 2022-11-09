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
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 0.1f;
    [SerializeField] private float maxSpeed = 10f;

    [SerializeField] private float currentSpeed = 0;
    private Vector3 target = Vector3.zero;
    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (moveCar) SetTarget();
        if (target == Vector3.zero) return;
        if (Vector3.Distance(transform.position, target) <= 1) target = Vector3.zero;
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(target - transform.position), rotationSpeed * Time.deltaTime);
        rb.velocity = transform.forward * currentSpeed;
        
        if (currentSpeed < maxSpeed) 
            currentSpeed += acceleration * Time.deltaTime * 10;
    }

    private void SetTarget()
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
        if (!hasHit) return;
        
        target = new Vector3(hit.point.x, transform.position.y, hit.point.z);
    }
    
    
    private static Ray GetMouseRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
    
    public float GetCurrentSpeed()
    {
        return rb.velocity.magnitude;
    }
}

