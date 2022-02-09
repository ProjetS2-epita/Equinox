using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [SerializeField] PlayerStats[] playerStats;
    // Start is called before the first frame update
    void Start()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        playerStats = FindObjectsOfType<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
