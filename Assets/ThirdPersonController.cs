using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    public Transform cameraTransform;
    public Animator animator;
    public float speed = 5f;
    public float rotationSpeed = 720f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;
    public float cameraDistance = 4f;
    public float cameraHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraYaw = 0f;
    private float cameraPitch = 10f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Update()
    {
        HandleCamera();
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        Vector3 moveDir = Quaternion.Euler(0, cameraYaw, 0) * inputDir;

        bool isMoving = inputDir.magnitude > 0.01f;
        bool isGrounded = controller.isGrounded;

        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isGrounded", isGrounded);
        }

        if (isMoving)
        {
            controller.Move(moveDir * speed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplicar gravedad
        if (isGrounded)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        cameraYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        cameraPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -35f, 60f);

        Vector3 targetPos = transform.position + Vector3.up * cameraHeight;
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 desiredCamOffset = rotation * new Vector3(0, 0, -cameraDistance);
        Vector3 desiredCamPos = targetPos + desiredCamOffset;

        // Raycast para detectar colisión con pared
        RaycastHit hit;
        Vector3 rayDir = desiredCamPos - targetPos;
        float rayDistance = cameraDistance;

        if (Physics.Raycast(targetPos, rayDir.normalized, out hit, rayDistance))
        {
            // Si choca con algo, acercamos la cámara hasta el punto de impacto
            desiredCamPos = hit.point - rayDir.normalized * 0.2f; // leve offset para que no atraviese
        }

        cameraTransform.position = desiredCamPos;
        cameraTransform.LookAt(targetPos);
    }

}
