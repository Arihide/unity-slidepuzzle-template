using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

public static class Builder
{
    private const string s_version = "1.0.0";

    [MenuItem("Builder/Make Common Settings")]
    private static void MakeCommonSettings()
    {
        PlayerSettings.bundleVersion = s_version;

        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

        // Android
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.androidTVCompatibility = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;

        // iOS
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        // NOTE: 自分のチームIDを入力すること
        PlayerSettings.iOS.appleDeveloperTeamID = "";
    }

    [MenuItem("Builder/Build Android Debug")]
    public static void BuildAndroidDebug()
    {
        MakeCommonSettings();

        PlayerSettings.Android.useCustomKeystore = false;

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        options.locationPathName = $"Build/{Application.productName}.apk";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Builder/Build Android Release")]
    public static void BuildAndroidRelease()
    {
        MakeCommonSettings();

        // NOTE: 本番用ビルド時には Data/Keystore/ 下にkeystoreファイルを入れて下記を修正すること
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = "Data/Keystore/upload.keystore";
        PlayerSettings.Android.keystorePass = "PASSWORD";
        PlayerSettings.Android.keyaliasName = "upload";
        PlayerSettings.Android.keyaliasPass = "PASSWORD";

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        options.locationPathName = $"Build/{Application.productName}.aab";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Builder/Build iOS Debug")]
    public static void BuildiOSDebug()
    {
        MakeCommonSettings();

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        options.locationPathName = "Builds/iOS";
        options.target = BuildTarget.iOS;
        options.options = BuildOptions.Development;
        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Builder/Build iOS Release")]
    public static void BuildiOSRelease()
    {
        MakeCommonSettings();

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        options.locationPathName = "Builds/iOS";
        options.target = BuildTarget.iOS;
        options.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(options);
    }
}

public class PostprocessBuild : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
            string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetString("CFBundleDevelopmentRegion", "ja");
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plist.WriteToFile(plistPath);

            string projPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            proj.SetBuildProperty(proj.GetUnityMainTargetGuid(), "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(proj.GetUnityFrameworkTargetGuid(), "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(proj.TargetGuidByName(PBXProject.GetUnityTestTargetName()), "ENABLE_BITCODE", "NO");
            proj.WriteToFile(projPath);
        }
    }
}