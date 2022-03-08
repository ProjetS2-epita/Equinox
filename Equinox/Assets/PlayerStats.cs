using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : MonoBehaviour
{
    [SerializeField] public string playerName;

    [SerializeField] public Sprite characterImage;

    [SerializeField] public int maxHP = 100;
    [SerializeField] public int currentHP;
    [SerializeField] public int maxSoif = 100;
    [SerializeField] public int currentSoif = 100;
    [SerializeField] public int MaxFaim = 100;
    [SerializeField] public int currentFaim = 100;
    [SerializeField] public int defense = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
