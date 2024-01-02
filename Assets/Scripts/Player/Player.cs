using UnityEngine;

public class Player : MonoBehaviour
{
    Vector2 rotation = new Vector2(0, 0);
    public float rotationSpeed;

    float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CameraMovement();
    }

    void CameraMovement()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = rotation * rotationSpeed;

        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        transform.position += transform.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
    }


}
