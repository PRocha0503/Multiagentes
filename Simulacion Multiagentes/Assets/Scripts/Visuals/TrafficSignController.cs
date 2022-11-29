using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TrafficSignController : MonoBehaviour
{
    [SerializeField] private GameObject greenLight;
    [SerializeField] private GameObject redLight;
    [SerializeField] private float rotationSpeed = 20f;
    
    MeshRenderer redLightRenderer;
    MeshRenderer greenLightRenderer;

    private void Awake()
    {
        greenLightRenderer = greenLight.GetComponent<MeshRenderer>();
        redLightRenderer = redLight.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        greenLight.transform.Rotate(rotationSpeed * Time.deltaTime, 0,0);
        redLight.transform.Rotate(rotationSpeed * Time.deltaTime, 0,0);
    }

    public void SetLight(bool isGreen)
    {
        greenLightRenderer.enabled = isGreen;
        redLightRenderer.enabled = !isGreen;

        //StartCoroutine(SlideColors(isGreen));
    }

    private IEnumerator SlideColors(bool isGreen)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        float slider = -.01f;

        while (elapsedTime < duration)
        {
            slider = Mathf.Lerp(-.01f, .01f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            
            if (isGreen)
            {
                foreach (Material material in greenLightRenderer.materials)
                {
                    material.SetFloat("Dissolve", slider);
                }

                foreach (Material material in redLightRenderer.materials)
                {
                    material.SetFloat("Dissolve", -slider);
                }
            }
            else
            {
                foreach (Material material in redLightRenderer.materials)
                {
                    material.SetFloat("Dissolve", slider);
                }
                
                foreach (Material material in greenLightRenderer.materials)
                {
                    material.SetFloat("Dissolve", -slider);
                }
            }
            yield return null;
        }
    }
    
}
