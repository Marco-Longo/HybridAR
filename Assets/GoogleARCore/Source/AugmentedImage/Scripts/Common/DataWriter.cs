using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class DataWriter
{
    private static string filename = "AccelDataFile.csv";

    public static void SaveFile(string msg)
    {
        string path = pathForDocumentsFile(filename);
        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(file);

        sw.WriteLine(msg);
        sw.Close();
        file.Close();
    }

    private static string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }
}
