using System.Collections.Generic;
using UnityEngine;

// Make Serializable to Save Changes
[System.Serializable]
public class Data
{
    // Game Settings
    [SerializeField] public bool soundStatus;
    [SerializeField] public bool hapticStatus;

    // Game Levels
    [SerializeField] public int curGameLevel;

    public Data()
    {
        curGameLevel = GameController.CurGameLevel;
    }
}