using UnityEditor;
using UnityEditor.Rendering.Universal;

[CustomEditor(typeof(SightRendererData), true)]
public class SightRendererDataEditor : ScriptableRendererDataEditor
{
    SerializedProperty m_OpaqueLayerMask;
    
    private void OnEnable()
    {
        m_OpaqueLayerMask = serializedObject.FindProperty("m_OpaqueLayerMask");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_OpaqueLayerMask);
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
