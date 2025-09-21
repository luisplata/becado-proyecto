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
            playerBase.enabled = !currentNPC.StartDialogue(playerBase.transform);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && other.GetComponent<NpcDialogo>() != null)
        {
            currentNPC = other.GetComponent<NpcDialogo>();
            Debug.Log("NPC cerca: " + currentNPC.name);
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") && other.GetComponent<NpcDialogo>() != null)
        {
            if (currentNPC == other.GetComponent<NpcDialogo>())
            {
                currentNPC.EndDialog();
                currentNPC = null;
                Debug.Log("NPC fuera de alcance");
                canInteract = false;
            }
        }
    }
}