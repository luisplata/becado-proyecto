using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    public Transform cameraTransform;
    public Animator animator;
    public float speed;
    public float rotationSpeed = 720f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;
    public float cameraDistance = 4f;
    public float cameraHeight = 2f;

    [Header("Aiming Settings")]
    public float aimCameraDistance = 3f;
    public float aimCameraHeight = 2f;
    public float aimRotationSpeed = 10f;
    public float cameraSmooth = 6f;

    [Header("Enemy Lock-On")]
    public float lockOnRange = 12f;
    private Transform currentEnemy;
    public float lockSmooth = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraYaw = 0f;
    private float cameraPitch = 10f;

    public float TargetSpeed;
    public bool isRunning;
    public bool isMoving;
    public bool isAiming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Update()
    {
        DetectEnemy();
        HandleCamera();
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        isAiming = Input.GetMouseButton(1) && currentEnemy != null; // solo apunta si hay enemigo

        Vector3 moveDir = Quaternion.Euler(0, cameraYaw, 0) * inputDir;

        isMoving = inputDir.magnitude > 0.01f;
        bool isGrounded = controller.isGrounded;

        isRunning = Input.GetKey(KeyCode.LeftShift) && !isAiming;

        float targetSpeed = isRunning ? 5f : (isAiming ? 2f : 1.2f);

        speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * 10f);

        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isGrounded", isGrounded);
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isAiming", isAiming);
        }

        if (isMoving && !isAiming)
        {
            controller.Move(moveDir * speed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (isAiming && currentEnemy != null)
        {
            // jugador siempre mira al enemigo
            Vector3 dirToEnemy = (currentEnemy.position - transform.position);
            dirToEnemy.y = 0; // evitar rotación hacia arriba/abajo
            if (dirToEnemy.sqrMagnitude > 0.01f)
            {
                Quaternion enemyRot = Quaternion.LookRotation(dirToEnemy);
                transform.rotation = Quaternion.Slerp(transform.rotation, enemyRot, Time.deltaTime * aimRotationSpeed);
            }

            // movimiento lateral (strafe)
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            Vector3 move = (right * h + forward * v).normalized;
            controller.Move(move * speed * Time.deltaTime);
        }

        // gravedad
        if (isGrounded)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        if (!isAiming || currentEnemy == null)
        {
            // sensibilidad distinta para X e Y
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * (mouseSensitivity * 0.5f); // Y más lenta

            cameraYaw += mouseX;
            cameraPitch -= mouseY;

            // limitar pitch más fuerte (ejemplo: -20 abajo, 40 arriba)
            cameraPitch = Mathf.Clamp(cameraPitch, -20f, 40f);

            float currentDistance = Mathf.Lerp(cameraDistance, cameraDistance, Time.deltaTime * cameraSmooth);
            float currentHeight = Mathf.Lerp(cameraHeight, cameraHeight, Time.deltaTime * cameraSmooth);

            Vector3 targetPos = transform.position + Vector3.up * currentHeight;
            Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
            Vector3 desiredCamOffset = rotation * new Vector3(0, 0, -cameraDistance);
            Vector3 desiredCamPos = targetPos + desiredCamOffset;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredCamPos, Time.deltaTime * cameraSmooth);
            cameraTransform.LookAt(targetPos);
        }
        else
        {
            // === LOCK-ON ===
            // punto intermedio solo en el plano XZ
            Vector3 midpoint = (transform.position + currentEnemy.position) / 2f;
            midpoint.y = transform.position.y + 1.6f; // altura fija aprox. pecho del player

            // posición de cámara detrás del jugador
            Vector3 offset = new Vector3(0, aimCameraHeight, -aimCameraDistance);
            Vector3 desiredCamPos = transform.position + transform.TransformDirection(offset);

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredCamPos, Time.deltaTime * cameraSmooth);

            // enfocar hacia el midpoint (plano horizontal)
            Vector3 lookDir = (midpoint - cameraTransform.position).normalized;
            lookDir.y = 0; // quita inclinación vertical
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(lookDir, Vector3.up);
                cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, lookRot, Time.deltaTime * lockSmooth);
            }
        }
    }

    void DetectEnemy()
    {
        currentEnemy = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, lockOnRange);
        float closestDist = Mathf.Infinity;

        foreach (Collider c in hits)
        {
            if (c.CompareTag("enemy"))
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentEnemy = c.transform;
                }
            }
        }
    }
    public void FixBugCinematicBoludon()
    {
        animator.SetBool("isMoving", false);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockOnRange);
    }
}


