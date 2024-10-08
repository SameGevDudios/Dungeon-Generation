using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    public float _speed = 10.0f;
    public float _sensitivity = 5.0f;

    private float _rotationX = 0.0f;
    private float _rotationY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Camera rotation
        _rotationX += Input.GetAxis("Mouse X") * _sensitivity;
        _rotationY -= Input.GetAxis("Mouse Y") * _sensitivity;
        _rotationY = Mathf.Clamp(_rotationY, -90, 90);
        transform.localRotation = Quaternion.Euler(_rotationY, _rotationX, 0);

        // Movement
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Flight");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveX, moveY, moveZ);
        float speed = _speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 100;
        }
        transform.Translate(movement * speed * Time.deltaTime);
        // Toggle cursor lock with the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == 
                CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
