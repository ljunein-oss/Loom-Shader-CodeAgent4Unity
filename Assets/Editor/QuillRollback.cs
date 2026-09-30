using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class QuillRollback {
[Serializable] public class Snapshot { public string path, bytes, meta; public bool existed, metaExisted, restored; }
static string Store { get { return Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/QuillSnapshots")); } }
static string Check(string path) {
var full=Path.GetFullPath(path);
if(!full.StartsWith(Path.GetFullPath(Application.dataPath)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)) throw new Exception("Choose a file inside Assets.");
for(var d=new DirectoryInfo(Path.GetDirectoryName(full));d!=null;d=d.Parent) if(d.Exists&&(d.Attributes&FileAttributes.ReparsePoint)!=0) throw new Exception("Linked directories are not supported.");
if(File.Exists(full)&&(File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0) throw new Exception("Linked files are not supported.");
return full;
}
public static void Write(string path,string text) {
path=Check(path); Check(path+".meta");
var s=new Snapshot {path=path,existed=File.Exists(path),metaExisted=File.Exists(path+".meta")};
s.bytes=s.existed?Convert.ToBase64String(File.ReadAllBytes(path)):"";
s.meta=s.metaExisted?Convert.ToBase64String(File.ReadAllBytes(path+".meta")):"";
Directory.CreateDirectory(Store);
File.WriteAllText(Path.Combine(Store,DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff")+".json"),JsonUtility.ToJson(s));
Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path,text); AssetDatabase.Refresh();
}
public static void Restore() {
if(!Directory.Exists(Store)) throw new Exception("No snapshots.");
foreach(var f in Directory.GetFiles(Store,"*.json").OrderByDescending(x=>x)) {
var s=JsonUtility.FromJson<Snapshot>(File.ReadAllText(f)); if(s.restored)continue;
Check(s.path);Check(s.path+".meta");
if(s.existed)File.WriteAllBytes(s.path,Convert.FromBase64String(s.bytes));else if(File.Exists(s.path))File.Delete(s.path);
if(s.metaExisted)File.WriteAllBytes(s.path+".meta",Convert.FromBase64String(s.meta));else if(File.Exists(s.path+".meta"))File.Delete(s.path+".meta");
s.restored=true;File.WriteAllText(f,JsonUtility.ToJson(s));AssetDatabase.Refresh();return;
} throw new Exception("No active snapshots.");
}
}
public class QuillReviewWindow:EditorWindow {
[SerializeField] string path="",code="";
[MenuItem("Tools/Shader-CodeAgent4Unity/File review and rollback")]
static void Open(){GetWindow<QuillReviewWindow>("Shader-CodeAgent4Unity Review");}
public static void Review(string text) { var w=GetWindow<QuillReviewWindow>("Shader-CodeAgent4Unity Review"); w.code=text; w.path=Application.dataPath+"/Generated.shader"; w.Show(); }
void OnGUI(){
EditorGUILayout.LabelField("Review file contents before applying");
path=EditorGUILayout.TextField("Absolute Assets path",path);
code=EditorGUILayout.TextArea(code,GUILayout.MinHeight(200));
if(GUILayout.Button("Apply with snapshot"))try{if(EditorUtility.DisplayDialog("Apply",path,"Apply","Cancel"))QuillRollback.Write(path,code);}catch(Exception e){Debug.LogError(e.Message);}
if(GUILayout.Button("Rollback latest"))try{if(EditorUtility.DisplayDialog("Rollback","Restores the last file and meta; overwrites subsequent manual edits.","Restore","Cancel"))QuillRollback.Restore();}catch(Exception e){Debug.LogError(e.Message);}
}
}
