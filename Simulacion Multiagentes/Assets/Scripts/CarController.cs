using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float acceleration = 0.1f;
    [SerializeField] private float maxSpeed = 10f;

    [SerializeField] private float currentSpeed = 0;
    private Vector3 target = Vector3.zero;
    private Rigidbody rb;
    private bool hasTarget;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hasTarget = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
        rb.constraints = RigidbodyConstraints.FreezePositionY;
    }

    private void Update()
    {
        if (target == Vector3.zero)
        {
            hasTarget = false;
            return;
        }
        if (Vector3.Distance(transform.position, target) <= .5)
        {
            target = Vector3.zero;
            return;
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(target - transform.position), rotationSpeed * Time.deltaTime);
        rb.velocity = transform.forward * currentSpeed;
        
        if (currentSpeed < maxSpeed) 
            currentSpeed += acceleration * Time.deltaTime * 10;
    }

    public void MoveTo(Vector3 target)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        this.target = target;
        hasTarget = true;
    }


    public float GetCurrentSpeed()
    {
        return rb.velocity.magnitude;
    }
    
    public bool GetHasTarget()
    {
        return hasTarget;
    }
}

