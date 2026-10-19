using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node3D
{
    public static int slotNumber;
    public static int currentLevel;
    public static GameManager Instance { get; private set; }
    

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        slotNumber = SaveManager.LoadSettingsValue("Settings", "Slot Number", 0);
        currentLevel = SaveManager.LoadValue(slotNumber, "Game Data", "Current Level", 0);
      


    }
  
}
