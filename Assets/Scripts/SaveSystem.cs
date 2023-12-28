using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static readonly string Path = Application.persistentDataPath + "/save.colorWater";

    public static void SaveGame()
    {
        var formatter = new BinaryFormatter();
        var stream = new FileStream(Path, FileMode.Create);
        var gameData = new Data();
        formatter.Serialize(stream, gameData);
        stream.Close();
    }

    public static Data LoadGame()
    {
        if (!File.Exists(Path)) return null;
        var formatter = new BinaryFormatter();
        var stream = new FileStream(Path, FileMode.Open);
        var gameData = formatter.Deserialize(stream) as Data;
        stream.Close();
        return gameData;
    }

    public static void DeleteSave()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}