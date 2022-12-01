using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TrafficSignController : MonoBehaviour
{
    [SerializeField] private GameObject greenLight;
    [SerializeField] private GameObject redLight;
    
    [SerializeField] private MeshRenderer greenLightMesh;
    [SerializeField] private MeshRenderer redLightMesh;
    
    [SerializeField] private float rotationSpeed = 2f;


    private void Update()
    {
        greenLight.transform.Rotate(rotationSpeed * Time.deltaTime, 0,0);
        redLight.transform.Rotate(rotationSpeed * Time.deltaTime, 0,0);
    }

    public void SetLight(bool isGreen)
    {
        greenLightMesh.enabled = isGreen;
        redLightMesh.enabled = !isGreen;
        
        //StartCoroutine(SlideColors(isGreen));
    }

    /*private IEnumerator SlideColors(bool isGreen)
    {
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            var slider = Mathf.Lerp(-.01f, .01f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            
            if (isGreen)
            {
                greenLightMaterial.SetFloat("Dissolve", slider);
                redLightMaterial.SetFloat("Dissolve", -slider);
            }
            else
            {
                redLightMaterial.SetFloat("Dissolve", slider);
                greenLightMaterial.SetFloat("Dissolve", -slider);
            }

            yield return null;
            StopAllCoroutines();
        }
    }*/

}
