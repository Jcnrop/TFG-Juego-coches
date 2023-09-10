//using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(carController))]
public class editorCarController : Editor
{
    SerializedProperty theRB;
    SerializedProperty barraNitro;
    SerializedProperty actualForwardSpeed;
    SerializedProperty reverseAccel;
    SerializedProperty maxSpeed;
    SerializedProperty turnStrenghtNormal;
    SerializedProperty turnStrenghtDerrape;
    SerializedProperty gravityForce;
    SerializedProperty dragOnGround;
    SerializedProperty nitroMax;
    SerializedProperty whatIsGround;
    SerializedProperty groundRayLength;
    SerializedProperty groundRayPoint;
    SerializedProperty heuristicaElegida;
    SerializedProperty esUnBot;

    //int selected = 0;

    private void OnEnable()
    {
        theRB = serializedObject.FindProperty("theRB");
        barraNitro = serializedObject.FindProperty("barraNitro");
        actualForwardSpeed = serializedObject.FindProperty("actualForwardSpeed");
        reverseAccel = serializedObject.FindProperty("reverseAccel");
        maxSpeed = serializedObject.FindProperty("maxSpeed");
        turnStrenghtNormal = serializedObject.FindProperty("turnStrenghtNormal");
        turnStrenghtDerrape = serializedObject.FindProperty("turnStrenghtDerrape");
        gravityForce = serializedObject.FindProperty("gravityForce");
        dragOnGround = serializedObject.FindProperty("dragOnGround");
        nitroMax = serializedObject.FindProperty("nitroMax");
        whatIsGround = serializedObject.FindProperty("whatIsGround");
        groundRayLength = serializedObject.FindProperty("groundRayLength");
        groundRayPoint = serializedObject.FindProperty("groundRayPoint");
        heuristicaElegida = serializedObject.FindProperty("heuristicaElegida");
        esUnBot = serializedObject.FindProperty("esUnBot");
    }

    public override void OnInspectorGUI()
    {
        /*base.OnInspectorGUI();

        var myScript = target as carController;

        myScript.EsUnBot = EditorGUILayout.Toggle("¿Es un bot?", myScript.EsUnBot);
        //myScript.Heuristica = myScript.Heuristica;

        if (myScript.EsUnBot)
        {
            selected = EditorGUILayout.Popup("Heurística", selected, myScript.Heuristica);
            myScript.HeuristicaElegida = myScript.Heuristica[selected];
        }*/

        carController _carController = (carController)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(theRB);
        EditorGUILayout.PropertyField(barraNitro);
        EditorGUILayout.PropertyField(actualForwardSpeed);
        EditorGUILayout.PropertyField(reverseAccel);
        EditorGUILayout.PropertyField(maxSpeed);
        EditorGUILayout.PropertyField(turnStrenghtNormal);
        EditorGUILayout.PropertyField(turnStrenghtDerrape);
        EditorGUILayout.PropertyField(gravityForce);
        EditorGUILayout.PropertyField(dragOnGround);
        EditorGUILayout.PropertyField(nitroMax);
        EditorGUILayout.PropertyField(whatIsGround);
        EditorGUILayout.PropertyField(groundRayLength);
        EditorGUILayout.PropertyField(groundRayPoint);
        EditorGUILayout.PropertyField(esUnBot);

        if (_carController.EsUnBot)
        {
            /*selected = EditorGUILayout.Popup("Heurística", selected, _carController.Heuristica);
            _carController.HeuristicaElegida = _carController.Heuristica[selected];*/

            EditorGUILayout.PropertyField(heuristicaElegida);
        }

        serializedObject.ApplyModifiedProperties();


    }
}
