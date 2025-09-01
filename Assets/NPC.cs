using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Datos del NPC")]
    public string npcID = "default";          // Ej: "sergio"
    public Transform cinematicCameraPos;      // Cámara para la cinemática
    public Transform cinematicCameraPos2; // segunda cámara opcional
    public Transform playerSpot; // punto frente al NPC donde se coloca el jugador
    public AudioClip npcDialogue;             // Audio del NPC
    [SerializeField] private Animator anim;
    public bool talk;

    private void Update()
    {
        anim.SetBool("talk", talk);
    }
}
