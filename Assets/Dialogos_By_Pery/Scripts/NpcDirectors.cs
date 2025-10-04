using System;
using UnityEngine;
using UnityEngine.Playables;

public class NpcDirectors : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public GameObject camera;

    // Evento para avisar cuando el director terminó
    public event Action<NpcDirectors> OnDirectorFinished;

    private void OnEnable()
    {
        if (camera != null)
            camera.SetActive(false);
    }

    public void Play()
    {
        if (playableDirector != null)
        {
            // Nos suscribimos al evento "stopped"
            playableDirector.stopped += HandleDirectorStopped;
            playableDirector.Play();
        }

        if (camera != null)
            camera.SetActive(true);
    }

    public void Stop()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= HandleDirectorStopped;
            playableDirector.Stop();
        }

        if (camera != null)
            camera.SetActive(false);
    }

    private void HandleDirectorStopped(PlayableDirector director)
    {
        // Importante: desuscribir para evitar fugas
        director.stopped -= HandleDirectorStopped;

        // Avisar a quien esté escuchando
        OnDirectorFinished?.Invoke(this);

        if (camera != null)
            camera.SetActive(false);
    }
}