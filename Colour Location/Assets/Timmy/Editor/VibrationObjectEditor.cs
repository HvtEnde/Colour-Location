using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VibrationObject))]
public class VibrationObjectEditor : Editor
{

    public override void OnInspectorGUI()
    {

        
        VibrationObject vibrationObject = (VibrationObject)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Vibration", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("This is the maximum amount of vibration on the controller.\nThe Toggle allows you to configure individual vibration for the left and right side of the controller (default is same on both sides).", MessageType.Info);
        vibrationObject.sameOnBothSides = EditorGUILayout.Toggle("Same on both sides", vibrationObject.sameOnBothSides);

        if (!vibrationObject.sameOnBothSides)
        {
            vibrationObject.powerLeft = EditorGUILayout.Slider("Vibration left", vibrationObject.powerLeft, 0f, 1f);
            vibrationObject.powerRight = EditorGUILayout.Slider("Vibration Right", vibrationObject.powerRight, 0f, 1f);
        }
        else
        {
            vibrationObject.powerLeft = EditorGUILayout.Slider("Vibration", vibrationObject.powerLeft, 0f, 1f);
            vibrationObject.powerRight = vibrationObject.powerLeft;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Object", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("This is the range within which the controllers will start to vibrate (white wire sphere) and the range at which point the vibration be at maximum (red wire sphere).", MessageType.Info);
        EditorGUILayout.LabelField("Min Val:", vibrationObject.minRadius.ToString());
        EditorGUILayout.LabelField("Max Val:", vibrationObject.maxRadius.ToString());
        EditorGUILayout.MinMaxSlider(ref vibrationObject.minRadius, ref vibrationObject.maxRadius, 0f, 15f);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(vibrationObject);
        }
    }


    void Start()
    {

    }

    void Update()
    {

    }
}
