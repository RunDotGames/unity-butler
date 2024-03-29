﻿using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace RDG.UnityButler {

  [Serializable]
  internal class PackagePreferences : IEquatable<PackagePreferences>, ICloneable {
    public string itchAPIKey;
    public string itchGame;
    public string itchUser;
    public string slackWebhook;
    public string discordWebhook;
    public bool postToSlack;
    public bool postToDiscord;

    public bool Equals(PackagePreferences other) {
      if (other == null) {
        return false;
      }
      
      return itchAPIKey == other.itchAPIKey &&
             itchGame == other.itchGame &&
             itchUser == other.itchUser &&
             slackWebhook == other.slackWebhook &&
             postToSlack == other.postToSlack &&
             postToDiscord == other.postToDiscord &&
             discordWebhook == other.discordWebhook;
    }
    
    public object Clone() {
      return new PackagePreferences(){
        itchGame = itchGame,
        itchUser = itchUser,
        itchAPIKey = itchAPIKey,
        slackWebhook = slackWebhook,
        postToSlack = postToSlack,
        discordWebhook = discordWebhook,
        postToDiscord =  postToDiscord
      };
    }
  }
  
  internal class PackagePreferencesManager {

    private PackagePreferences preferences;
    private readonly ShellWrapper shell;

    public PackagePreferencesManager(ShellWrapper shell) {
      this.shell = shell;
      preferences = Deserialize();
      UpdateShell();
    }

    public PackagePreferences Update(PackagePreferences updated) {
      if (preferences.Equals(updated)) {
        return updated;
      }

      preferences = updated;
      Serialize(preferences);
      UpdateShell();
      return (PackagePreferences)preferences.Clone();
    }

    public PackagePreferences ClonePreferences() {
      return (PackagePreferences)preferences.Clone();
    }

    private void UpdateShell() {
      shell.AddEnvVar("BUTLER_API_KEY", preferences.itchAPIKey);
    }

    private static void Serialize(PackagePreferences toSerialize) {
      var preferencesPath = MakePreferencesPath();
      Directory.CreateDirectory(Path.GetDirectoryName(preferencesPath) ?? throw new Exception("unable to locate preferences"));
      File.WriteAllText(preferencesPath, JsonUtility.ToJson(toSerialize));
    }

    private static PackagePreferences Deserialize() {
      var preferencesPath = MakePreferencesPath();
      return !File.Exists(preferencesPath) ?
        new PackagePreferences() :
        JsonUtility.FromJson<PackagePreferences>(File.ReadAllText(preferencesPath));
    }

    private static string MakePreferencesPath() => Path.Combine(new[]{
      $"{Application.dataPath}", "RDG", "UnityButler", "preferences.json"
    });
    
  }
}
