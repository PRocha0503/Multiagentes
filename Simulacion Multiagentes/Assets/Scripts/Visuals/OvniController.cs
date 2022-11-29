using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class OvniController : MonoBehaviour
{
    [SerializeField] private VisualEffect laserEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;
        laserEffect.Play();
    }
}
