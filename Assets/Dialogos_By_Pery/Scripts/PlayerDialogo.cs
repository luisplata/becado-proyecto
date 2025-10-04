using UnityEngine;

public class PlayerDialogo : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public bool canInteract = false;
    private NpcDialogo currentNPC;
    public ThirdPersonController playerBase;

    void Update()
    {
        if (canInteract && Input.GetKeyDown(interactKey) && currentNPC != null)
        {
            if (!currentNPC.IsDialogueActive)
            {
                playerBase.enabled = false; // Desactivar control del jugador
                currentNPC.StartDialogue(playerBase.transform);
            }
            else
            {
                currentNPC.ForceNextLine();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("NPC")) return;
        var npc = other.GetComponent<NpcDialogo>();
        if (npc == null) return;

        currentNPC = npc;

        // Defensive subscribe: eliminar antes de añadir para evitar múltiples suscripciones
        currentNPC.OnDialogueFinished -= OnDialogueFinished;
        currentNPC.OnDialogueFinished += OnDialogueFinished;

        Debug.Log("NPC cerca: " + currentNPC.name);
        UiManager.instance.ShowInteractPrompt();
        canInteract = true;
    }

    private void OnDialogueFinished()
    {
        // copia local para evitar race conditions si currentNPC cambia durante la ejecución
        var npc = currentNPC;

        playerBase.enabled = true; // Reactivar control del jugador

        if (npc != null)
        {
            // Desuscribir de forma segura
            npc.OnDialogueFinished -= OnDialogueFinished;

            // Si currentNPC sigue siendo ese npc, limpiarlo
            if (currentNPC == npc)
                currentNPC = null;
        }

        canInteract = false;
        Debug.Log("Diálogo terminado");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("NPC")) return;
        var npc = other.GetComponent<NpcDialogo>();
        if (npc == null) return;

        if (currentNPC != npc) return;

        // 1) DESUSCRIBIR antes de llamar EndDialog para evitar que el evento se dispare y
        //    cambie currentNPC mientras estamos en este método.
        currentNPC.OnDialogueFinished -= OnDialogueFinished;

        // 2) Llamamos EndDialog (no ejecutará OnDialogueFinished porque ya nos desuscribimos)
        currentNPC.EndDialog();

        // 3) Reactivamos control y limpiamos referencia
        playerBase.enabled = true;
        currentNPC = null;
        canInteract = false;

        Debug.Log("NPC fuera de alcance");
        UiManager.instance.HideInteractPrompt();
    }
}