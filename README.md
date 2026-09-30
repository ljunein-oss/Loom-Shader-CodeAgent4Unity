# Universal Coding Agent for Unity

## 中文

Universal Coding Agent 是一个运行在 Unity Editor 内的通用 AI 编程代理窗口，面向 ShaderLab/HLSL、Compute Shader、C#、URP/HDRP 和编辑器工具的编写与排错。

### 功能

- Provider 下拉选择：OpenAI/Codex、Claude、DeepSeek、GLM、Gemini、Qwen/DashScope、Ollama、Custom。
- 每个 Provider 提供主流模型下拉列表；Custom 可手动填写模型。
- Endpoint 自动切换预设，也可以手动修改。
- Endpoint compatibility 检测按钮，用一次最小请求验证接口可达性和响应格式。
- 自动附带当前场景、Render Pipeline、选中对象/材质/Shader 和 Console 错误。
- 可附加 `.shader`、`.compute`、`.hlsl`、`.shadergraph`、`.cs` 或文本文件。
- 只返回建议和代码，不自动修改工程文件，方便审阅和回滚。

### 安装

将 `Assets/Editor/AIAssistantWindow.cs` 和 `Assets/UniversalCodingAgent/` 放入 Unity 工程，等待 Unity 编译。通过 `Tools > Universal Coding Agent` 打开窗口。

### 配置

在 Settings 中选择 Provider、Model，填写 API Key。API Key 仅保存到本机 Unity EditorPrefs，不写入工程文件。Ollama 默认使用本机 `http://localhost:11434/v1/chat/completions`。

### API 兼容性

除 Claude 原生 endpoint 外，默认请求格式为 OpenAI Chat Completions：`model`、`messages`、`temperature`、`stream:false`，读取 `choices[0].message.content`。使用不兼容该协议的服务时，请配置兼容网关或 Custom endpoint。

### 安全和限制

不要提交 API Key。插件是代理窗口，不会自动执行 shell、写文件或提交代码。发送大型 Shader 前建议只附加相关文件，避免超出模型上下文限制。

## English

Universal Coding Agent is a Unity Editor AI coding proxy for ShaderLab/HLSL, Compute Shaders, C#, URP/HDRP and Unity editor tooling.

### Features

- Provider dropdown: OpenAI/Codex, Claude, DeepSeek, GLM, Gemini, Qwen/DashScope, Ollama and Custom.
- Popular models are selectable per provider; Custom allows a manual model name.
- Endpoints are populated automatically and remain editable.
- Endpoint compatibility check sends a minimal request to verify reachability and response shape.
- Optional context from the active scene, render pipeline, selected objects/materials/shaders and Console errors.
- Attach `.shader`, `.compute`, `.hlsl`, `.shadergraph`, `.cs` or text files for targeted review.
- The agent returns suggestions and code without modifying project files automatically.

### Installation

Copy `Assets/Editor/AIAssistantWindow.cs` and `Assets/UniversalCodingAgent/` into a Unity project and wait for compilation. Open it from `Tools > Universal Coding Agent`.

### Configuration

Choose a Provider and Model in Settings, then enter the API key. Keys are stored only in local Unity EditorPrefs and are not written to project files. Ollama defaults to `http://localhost:11434/v1/chat/completions`.

### API compatibility

Except for the native Claude endpoint, presets use the OpenAI Chat Completions shape: `model`, `messages`, `temperature`, `stream:false`, and `choices[0].message.content`. Use a compatible gateway or Custom endpoint for other protocols.

### Security and limitations

Never commit API keys. This plugin is a proxy window; it does not execute shell commands, edit files or commit code automatically. Attach only relevant files when possible to keep requests within model context limits.
