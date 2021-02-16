using UnityEngine;

public class RigidbodyController : MonoBehaviour
{
    [SerializeField] private Rigidbody targetRigidbody = default;
    [SerializeField] private float movementSpeed = default;

    private Vector3 _direction = Vector3.zero;
    
    private void Start()
    {
        CheckDependencies();
    }

    private void CheckDependencies()
    {
        if (targetRigidbody == null)
        {
            enabled = false;
            Debug.LogWarning($"RigidbodyController: {gameObject.name} does not have a " +
                             "targetRigidbody assigned - it has been disabled.");            
        }
    }

    private void Update()
    {
        CalculateMovementDirection();
        MovePlayer();
    }

    private void CalculateMovementDirection()
    {
        var inputX = - Input.GetAxisRaw("Vertical");
        var inputY = Input.GetAxisRaw("Horizontal");
        _direction = new Vector3(inputX, 0, inputY).normalized;
    }
    
    private void MovePlayer()
    {
        var velocity = _direction * movementSpeed;
        targetRigidbody.velocity = velocity;
    }
}