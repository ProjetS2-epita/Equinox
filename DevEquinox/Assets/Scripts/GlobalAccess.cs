using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GlobalAccess
{
    readonly public static uint _maxEnemiesInSight = 200;
    readonly public static float _triggerUpdateRate = 0.5f;

    readonly public static string _Enemy = "Enemy";
    readonly public static string _Player = "Player";
    readonly public static string _Drone = "Drone";
    readonly public static string _MainCamera = "MainCamera";
    readonly public static string _GameManager = "GameManager";
    readonly public static string _DroneCamera = "DroneCamera";

    readonly public static string _lureSound = "Assets/Lure/Humming-Noise.mp3";
    readonly public static string _gunshotSound = "Assets/Player/Sounds/Hand Gun 1.wav";
    readonly public static string _droneFlyingSound = "Assets/Drone/Sounds/helicopter-sound-effect.mp3";

    readonly public static string _playerPrefab = "Assets/Player/ThirdPerson_Player.prefab";
    readonly public static string _dronePrefab = "Assets/Drone/drone.prefab";
    readonly public static string _lurePrefab = "Assets/Lure/Lure.prefab";
    readonly public static string _zombiePrefab = "Assets/Enemies/Zombie1/Zombie1.prefab";
}
