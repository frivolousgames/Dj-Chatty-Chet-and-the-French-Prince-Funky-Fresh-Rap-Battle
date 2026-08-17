using Godot;
using System;

public static partial class SaveManager
{
    private static string GetSavePath(int slotNumber)
    {
        return ($"user://savegame_slot_{slotNumber}.cfg");
    }

    
    public static void SaveValue<[MustBeVariant] T> (int slotNumber, string section, string key, T value)
    {
        ConfigFile config = new ConfigFile();
        string path = GetSavePath(slotNumber);

        if(FileAccess.FileExists(path))
        {
            config.Load(path);
        }

        config.SetValue(section, key, Variant.From(value));
        config.Save(path);
    }

    public static T LoadValue<[MustBeVariant] T> (int slotNumber, string section, string key, T defaultValue)
    {
        ConfigFile config = new ConfigFile();
        string path = GetSavePath(slotNumber);

        if (!FileAccess.FileExists(path) || config.Load(path) != Error.Ok)
        {
            return defaultValue;
        }

        return config.GetValue(section, key, Variant.From(defaultValue)).As <T>();
    }

    public static void SaveSettingsValue<[MustBeVariant] T>(string section, string key, T value)
    {
        ConfigFile config = new ConfigFile();
        string path = "user://settings.cfg";

        if (FileAccess.FileExists(path))
        {
            config.Load(path);
        }

        config.SetValue(section, key, Variant.From(value));
        config.Save(path);
    }

    public static T LoadSettingsValue<[MustBeVariant] T>(string section, string key, T defaultValue)
    {
        ConfigFile config = new ConfigFile();
        string path = "user://settings.cfg";

        if (!FileAccess.FileExists(path) || config.Load(path) != Error.Ok)
        {
            return defaultValue;
        }

        return config.GetValue(section, key, Variant.From(defaultValue)).As<T>();
    }
}
