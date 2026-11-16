using System;
using System.Collections.Generic;
using UnityEngine;
namespace TrainSummer {
[Serializable]public sealed class LanguageEntry {public string key,value;}
[Serializable]public sealed class LanguageCatalog {public LanguageEntry[] entries;}
public static class Localizer {
 static readonly Dictionary<string,string> values=new Dictionary<string,string>();
 public static bool English {get;private set;}
 public static void SetLanguage(bool english){English=english;values.Clear();var asset=Resources.Load<TextAsset>("Localization/menu_"+(english?"en":"ru"));if(asset==null)return;var catalog=JsonUtility.FromJson<LanguageCatalog>(asset.text);if(catalog?.entries==null)return;foreach(var entry in catalog.entries)values[entry.key]=entry.value;}
 public static string Text(string key)=>values.TryGetValue(key,out var value)?value:key;
}
}
