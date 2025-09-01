using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionDistance = 3f;

    [Header("Referencias")]
    public Camera mainCamera;

    private bool isInCinematic = false;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private AudioSource audioSource;
    private ThirdPersonController playerMovement; // tu script de movimiento

    [SerializeField] private Animator animHUD_barritas;
    [SerializeField] public bool skipToEnd;

    [SerializeField] private GameObject e_para_interact;

    private NPC npcTarget; // NPC más cercano

    void Start()
    {
        // Creamos un AudioSource en el jugador
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Referencia al script de movimiento del jugador
        playerMovement = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (!isInCinematic)
        {
            DetectNearestNPC();

            if (npcTarget != null && Input.GetKeyDown(interactKey))
            {
                StartCoroutine(StartCinematic(npcTarget));
            }
        }

        if(npcTarget != null)
        {
            e_para_interact.SetActive(true);
        }
        else
        {
            e_para_interact.SetActive(false);
        }

        if (isInCinematic) {
            e_para_interact.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isInCinematic && !skipToEnd)
        {
            audioSource.time = audioSource.clip.length - 0.01f;
            skipToEnd = true;
        }
    }

    void DetectNearestNPC()
    {
        NPC[] allNPCs = FindObjectsOfType<NPC>();
        npcTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (NPC npc in allNPCs)
        {
            float dist = Vector3.Distance(transform.position, npc.transform.position);
            if (dist <= interactionDistance && dist < closestDist)
            {
                closestDist = dist;
                npcTarget = npc;
            }
        }
    }

    IEnumerator StartCinematic(NPC npc)
    {
        isInCinematic = true;

        // Guardamos la cámara original
        originalCamPos = mainCamera.transform.position;
        originalCamRot = mainCamera.transform.rotation;
        float originalFOV = mainCamera.fieldOfView;

        // Configuramos primera cámara
        if (npc.cinematicCameraPos != null)
        {
            mainCamera.transform.position = npc.cinematicCameraPos.position;
            mainCamera.transform.rotation = npc.cinematicCameraPos.rotation;
        }

        // Mover al jugador frente al NPC
        if (npc.playerSpot != null)
        {
            transform.position = npc.playerSpot.position;
            transform.rotation = npc.playerSpot.rotation; // hace que mire al NPC
        }

        animHUD_barritas.SetBool("show", true);
        mainCamera.fieldOfView = 35f;

        if (playerMovement != null)
            playerMovement.FixBugCinematicBoludon();
            playerMovement.enabled = false;

        npc.talk = true;

        // Reproducimos diálogo
        if (npc.npcDialogue != null)
        {
            audioSource.clip = npc.npcDialogue;
            audioSource.Play();

            // Mientras el audio está sonando controlamos los "cortes"
            yield return StartCoroutine(HandleCinematicCuts(npc));

            // Esperamos a que termine el audio

            if (!skipToEnd)
            {
                yield return new WaitForSeconds(audioSource.clip.length - audioSource.time);
            }
            // acá sigue el código que se ejecuta "al final"
        }

        // Restauramos cámara
        mainCamera.transform.position = originalCamPos;
        mainCamera.transform.rotation = originalCamRot;
        mainCamera.fieldOfView = originalFOV;

        audioSource.time = 0;

        animHUD_barritas.SetBool("show", false);
        npc.talk = false;
        skipToEnd = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        isInCinematic = false;
    }

    IEnumerator HandleCinematicCuts(NPC npc)
    {
        float lastCutTime = Time.time; // arranca con tiempo actual
        bool useAltCam = false;

        float[] samples = new float[256];

        while (audioSource.isPlaying)
        {
            // ignoramos primeros 0.5s del audio
            if (audioSource.time > 0.5f)
            {
                audioSource.GetOutputData(samples, 0);

                float sum = 0f;
                for (int i = 0; i < samples.Length; i++)
                    sum += samples[i] * samples[i];
                float rms = Mathf.Sqrt(sum / samples.Length);

                if (rms > 0.12f && Time.time - lastCutTime >= 2f) // umbral ajustable
                {
                    // cambiamos de cámara
                    if (useAltCam && npc.cinematicCameraPos != null)
                    {
                        mainCamera.transform.position = npc.cinematicCameraPos.position;
                        mainCamera.transform.rotation = npc.cinematicCameraPos.rotation;
                    }
                    else if (!useAltCam && npc.cinematicCameraPos2 != null)
                    {
                        mainCamera.transform.position = npc.cinematicCameraPos2.position;
                        mainCamera.transform.rotation = npc.cinematicCameraPos2.rotation;
                    }

                    useAltCam = !useAltCam;
                    lastCutTime = Time.time;
                }
            }

            yield return null;
        }
    }

}
