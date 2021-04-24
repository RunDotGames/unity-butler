using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RDG.UnityButler {

  public class PublishWindow : EditorWindow {

    private Butler butler;
    private PackagePreferencesManager preferencesManager;
    private PackagePreferences preferences;

    private bool showHelp;
    
    [MenuItem("Window/Unity Butler")]
    public static void Init() {
      var window = (PublishWindow)GetWindow(typeof(PublishWindow), false, "Publish");
      window.Show();
    }
    
    private void InitState() {
      if (butler != null) {
        return;
      }

      var shell = new ShellWrapper();
      butler = new Butler(shell);
      preferencesManager = new PackagePreferencesManager(shell);
      preferences = preferencesManager.ClonePreferences();
    }

    private void OnGUI() {
      InitState();
      EditorGUILayout.Separator();
      DrawButlerWarning();
      EditorGUILayout.Separator();
      DrawItchSettings();
      EditorGUILayout.Separator();
      DrawSlackSettings();
      EditorGUILayout.Separator();
      GUI.enabled = butler.IsInstalled;
      if (GUILayout.Button("Publish",  GUILayout.ExpandWidth(false))) {
        Publish();
      }
      GUI.enabled = true;
    }
    
    private void DrawButlerWarning() {
      if (butler.IsInstalled) {
        return;
      }
      
      EditorGUILayout.Separator();
      EditorGUILayout.BeginVertical(GUI.skin.box);
      EditorGUILayout.HelpBox("Butler Is Not Installed", MessageType.Error);
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Install Butler", GUILayout.ExpandWidth(false))) {
        butler.Install();
      }
      if (GUILayout.Button("Re-Check", GUILayout.ExpandWidth(false))) {
        butler.UpdateInstallStatus();
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.Separator();
      EditorGUILayout.EndVertical();
    }

    private void DrawSlackSettings() {
      EditorGUILayout.LabelField("Slack Settings");
      EditorGUILayout.BeginVertical(GUI.skin.box);
      preferences.slackWebhook = EditorGUILayout.TextField("Slack Webhook", preferences.slackWebhook);
      preferences.postToSlack = EditorGUILayout.Toggle("Post To Slack", preferences.postToSlack);
      preferences = preferencesManager.Update(preferences);
      EditorGUILayout.EndVertical();
    }

    private void DrawItchSettings() {
      EditorGUILayout.BeginHorizontal();
      if (!showHelp && GUILayout.Button("?", GUILayout.ExpandWidth(false))) {
        showHelp = true;
      }
      EditorGUILayout.LabelField("Itch.io Settings", GUILayout.ExpandWidth(false));
      EditorGUILayout.EndHorizontal();
      DrawItchHelp();
      EditorGUILayout.BeginVertical(GUI.skin.box);
      preferences.itchAPIKey = EditorGUILayout.TextField("API Key", preferences.itchAPIKey);
      preferences.itchUser = EditorGUILayout.TextField("Username", preferences.itchUser);
      preferences.itchGame = EditorGUILayout.TextField("Game", preferences.itchGame);
      EditorGUILayout.EndVertical();
    }

    private void DrawItchHelp() {
      if (!showHelp) {
        return;
      }
      
      EditorGUILayout.BeginVertical(GUI.skin.box);
      GUILayout.Label("Generate new API Keys");
      GUILayout.TextField("https://itch.io/user/settings/api-keys");
      EditorGUILayout.Separator();
      GUILayout.Label("Butler publish documentation (username/game info)");
      GUILayout.TextField("https://itch.io/docs/butler/pushing.html");
      if (GUILayout.Button("Hide", GUILayout.ExpandWidth(false))) {
        showHelp = false;
      }
      EditorGUILayout.EndVertical();
    }

    
    private void Publish() {
      if (string.IsNullOrEmpty(preferences.itchAPIKey)) {
        throw new Exception("Butler API Key Required");
      }

      if (string.IsNullOrEmpty(preferences.slackWebhook) && preferences.postToSlack) {
        throw new Exception("Webhook required when posting to slack");
      }
      
      Debug.Log("Building WebGL...");
      var webGlPath = MakeTempBuildPath();
      var report = BuildPipeline.BuildPlayer(GetEnabledScenes(), webGlPath, BuildTarget.WebGL, BuildOptions.None);
      if (report.summary.result != BuildResult.Succeeded) {
        throw new Exception($"failed to build for webgl: {report.summary.result}");
      }

      Debug.Log("Uploading to Itch.io");
      var uploadResult = butler.Publish(webGlPath, $"{preferences.itchUser}/{preferences.itchGame}:webgl");
      Directory.Delete(webGlPath, true);
      if (!uploadResult) {
        throw new Exception("failed to upload to itch.io");
      }

      if (!preferences.postToSlack) {
        Debug.Log("Done.");
        return;
      }

      Debug.Log("Posting to slack...");
      PostToSlack(":tada: New Browser Playable Build Published").ContinueWith((success) => {
        if (!success.Result) {
          Debug.LogError("Failed Posting To Slack");
          return;
        }

        Debug.Log("Done.");
      });
    }


    private static string[] GetEnabledScenes() {
      return EditorBuildSettings.scenes
        .Where((scene) => scene.enabled && !string.IsNullOrEmpty(scene.path))
        .Select((scene) => scene.path).ToArray();
    }

    

    private async Task<bool> PostToSlack(string message) {
      var myJson = "{\"text\":\"" + message + "\"}";
      var client = new HttpClient();
      var response = await client.PostAsync(preferences.slackWebhook, new StringContent(myJson, Encoding.UTF8, "application/json"));
      return response.StatusCode == HttpStatusCode.OK;
    }
    
    private static string MakeTempBuildPath() {
      return Path.Combine(new[]{ Path.GetTempPath(), Path.GetRandomFileName() });
    }
  }
}