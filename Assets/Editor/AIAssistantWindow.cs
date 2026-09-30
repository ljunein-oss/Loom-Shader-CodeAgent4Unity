using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class AIAssistantWindow : EditorWindow
{
    const string Prefix = "UniversalCodingAgent.";
    const string DefaultSystem = "You are a senior Unity coding agent. Help write and debug C#, HLSL/ShaderLab, URP and editor tools. Give concrete code and explain file paths, assumptions and verification steps. Never claim to have modified files; return patches or complete snippets for the user to apply.";
    static string Provider { get => EditorPrefs.GetString(Prefix+"Provider", "DeepSeek"); set => EditorPrefs.SetString(Prefix+"Provider", value); }
    static string Endpoint { get => EditorPrefs.GetString(Prefix+"Endpoint", "https://api.deepseek.com/chat/completions"); set => EditorPrefs.SetString(Prefix+"Endpoint", value); }
    static string Model { get => EditorPrefs.GetString(Prefix+"Model", "deepseek-flash"); set => EditorPrefs.SetString(Prefix+"Model", value); }
    static string ApiKey { get => EditorPrefs.GetString(Prefix+"Key", ""); set => EditorPrefs.SetString(Prefix+"Key", value); }
    static string SystemPrompt { get => EditorPrefs.GetString(Prefix+"System", DefaultSystem); set => EditorPrefs.SetString(Prefix+"System", value); }
    static bool IncludeScene { get => EditorPrefs.GetBool(Prefix+"Scene", true); set => EditorPrefs.SetBool(Prefix+"Scene", value); }
    static bool IncludeSelection { get => EditorPrefs.GetBool(Prefix+"Selection", true); set => EditorPrefs.SetBool(Prefix+"Selection", value); }
    static bool IncludeLogs { get => EditorPrefs.GetBool(Prefix+"Logs", true); set => EditorPrefs.SetBool(Prefix+"Logs", value); }
    static string AttachedFile { get => EditorPrefs.GetString(Prefix+"File", ""); set => EditorPrefs.SetString(Prefix+"File", value); }

    [Serializable] class Msg { public string role; public string content; }
    [Serializable] class Req { public string model; public Msg[] messages; public float temperature = 0.2f; public bool stream = false; }
    [Serializable] class ClaudeReq { public string model, system; public Msg[] messages; public int max_tokens=4096; }
    [Serializable] class ClaudeBlock { public string type, text; }
    [Serializable] class ClaudeResp { public ClaudeBlock[] content; }
    bool requestClaude;
    [Serializable] class ChoiceMsg { public string content; }
    [Serializable] class Choice { public ChoiceMsg message; }
    [Serializable] class Resp { public Choice[] choices; }
    [Serializable] class ErrorBody { public ErrorInfo error; }
    [Serializable] class ErrorInfo { public string message; }
    class LogItem { public string type, text; }

    readonly List<Msg> history = new List<Msg>();
    readonly List<LogItem> logs = new List<LogItem>();
    string input = "", status = ""; bool settings, sending; Vector2 scroll; UnityWebRequest request;

    [MenuItem("Tools/Shader-CodeAgent4Unity/Open", false, 90)]
    static void Open() { var w = GetWindow<AIAssistantWindow>("Shader-CodeAgent4Unity"); w.minSize = new Vector2(520, 480); }
    void OnEnable() { if (Provider == "Google Gemini" || Provider == "Qwen / DashScope") { Provider = "DeepSeek"; ApplyPreset(Provider); EditorPrefs.DeleteKey(Prefix+"Key"); } Application.logMessageReceived += CaptureLog; EditorApplication.update += UpdateRequest; }
    void OnDisable() { if (modelRequest != null) { modelRequest.Dispose(); modelRequest=null; } Application.logMessageReceived -= CaptureLog; EditorApplication.update -= UpdateRequest; if (request != null) request.Dispose(); }
    void CaptureLog(string condition, string stack, LogType type) { if (type != LogType.Error && type != LogType.Exception && type != LogType.Warning) return; var line = condition.Split('\n')[0].Trim(); if (logs.Any(x => x.text == line)) return; logs.Add(new LogItem { type = type.ToString(), text = line }); if (logs.Count > 80) logs.RemoveAt(0); Repaint(); }

    void OnGUI() { Toolbar(); if (settings) Settings(); scroll = EditorGUILayout.BeginScrollView(scroll); foreach (var m in history) Bubble(m); if (sending) EditorGUILayout.LabelField("Waiting for model...", EditorStyles.miniLabel); EditorGUILayout.EndScrollView(); InputPanel(); if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info); }
    void Toolbar() { if (GUILayout.Button("Review latest generated code")) { var last = history.LastOrDefault(x => x.role == "assistant"); if (last != null) { var parts = last.content.Split(new[] { new string((char)96, 3) }, StringSplitOptions.None); if (parts.Length >= 3) { var block = parts[1]; var newline = block.IndexOf((char)10); QuillReviewWindow.Review(newline >= 0 ? block.Substring(newline + 1) : block); } else status = "No fenced code in latest response."; } } using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) { settings = GUILayout.Toggle(settings, "Settings", EditorStyles.toolbarButton, GUILayout.Width(65)); GUILayout.Label(Provider + " / " + Model, EditorStyles.miniLabel); GUILayout.FlexibleSpace(); if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50))) history.Clear(); if (GUILayout.Button("Attach file", EditorStyles.toolbarButton, GUILayout.Width(75))) PickFile(); if (GUILayout.Button("Copy prompt", EditorStyles.toolbarButton, GUILayout.Width(82))) CopyPrompt(); if (GUILayout.Button("Grab errors", EditorStyles.toolbarButton, GUILayout.Width(75))) GrabLog(); } }
    void Settings() { using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) { var names = new[] { "OpenAI / Codex", "Anthropic Claude", "DeepSeek", "GLM", "Ollama", "Custom" }; int idx = Mathf.Max(0, Array.IndexOf(names, Provider)); int next = EditorGUILayout.Popup("Provider", idx, names); if (next != idx) { Provider = names[next]; ApplyPreset(Provider); } Endpoint = EditorGUILayout.TextField("Chat endpoint", Endpoint); var models = ModelsFor(Provider); var options = new[] { "Select model / enter ID below" }.Concat(models).ToArray(); int mi = Array.IndexOf(models, Model) + 1; int mn = EditorGUILayout.Popup("Model", mi, options); if (mn > 0 && mn != mi) Model = models[mn - 1]; if (GUILayout.Button("Refresh models from provider")) FetchModels(); Model = EditorGUILayout.TextField("Custom model", Model); ApiKey = EditorGUILayout.PasswordField("API key", ApiKey); if (GUILayout.Button("Forget saved API key")) EditorPrefs.DeleteKey(Prefix+"Key"); if (GUILayout.Button("Detect endpoint compatibility")) DetectEndpoint(); SystemPrompt = EditorGUILayout.TextArea(SystemPrompt, GUILayout.Height(55)); IncludeScene = EditorGUILayout.ToggleLeft("Include active scene and render pipeline", IncludeScene); IncludeSelection = EditorGUILayout.ToggleLeft("Include selected objects and materials", IncludeSelection); IncludeLogs = EditorGUILayout.ToggleLeft("Include recent Console errors", IncludeLogs); if (!string.IsNullOrEmpty(AttachedFile)) EditorGUILayout.LabelField("Attached", AttachedFile); EditorGUILayout.LabelField("Keys are stored locally in Unity EditorPrefs.", EditorStyles.miniLabel); } }
    static string[] ModelsFor(string p) { var cached=EditorPrefs.GetString(ModelCacheKey, ""); if (!string.IsNullOrEmpty(cached)) return cached.Split((char)10);
        if (p == "OpenAI / Codex") return new[] { "gpt-6-astra", "gpt-6.1-sol", "gpt-6-luna" }; if (p == "Anthropic Claude") return new[] { "claude-fable-5-1", "claude-opus-5-5", "claude-sonnet-5-5" }; if (p == "DeepSeek") return new[] { "deepseek-flash", "deepseek-v4-pro" };
        if (p == "GLM") return new[] { "glm-5.3" };
        return string.IsNullOrEmpty(Model) ? new string[0] : new[] { Model };
    }
    static void ApplyPreset(string p) {
        Model = "";
        if (p == "OpenAI / Codex") { Endpoint = "https://api.openai.com/v1/chat/completions"; Model = "gpt-6-astra"; }
        else if (p == "Anthropic Claude") { Endpoint = "https://api.anthropic.com/v1/messages"; Model = "claude-opus-5-5"; }
        else if (p == "DeepSeek") { Endpoint = "https://api.deepseek.com/chat/completions"; Model = "deepseek-flash"; }
        else if (p == "GLM") { Endpoint = "https://open.bigmodel.cn/api/paas/v4/chat/completions"; Model = "glm-5.3"; }
        else if (p == "Google Gemini") Endpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
        else if (p == "Ollama") Endpoint = "http://localhost:11434/v1/chat/completions";
    }
    void DetectEndpoint() { if (sending) return; input = "Reply with API_OK"; Send(); }
    void Bubble(Msg m) { using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) { EditorGUILayout.LabelField(m.role == "user" ? "You" : "Agent", EditorStyles.miniBoldLabel); EditorGUILayout.SelectableLabel(m.content, new GUIStyle(EditorStyles.wordWrappedLabel)); } }
    void InputPanel() { using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) { input = EditorGUILayout.TextArea(input, GUILayout.MinHeight(60)); using (new EditorGUILayout.HorizontalScope()) { EditorGUI.BeginDisabledGroup(sending); if (GUILayout.Button("Send (Ctrl+Enter)", GUILayout.Height(26))) Send(); EditorGUI.EndDisabledGroup(); if (GUILayout.Button("Preview context", GUILayout.Width(120))) Debug.Log(BuildContext()); } var e = Event.current; if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return && (e.control || e.command)) { e.Use(); Send(); } } }
    void PickFile() { var p = EditorUtility.OpenFilePanel("Attach shader or script", Application.dataPath, "shader,compute,hlsl,shadergraph,cs,txt"); if (!string.IsNullOrEmpty(p)) AttachedFile = p; }
    string BuildContext() { var sb = new StringBuilder(); if (IncludeScene) { var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); sb.AppendLine("Scene: " + s.name + " (" + s.path + ")"); var rp = GraphicsSettings.currentRenderPipeline; sb.AppendLine("Render pipeline: " + (rp ? rp.GetType().Name : "Built-in")); } if (IncludeSelection && Selection.gameObjects.Length > 0) { sb.AppendLine("Selection:"); foreach (var go in Selection.gameObjects.Take(8)) { sb.AppendLine("- " + go.name); foreach (var r in go.GetComponents<Renderer>().Take(2)) foreach (var mat in r.sharedMaterials.Take(4)) if (mat) sb.AppendLine("  material " + mat.name + ", shader " + (mat.shader ? mat.shader.name : "missing")); } } if (IncludeLogs && logs.Count > 0) { sb.AppendLine("Recent Console:"); foreach (var l in logs.Skip(Math.Max(0, logs.Count - 12))) sb.AppendLine("[" + l.type + "] " + l.text); } if (!string.IsNullOrEmpty(AttachedFile) && File.Exists(AttachedFile)) { var text = File.ReadAllText(AttachedFile); if (text.Length > 30000) text = text.Substring(0, 30000); sb.AppendLine("Attached file: " + AttachedFile); sb.AppendLine("```\n" + text + "\n```"); } return sb.ToString().Trim(); }
    void GrabLog() { try { var p = Application.consoleLogPath; if (!File.Exists(p)) return; foreach (var line in File.ReadAllLines(p).Reverse().Take(3000).Where(x => x.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0).Take(20)) if (!logs.Any(x => x.text == line.Trim())) logs.Add(new LogItem { type = "Log", text = line.Trim() }); status = "Imported recent errors."; } catch (Exception e) { status = e.Message; } }
    void CopyPrompt() { var q = input.Trim(); EditorGUIUtility.systemCopyBuffer = q + "\n\n--- Unity context ---\n" + BuildContext(); status = "Prompt copied."; }
    void Send() { if (sending) return; if (string.IsNullOrWhiteSpace(Model)) { status = "Enter a verified API model ID; current model list is not verified for this provider."; return; } var q = input.Trim(); if (string.IsNullOrEmpty(q)) return;  if (string.IsNullOrEmpty(Endpoint) || (Endpoint.StartsWith("http") && string.IsNullOrEmpty(ApiKey) && Provider != "Ollama")) { settings = true; status = "Configure endpoint and API key first."; return; } var ctx = BuildContext(); var user = q + (string.IsNullOrEmpty(ctx) ? "" : "\n\n--- Unity context ---\n" + ctx); history.Add(new Msg { role = "user", content = q }); var msgs = new List<Msg> { new Msg { role = "system", content = SystemPrompt } }; msgs.AddRange(history); msgs[msgs.Count - 1] = new Msg { role = "user", content = user }; var body = JsonUtility.ToJson(new Req { model = Model, messages = msgs.ToArray() }); requestClaude = Provider == "Anthropic Claude"; if (requestClaude) body=JsonUtility.ToJson(new ClaudeReq { model=Model, system=SystemPrompt, messages=msgs.Skip(1).ToArray() }); input = ""; sending = true; status = "Requesting..."; request = new UnityWebRequest(Endpoint, "POST") { uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)), downloadHandler = new DownloadHandlerBuffer(), timeout = 180 }; request.SetRequestHeader("Content-Type", "application/json"); if (requestClaude) { request.SetRequestHeader("x-api-key", ApiKey); request.SetRequestHeader("anthropic-version", "2023-06-01"); } else if (!string.IsNullOrEmpty(ApiKey)) request.SetRequestHeader("Authorization", "Bearer " + ApiKey); request.SendWebRequest(); }
    void UpdateRequest() { PollModels(); if (request == null || !request.isDone) return; var r = request; request = null; sending = false; if (r.result != UnityWebRequest.Result.Success) { var msg = r.error; try { var eb = JsonUtility.FromJson<ErrorBody>(r.downloadHandler.text); if (eb != null && eb.error != null) msg = eb.error.message; } catch { } status = msg; history.Add(new Msg { role = "assistant", content = "Request failed: " + msg }); } else { try { var x = JsonUtility.FromJson<Resp>(r.downloadHandler.text); var reply = x.choices != null && x.choices.Length > 0 ? x.choices[0].message.content : "Empty response"; if(requestClaude) { var cr=JsonUtility.FromJson<ClaudeResp>(r.downloadHandler.text); reply=cr.content==null ? "Empty response" : string.Join("\n",cr.content.Where(b=>b.type=="text").Select(b=>b.text)); } history.Add(new Msg { role = "assistant", content = reply }); status = ""; } catch (Exception e) { status = "Parse error: " + e.Message; } } r.Dispose(); Repaint(); }

    [Serializable] class ModelItem { public string id; }
    [Serializable] class ModelList { public ModelItem[] data; public bool has_more; }
    UnityWebRequest modelRequest;
    string modelScope;
    static string ModelCacheKey { get { return Prefix + "Models." + Provider + "." + Endpoint; } }
    void FetchModels() {
        if (modelRequest != null) return;
        try {
            var uri = new Uri(Endpoint);
            var path = uri.AbsolutePath;
            string suffix = path.EndsWith("/chat/completions") ? "/chat/completions" : path.EndsWith("/messages") ? "/messages" : path.EndsWith("/responses") ? "/responses" : null;
            if (suffix == null) throw new Exception("Endpoint must end in /chat/completions, /responses or /messages.");
            var url = uri.GetLeftPart(UriPartial.Authority) + path.Substring(0, path.Length - suffix.Length) + "/models";
            modelScope = ModelCacheKey;
            modelRequest = UnityWebRequest.Get(url); modelRequest.timeout = 30;
            if (Provider == "Anthropic Claude") { modelRequest.SetRequestHeader("x-api-key", ApiKey); modelRequest.SetRequestHeader("anthropic-version", "2023-06-01"); }
            else if (!string.IsNullOrEmpty(ApiKey)) modelRequest.SetRequestHeader("Authorization", "Bearer " + ApiKey);
            modelRequest.SendWebRequest(); status = "Fetching account model IDs...";
        } catch(Exception e) { status=e.Message; if(modelRequest!=null)modelRequest.Dispose(); modelRequest=null; }
    }
    void PollModels() {
        if(modelRequest==null || !modelRequest.isDone)return;
        var r=modelRequest; modelRequest=null;
        try {
            if(r.result!=UnityWebRequest.Result.Success)throw new Exception("Model list request failed: HTTP " + r.responseCode + ". Manual model IDs remain available.");
            var data=JsonUtility.FromJson<ModelList>(r.downloadHandler.text);
            if(data==null||data.data==null)throw new Exception("Invalid model list response.");
            var ids=data.data.Where(x=>x!=null&&!string.IsNullOrEmpty(x.id)).Select(x=>x.id).Distinct().ToArray();
            if(ids.Length==0)throw new Exception("No models returned.");
            EditorPrefs.SetString(modelScope,string.Join("\n",ids));
            status="Loaded " + ids.Length + " account model IDs" + (data.has_more ? " (first page; more models available)." : ".") + " Select a text generation model compatible with the endpoint.";
        }catch(Exception e){status=e.Message;}finally{r.Dispose();Repaint();}
    }
}
