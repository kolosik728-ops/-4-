using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TacticalArena;
public static class Task4Builder {
    const string ScenePath="Assets/Scenes/MainMenu.unity", PrefabPath="Assets/Prefabs/Unit.prefab";
    [MenuItem("Task4/Создать сцену и префаб")]
    public static void Setup() {Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Prefabs");AssetDatabase.Refresh();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.gameObject.AddComponent<AudioListener>();camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.06f,.10f);camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);
        var obj=new GameObject("Unit");obj.AddComponent<UnitView>();var prefab=PrefabUtility.SaveAsPrefabAsset(obj,PrefabPath);Object.DestroyImmediate(obj);
        var game=new GameObject("GameController").AddComponent<GameController>();var so=new SerializedObject(game);so.FindProperty("unitPrefab").objectReferenceValue=prefab.GetComponent<UnitView>();so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};PlayerSettings.productName="Tactical Arena";PlayerSettings.companyName="Task4";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;AssetDatabase.SaveAssets();}
    [MenuItem("Task4/Собрать Windows x64")]
    public static void BuildWindows() {if(!File.Exists(ScenePath))Setup();PlayerSettings.productName="Tactical Arena";PlayerSettings.companyName="Task4";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;Directory.CreateDirectory("Build/Windows");var report=BuildPipeline.BuildPlayer(new[]{ScenePath},"Build/Windows/TacticalArena.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Сборка не завершена: "+report.summary.result);}
}
