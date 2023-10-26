using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip[] audios;
    public static MusicManager musicManager;
    AudioSource aS;

    void Awake() 
    {
        aS = GetComponent<AudioSource>();
        aS.Play();

    }

    void Update() 
    {
        
    }
}
