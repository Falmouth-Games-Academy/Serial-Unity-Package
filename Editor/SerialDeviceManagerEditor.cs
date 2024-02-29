using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SerialComManager)), CanEditMultipleObjects]
public class SerialDeviceManagerEditor : Editor
{
    private BuildTargetGroup buildTargetGroup;
    private SerialComManager serialManager;

    public override void OnInspectorGUI()
    {
        // Build target group is being depreceated
        // however no unity method exists at the moment to replace it.
        // TODO: Make a new method that uses NamedBuildTarget.
        buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        serialManager = target as SerialComManager;

        base.OnInspectorGUI();

        // Display text and button to change the API level.
        SetAPILevel();

    }

    /// <summary>
    /// Creates UI elements that check API level 
    /// and allow the user to press a button to change it if required.
    /// </summary>
    private void SetAPILevel()
    {
        // Check if the current project has the correct build settings.
        // If it does tell the user everything is fine,
        // else provide a prompt and a button to alert them.

        // TODO: Work out a neater way to produce.
        if (PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup) != ApiCompatibilityLevel.NET_4_6)
        {

            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.red;

            GUILayout.TextArea(
                "\n Unity Arduino manager requires API Compatibility level of 4.X.\n \n" +
                "Please change this in your project settings or press the button below.\n");

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Change API Level to .NET 4.6"))
            {
                // Directly changes the users API level in their project settings,
                // because using anything other than .NET 4.X will not allow the use of System.IO.Ports
                PlayerSettings.SetApiCompatibilityLevel(buildTargetGroup, ApiCompatibilityLevel.NET_4_6);
                SetGlobalDefine("SUP_API_SET");
            }
            GUILayout.EndVertical();
        }

        if (PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup) == ApiCompatibilityLevel.NET_4_6)
        {

            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.green;

            GUILayout.TextArea(
                "\nAPI compatibility level is set correctly to .NET 4.6\n\n" +
                "Unity Arduino should work as expected.\n");

            GUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Sets global define to avoid calls to System.IO.Ports, 
    /// when API level is not set correctly.
    /// </summary>
    private void SetGlobalDefine(string define)
    {
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (currentDefines.Contains(define))
        {
            Debug.LogWarning($"<b>{define}</b> Already exists in scripting defines for group <b>{buildTargetGroup}</b>");
            return;
        }

        // Defines are seperated by a ; in unity so add ;define onto the
        // string of current defines to add a new one.
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefines + ";" + define);
    }
}
