using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnumFileGenerator
{
    public static void GenerateEnumFile(string fileName, List<string> enumNames)
    {
        string enumFileName = fileName;
        string filePathAndName = "Assets/Scripts/Enums/" + fileName + ".cs";
        List<string> names = new List<string>();
        for (int i = 0; i < enumNames.Count; i++)
        {
            if (names.Contains(enumNames[i]))
            {
                Debug.LogWarning("There is a duplicate reference name " + enumNames[i]);
                return;
            }
            names.Add(enumNames[i]);
        }

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {

            streamWriter.WriteLine("public enum " + fileName);
            streamWriter.WriteLine("{");

            for (int i = 0; i < names.Count - 1; i++)
            {
                streamWriter.WriteLine("	" + names[i] + ",");
            }
            streamWriter.WriteLine("	" + names[names.Count - 1]);
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save Project");
    }
}
