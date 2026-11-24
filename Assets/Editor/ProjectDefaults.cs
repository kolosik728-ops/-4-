using UnityEditor;
[InitializeOnLoad]
public static class ProjectDefaults {
    static ProjectDefaults() { EditorApplication.delayCall += Apply; }
    static void Apply() {
        if(PlayerSettings.productName!="Tactical Arena") PlayerSettings.productName="Tactical Arena";
        if(PlayerSettings.companyName!="Task4") PlayerSettings.companyName="Task4";
        if(PlayerSettings.defaultScreenWidth!=1280) PlayerSettings.defaultScreenWidth=1280;
        if(PlayerSettings.defaultScreenHeight!=800) PlayerSettings.defaultScreenHeight=800;
    }
}
