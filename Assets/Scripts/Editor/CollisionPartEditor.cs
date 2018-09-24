using UnityEngine;
using UnityEditor;


/// <summary>
/// CollisionPart拡張
/// </summary>
[CustomEditor(typeof(CollisionPart)), CanEditMultipleObjects]
public sealed class CollisionPartEditor : Editor {
    /// <summary>
    /// Inspector表示
    /// </summary>
    public override void OnInspectorGUI() {
        SerializedProperty dataArrayProp = this.serializedObject.FindProperty("collisionDatas");

        GUILayout.Space(3f);

        int pointCount = dataArrayProp.arraySize;
        if (pointCount > 0) {
            for (int pointIndex = 0; pointIndex < pointCount; ++pointIndex) {
                SerializedProperty dataProp = dataArrayProp.GetArrayElementAtIndex(pointIndex);
                SerializedProperty formProp = dataProp.FindPropertyRelative("form");
                EditorGUILayout.PropertyField(formProp, new GUIContent("Form", "形状"));
                if (formProp.enumValueIndex == (int)COL_FORM.CIRCLE)
                    EditorGUILayout.PropertyField(dataProp.FindPropertyRelative("range"), new GUIContent("Range", "半径"));
                else if (formProp.enumValueIndex == (int)COL_FORM.RECTANGLE)
                    EditorGUILayout.PropertyField(dataProp.FindPropertyRelative("size"), new GUIContent("Size", "幅と高さ"));
                SerializedProperty offsetProp = dataProp.FindPropertyRelative("offset");
                Vector2 offset = offsetProp.vector3Value;
                offsetProp.vector3Value = EditorGUILayout.Vector2Field(new GUIContent("Offset", "オフセット"), offset);
                EditorGUILayout.Slider(dataProp.FindPropertyRelative("angle"), -180f, 180f, new GUIContent("Angle", "回転角"));

                GUILayout.BeginHorizontal();
                // コリジョン挿入
                if (GUILayout.Button("+", EditorStyles.miniButtonLeft)) {
                    ++pointCount;
                    dataArrayProp.InsertArrayElementAtIndex(pointIndex);
                }
                // コリジョン削除
                if (pointCount <= 1)
                    GUI.color = Color.gray;
                if (GUILayout.Button("-", EditorStyles.miniButtonRight) && pointCount > 1) {
                    dataArrayProp.DeleteArrayElementAtIndex(pointIndex);
                    --pointCount;
                    --pointIndex;
                }
                GUILayout.EndHorizontal();
            }
        } else {
            // コリジョン追加
            if (GUILayout.Button("+", EditorStyles.miniButtonLeft)) {
                ++pointCount;
                dataArrayProp.InsertArrayElementAtIndex(0);
            }
        }
        this.serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            this.UpdateParam();
    }

    /// <summary>
    /// コリジョンデータ更新
    /// </summary>
    private void UpdateParam() {
        SerializedProperty dataArrayProp = this.serializedObject.FindProperty("collisionDatas");

        for (int i = 0; i < dataArrayProp.arraySize; ++i) {
            SerializedProperty dataProp = dataArrayProp.GetArrayElementAtIndex(i);
            Vector3 dataOffset = dataProp.FindPropertyRelative("offset").vector3Value;

            switch (dataProp.FindPropertyRelative("form").enumValueIndex) {
                case (int)COL_FORM.CIRCLE:
                    dataProp.FindPropertyRelative("size").vector2Value = Vector2.zero;
                    float dataRange = dataProp.FindPropertyRelative("range").floatValue;
                    dataOffset.z = 0f;
                    break;
                case (int)COL_FORM.RECTANGLE:
                    Vector2 dataSize = dataProp.FindPropertyRelative("size").vector2Value;
                    Vector2 farPoint = new Vector2(System.Math.Abs(dataSize.x), System.Math.Abs(dataSize.y)) * 0.5f;
                    farPoint.x += System.Math.Abs(dataOffset.x);
                    farPoint.y += System.Math.Abs(dataOffset.z);
                    dataProp.FindPropertyRelative("range").floatValue = farPoint.magnitude;
                    break;
            }
        }
        this.serializedObject.ApplyModifiedProperties();
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    private static void DrawWireGizmo(Transform transform, GizmoType gizmoType) {
        CollisionPart t = transform.GetComponent<CollisionPart>();
        if (t == null)
            return;

        SerializedObject so = new SerializedObject(t);
        SerializedProperty dataArrayProp = so.FindProperty("collisionDatas");

        for (int i = 0; i < dataArrayProp.arraySize; ++i) {
            Gizmos.color = new Color(0f, 1f, 0f, 1f);

            SerializedProperty dataProp = dataArrayProp.GetArrayElementAtIndex(i);
            float range = dataProp.FindPropertyRelative("range").floatValue;
            int form = dataProp.FindPropertyRelative("form").enumValueIndex;
            Vector2 size = dataProp.FindPropertyRelative("size").vector2Value;
            Vector3 offset = dataProp.FindPropertyRelative("offset").vector3Value;
            float angle = dataProp.FindPropertyRelative("angle").floatValue;

            Quaternion rotation = t.transform.rotation;
            Vector3 position = t.transform.position + rotation * offset;
            Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.AngleAxis(t.transform.eulerAngles.z + angle, Vector3.forward), new Vector3(1f, 1f, 0.01f));
            if (form == (int)COL_FORM.CIRCLE) {
                Gizmos.DrawWireSphere(Vector3.zero, range);
            } else {
                Vector3 cubeSize = new Vector3(size.x, size.y);
                Gizmos.DrawWireCube(Vector3.zero, cubeSize);
            }
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
        }
    }
}
