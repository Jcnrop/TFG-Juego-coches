using UnityEditor;

[CustomEditor(typeof(generarCoches))]

public class editorGenerarCoches : Editor
{
    SerializedProperty coche_principal; // Si es nulo, elige un coche al azar de entre los generados y se le asigna ser el coche principal
    SerializedProperty num_coches;
    SerializedProperty posiciones_coches;
    SerializedProperty prefabCoche;
    SerializedProperty prefabCollision;
    SerializedProperty prefabColisionSe�alizador;//Collider que se coloca al coche principal para realizar las se�alizaciones
    SerializedProperty generarCochePrincipal;
    SerializedProperty seguimientoCamara;

    private void OnEnable()
    {
        coche_principal = serializedObject.FindProperty("coche_principal");
        num_coches = serializedObject.FindProperty("num_coches");
        posiciones_coches = serializedObject.FindProperty("posiciones_coches");
        prefabCoche = serializedObject.FindProperty("prefabCoche");
        prefabCollision = serializedObject.FindProperty("prefabCollision");
        prefabColisionSe�alizador = serializedObject.FindProperty("prefabColisionSe�alizador");
        generarCochePrincipal = serializedObject.FindProperty("generarCochePrincipal");
        seguimientoCamara = serializedObject.FindProperty("seguimientoCamara");
    }

    public override void OnInspectorGUI()
    {
        generarCoches _genCochesController = (generarCoches)target;

        serializedObject.Update();


        EditorGUILayout.PropertyField(seguimientoCamara);
        EditorGUILayout.PropertyField(generarCochePrincipal);

        if (!_genCochesController.GenerarCochePrincipal)
        {
            EditorGUILayout.PropertyField(coche_principal);
        }

        EditorGUILayout.PropertyField(num_coches);
        EditorGUILayout.PropertyField(posiciones_coches);
        EditorGUILayout.PropertyField(prefabCoche);
        EditorGUILayout.PropertyField(prefabCollision);
        EditorGUILayout.PropertyField(prefabColisionSe�alizador);
        

        serializedObject.ApplyModifiedProperties();
    }
}
