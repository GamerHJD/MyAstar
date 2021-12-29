/****************************************************
    文件：CellEditor.cs
	作者：HJD
    邮箱: 491799022@qq.com
    日期：#CreateTime#
	功能：网格属性编辑
*****************************************************/

using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Cell))]//XXX是我们要扩展Inspector面板的脚本名,以下我们称之为目标脚本
[CanEditMultipleObjects]
public class CellEditor : Editor
{
    

    SerializedProperty canMove;
    void OnEnable()
    {
        canMove = serializedObject.FindProperty("canMove");
    }
    public override void OnInspectorGUI()
    {
        // 官方说要先这样
        DrawDefaultInspector();//绘制原来的变量
        serializedObject.Update();

        EditorGUILayout.BeginVertical();//垂直布局

        if (canMove.hasMultipleDifferentValues)
        {
            canMove.boolValue = EditorGUILayout.Toggle("canMove", canMove.boolValue);
        }
        if (GUILayout.Button("SetMoveFalse"))
        {
            canMove.boolValue = EditorGUILayout.Toggle("canMove", false);
            ChangeCellColor();
            serializedObject.ApplyModifiedProperties();
        }
        if(GUILayout.Button("SetMoveTrue"))
        {
            canMove.boolValue = EditorGUILayout.Toggle("canMove", true);
           ChangeCellColor();
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    void ChangeCellColor()
    {
        if (Selection.gameObjects.Length > 0)
        {


            foreach (var go in Selection.gameObjects)
            {

                Cell cell = go.GetComponent<Cell>();
                if (cell != null)
                {


                    cell.SetMateril();


                }



            }

        }
    }
}