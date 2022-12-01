using System.Collections;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private bool hasTarget;
    
    public void MoveTo(Vector3 target, float time)
    {
        hasTarget = true;
        StartCoroutine(Move(target, time));
    }

    IEnumerator Move(Vector3 target, float time)
    {
        target += new Vector3(0,0,-1f); 
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        Vector3 targetRotation = transform.position - target;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, target, (elapsedTime / time));
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetRotation), (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        hasTarget = false;
        StopAllCoroutines();
    }
    
    
    public bool GetHasTarget()
    {
        return hasTarget;
    }
}

