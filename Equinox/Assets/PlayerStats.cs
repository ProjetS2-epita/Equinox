using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [SerializeField] public string playerName;

    [SerializeField] public Sprite characterImage;

    [SerializeField] public int maxHP = 100;
    [SerializeField] public int currentHP;
    [SerializeField] public int maxFroid = 100;
    [SerializeField] public int currentFroid = 100;
    [SerializeField] public int MaxFaim = 100;
    [SerializeField] public int currentFaim = 100;
    [SerializeField] public int defense = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }


}
