using UnityEngine;

public class Player : MonoBehaviour
{
    Vector2 rotation = new Vector2(0, 0);
    public float rotationSpeed;

    float moveSpeed;

    public float maxDist;
    public float walkSpeed;
    public float sprintSpeed;

    Transform camT;

    public Transform highlight;

    void Start()
    {
        camT = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CameraMovement();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Break();
        }

        Vector3 floatPos = CastRay();

        if (World.instance.IsBlockAt(floatPos))
        {
            highlight.position = new Vector3(Mathf.FloorToInt(floatPos.x),
                Mathf.FloorToInt(floatPos.y),
                Mathf.FloorToInt(floatPos.z)
                );

            if (Input.GetButtonDown("Fire1"))
            {
                BreakBlock(floatPos);
            }

            if (Input.GetButtonDown("Fire2"))
            {
                PlaceBlock(floatPos);
            }
        }
        else
        {
            highlight.position = new Vector3(0, -1000, 0);
        }

        if (Input.GetButton("Jump"))
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
        }
    }

    void CameraMovement()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = rotation * rotationSpeed;

        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        transform.position += Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime * transform.forward;
        transform.position += Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime * transform.right;
    }

    Vector3 CastRay()
    {
        float point = 0.0f;
        float step = 0.1f;

        Vector3 dir = camT.forward;
        Vector3 start = camT.position;
        Vector3 pos = start;

        while (point < maxDist)
        {
            if (World.instance.IsBlockAt(pos))
                break;

            pos += dir * step;

            point += step;
        }

        return pos;
    }

    void BreakBlock(Vector3 pos)
    {
        if (World.instance.IsBlockAt(pos))
        {
            World.instance.EditChunkBlockmap(pos, Utility.Blocks.Air);
        }
    }

    // TODO: FIX THIS
    void PlaceBlock(Vector3 pos)
    {
        float scale = 0.1f;
        Vector3 placePos = (pos - camT.forward * scale);

        if (!World.instance.IsBlockAt(placePos))
        {
            World.instance.EditChunkBlockmap(placePos, Utility.Blocks.Stone);
        }
    }
}
