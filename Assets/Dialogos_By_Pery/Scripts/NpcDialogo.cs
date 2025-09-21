using UnityEngine;

public class NpcDialogo : MonoBehaviour
{
    public string npcName;
    [TextArea(3, 10)] public string[] dialogLines;
    public Animator animator;
    public NpcDirectors[] playableDirectores;
    public Transform positionPlayer;
    public int currentLineIndex;
    public Transform playerBase;
    public bool canInteract;
    public AudioClip[] audioClips;
    public AudioSource audioSource;

    public bool StartDialogue(Transform playerBase)
    {
        canInteract = true;
        this.playerBase = playerBase;
        Debug.Log("Iniciando diálogo con " + npcName);
        if (currentLineIndex >= dialogLines.Length)
        {
            EndDialog();
            return false;
        }
        else
        {
            if (playableDirectores.Length > 0 && currentLineIndex <= playableDirectores.Length)
            {
                playableDirectores[currentLineIndex].Play();
            }

            Debug.Log(npcName + ": " + dialogLines[currentLineIndex]);
            animator.SetInteger("step", currentLineIndex + 1);
            if (audioSource.clip != null)
            {
                audioSource.Stop();
            }

            if (audioClips.Length > 0 && currentLineIndex < audioClips.Length && audioClips[currentLineIndex] != null)
            {
                audioSource.clip = audioClips[currentLineIndex];
                audioSource.Play();
            }

            currentLineIndex++;
            return true;
        }
    }

    private void Update()
    {
        if (canInteract && playerBase != null)
        {
            playerBase.rotation = positionPlayer.rotation;
            playerBase.position = positionPlayer.position;
        }
    }

    public void EndDialog()
    {
        Debug.Log("Diálogo terminado.");
        animator.SetInteger("step", dialogLines.Length + 1);
        currentLineIndex = 0; // Reinicia el diálogo
        canInteract = false;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        foreach (var director in playableDirectores)
        {
            director.Stop();
        }
    }
}