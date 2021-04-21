using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace RDG.UnityButler {
  internal class Butler {

    private readonly ShellWrapper shell;

    public bool IsInstalled { get; private set; }

    public Butler(ShellWrapper shell) {
      this.shell = shell;
      shell.AddEnvVar("UNITY_APP_PATH", Application.dataPath);
      UpdateInstallStatus();
    }

    public bool Publish(string gamePath, string gameName) {
      return shell.Run($"butler push \"{gamePath}\" {gameName}", true, MakeButlerInstallPath());
      
    }
    
    public void Install() {
      var info = PackageInfo.FindForAssetPath("Packages/com.rundotgames.unitybutler");
      if (info == null) {
        throw new Exception("unable to locate unity butler package info");
      }
      
      if (!shell.Run("install-unity-butler.bat", true, Path.Combine(new[]{$"{info.resolvedPath}", "Editor", "Shell"}))) {
        throw new Exception("failed to install unity butler");
      }
      
      UpdateInstallStatus();
    }

    public void UpdateInstallStatus() {
      var installPath = MakeButlerInstallPath();
      IsInstalled = shell.Run("butler -v", false, installPath);
      if (!IsInstalled) {
        Debug.LogWarning($"Unable to find butler in {installPath}");
      }
    }
    
    private static string MakeButlerInstallPath() {
      return Path.Combine(new[]{
        $"{Application.dataPath}", "RDG", "UnityButler", "bin"
      });
    }
    
  }
}
