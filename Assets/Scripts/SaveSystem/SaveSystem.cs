using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem {
    
    private static readonly string savePath = 
        Application.persistentDataPath + "/worldGenerator.save";

    public static void SaveGame(GameData data) {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static GameData LoadGame() {
        if (File.Exists(savePath)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();

            return data;
        } else {
            Debug.LogError("Save file not found");
        }

        return null;
    }
}
