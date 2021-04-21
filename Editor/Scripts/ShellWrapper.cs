using System;
using System.Collections.Specialized;
using UnityEngine;

namespace RDG.UnityButler {
  
  internal class ShellWrapper {

    private readonly StringDictionary envVars = new StringDictionary();
    
    public void AddEnvVar(string key, string value) {
      envVars[key] = value;
    }
    
    public bool Run(string command, bool show, string workDir=null) {
      try {
        var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", $"/c {command}"){
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          CreateNoWindow = !show,
          UseShellExecute = false,
          WorkingDirectory = workDir
        };
        foreach (string envKey in envVars.Keys) {
          procStartInfo.EnvironmentVariables[envKey] = envVars[envKey];
        }
        var proc = new System.Diagnostics.Process{
          StartInfo = procStartInfo
        };
        proc.Start();
        var infoText = proc.StandardOutput.ReadToEnd();
        var errText = proc.StandardError.ReadToEnd();
        var isSuccess = proc.ExitCode == 0;
        if (!isSuccess) {
          Debug.LogError($"Command Error:\n{errText}\n\nCommand Out:\n{infoText}");
        }
        return isSuccess;
      }
      catch (Exception e) {
        Debug.LogError(e);
        return false;
      }

    }
  }
}
