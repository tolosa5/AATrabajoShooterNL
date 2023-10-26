using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gM;
    public bool detected;
    public bool finalEntered;
    public bool death;
    public bool grapp;
    public bool attack;
    public bool wallrun;

    // Start is called before the first frame update
    void Awake()
    {
        if (gM != this && gM != null)
        {
            Destroy(gameObject);
        }
        else
        {
            gM = this;
        }
        DontDestroyOnLoad(gameObject);
    }
}
