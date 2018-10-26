using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerFile {

	public static void Save(FileDetails fileDetails)
    {
        string path = Application.persistentDataPath + "/saves/";

        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);

        path += fileDetails.filename + ".dat";

        FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, fileDetails);

        stream.Close();

        Debug.Log("Saved game at: " + path);
    }

    public static FileDetails Load(string path)
    {
        FileDetails fileDetails = new FileDetails();

        if (File.Exists(path) == false)
            return null;

        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();

        fileDetails = (FileDetails)bf.Deserialize(stream);

        stream.Close();

        return fileDetails;
    }

    public static FileDetails[] GetFiles()
    {
        
        string path = Application.persistentDataPath + "/saves/";

        if (Directory.Exists(path) == false)
            return null;

        Debug.Log("Directory exists");

        DirectoryInfo dir = new DirectoryInfo(path);

        //Debug.Log(dir.FullName);

        FileInfo[] files = dir.GetFiles("*.dat");

        //Debug.Log("Files count = " + files.Length);

        FileStream stream;
        BinaryFormatter bf = new BinaryFormatter();    

        FileDetails[] details = new FileDetails[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            stream = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            details[i] = (FileDetails)bf.Deserialize(stream);
            stream.Close();
        }
        
        return details;
    }
}

[Serializable]
public class FileDetails {

    public string filename;
    public int levelIndex;
    public float position_x, position_y;

    // TODO: add ability / item info

    public FileDetails()
    {
        filename = "Test";
        levelIndex = 0;
        position_x = position_y = 0;
    }

    public void SetFilename(string filename)
    {
        this.filename = filename;
    }

    public void SetDetails(Player player)
    {
        filename = player.fileDetails.filename;
        levelIndex = player.levelgrid.curIndex;
        position_x = player.rigidbody2d.position.x;
        position_y = player.rigidbody2d.position.y;
    }

    public void SetPlayer(ref Player player)
    {
        player.rigidbody2d.position = new Vector2(position_x, position_y);
        player.levelgrid.curIndex = levelIndex;
    }
    
}
