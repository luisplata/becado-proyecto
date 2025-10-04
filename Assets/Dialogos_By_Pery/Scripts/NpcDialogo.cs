using System;
using UnityEngine;
using System.Collections;

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

    private bool directorFinished;
    private Coroutine waitCoroutine;

    public bool IsDialogueActive { get; private set; }

    public Action OnDialogueFinished;

    // 🚀 Inicia el diálogo
    public void StartDialogue(Transform playerBase)
    {
        canInteract = true;
        IsDialogueActive = true;
        this.playerBase = playerBase;
        currentLineIndex = 0; // siempre arranca desde el inicio

        ShowLine();
    }

    // 🔹 Muestra la línea actual
    private void ShowLine()
    {
        if (currentLineIndex >= dialogLines.Length)
        {
            EndDialog();
            return;
        }

        string line = dialogLines[currentLineIndex];
        Debug.Log($"{npcName}: {line}");

        // Mostrar en UI
        UiManager.instance?.ShowDialogPanel(npcName, line);

        // Animación
        if (animator != null)
            animator.SetInteger("step", currentLineIndex + 1);

        // Director
        if (playableDirectores.Length > 0 && currentLineIndex < playableDirectores.Length)
        {
            var director = playableDirectores[currentLineIndex];
            directorFinished = false;
            director.OnDirectorFinished += OnDirectorFinishedHandler;
            director.Play();
        }
        else
        {
            directorFinished = true;
        }

        // Audio
        if (audioClips.Length > 0 && currentLineIndex < audioClips.Length && audioClips[currentLineIndex] != null)
        {
            audioSource.clip = audioClips[currentLineIndex];
            audioSource.Play();
        }

        // Cancelar cualquier corutina anterior antes de arrancar otra
        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);

        waitCoroutine = StartCoroutine(WaitForLineEnd());
    }

    private void OnDirectorFinishedHandler(NpcDirectors director)
    {
        director.OnDirectorFinished -= OnDirectorFinishedHandler;
        directorFinished = true;
    }

    private IEnumerator WaitForLineEnd()
    {
        while (true)
        {
            // Avance automático (terminaron audio y director)
            bool autoAdvance = !audioSource.isPlaying && directorFinished;

            if (autoAdvance)
            {
                NextLine();
                yield break; // salir y limpiar corutina
            }

            yield return null;
        }
    }

    // 🔹 Avanza a la siguiente línea
    public void NextLine()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        foreach (var director in playableDirectores)
            director.Stop();

        currentLineIndex++;
        ShowLine();
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
        if (animator != null)
            animator.SetInteger("step", dialogLines.Length + 1);

        currentLineIndex = 0;
        canInteract = false;
        IsDialogueActive = false;

        if (audioSource.isPlaying)
            audioSource.Stop();

        foreach (var director in playableDirectores)
            director.Stop();

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        // 🔹 Ocultar panel UI
        UiManager.instance?.HideDialogPanel();

        // Avisar al PlayerDialogo
        OnDialogueFinished?.Invoke();
    }

    // 🔹 Avance manual desde PlayerDialogo
    public void ForceNextLine()
    {
        if (IsDialogueActive)
        {
            NextLine();
        }
    }
}
