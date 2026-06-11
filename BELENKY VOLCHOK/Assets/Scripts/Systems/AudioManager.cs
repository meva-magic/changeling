using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning($"Sound '{sound}' not found in AudioManager!");
            return;
        }
        if (s.source == null)
        {
            Debug.LogWarning($"Sound source for '{sound}' is null!");
            return;
        }
        s.source.Play();
    }
    
    public void Stop(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning($"Sound '{sound}' not found in AudioManager!");
            return;
        }
        if (s.source == null)
        {
            Debug.LogWarning($"Sound source for '{sound}' is null!");
            return;
        }
        s.source.Stop();
    }
}