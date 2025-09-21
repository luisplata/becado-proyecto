using System;
using UnityEngine;
using UnityEngine.Playables;

public class NpcDirectors : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public GameObject camera;

    private void OnEnable()
    {
        camera.SetActive(false);
    }

    public void Play()
    {
        if (playableDirector != null)
        {
            playableDirector.Play();
        }

        if (camera != null)
        {
            camera.SetActive(true);
        }
    }

    public void Stop()
    {
        if (playableDirector != null)
        {
            playableDirector.Stop();
        }

        if (camera != null)
        {
            camera.SetActive(false);
        }
    }
}