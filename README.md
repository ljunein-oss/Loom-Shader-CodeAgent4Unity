# Shader-CodeAgent4Unity

**1.3 Quill** · Unity **2022+** · MIT License

在 Unity Editor 内使用大模型编写 Shader、分析 C# 错误，并通过人工审阅、文件快照和回滚应用修改。

## 环境要求

- Unity 2022 或更新版本，推荐 Unity 2022.3 LTS。
- 可访问模型服务的网络，以及对应服务商的 API Key；使用 Ollama 时需自行启动本地服务。
- 插件仅在 Editor 运行，不进入游戏运行时。

## 功能

| 功能 | 使用方式 |
| --- | --- |
| Shader / C# 对话 | 描述需求，附加相关文件，获取完整代码 |
| Unity 上下文 | 可选发送当前场景、渲染管线、选中材质和近期错误 |
| 文件审阅 | 将最新回复的第一个代码块送入独立审阅窗口 |
| 文件写入 | 确认目标路径与内容后写入 Assets 内文件 |
| 持久快照 | 写入前保存原文件及其 .meta |
| 回滚 | 恢复已有文件；撤销新建文件及 .meta |
| 自定义接口 | 配置 Chat Completions 兼容地址与模型 ID |

## 安装

1. 下载仓库 ZIP 或克隆仓库。
2. 将 Assets/Editor/AIAssistantWindow.cs 和 Assets/Editor/QuillRollback.cs **一起**复制到工程 Assets/Editor。
3. 等待 Unity 编译，打开 **Tools > Shader-CodeAgent4Unity > Open**。
4. 升级旧版本时覆盖对应文件，避免在其他目录保留同名类的副本。

当前采用源码安装方式，不是 Unity Package Manager 包。

## 模型配置

打开 Settings，选择 Provider，填写 Endpoint、Model 和 API Key。Model 字段始终可以手动编辑，应填写服务商实际接受的 API ID。

| 服务 | 配置 |
| --- | --- |
| DeepSeek | 预设 deepseek-V41-flash、deepseek-v4-pro；地址 https://api.deepseek.com/chat/completions |
| GLM | 预设 glm-5.3；地址 https://open.bigmodel.cn/api/paas/v4/chat/completions |
| OpenAI | 使用支持 Chat Completions 的模型 ID；地址 https://api.openai.com/v1/chat/completions |
| Claude | 原生 Messages 接口；预设 claude-fable-5-1、claude-opus-5-5、claude-sonnet-5-5 |
| Ollama | 默认 http://localhost:11434/v1/chat/completions，填写已安装的模型 ID |
| Custom | 自行填写完整 Chat Completions endpoint、模型 ID 和 Key |

Claude 使用 Messages；其他预设使用 Chat Completions，不包含 Responses/Codex 专用协议适配。供应商是否授权某个模型，以账号和接口实际响应为准。Gemini 和 Qwen 不提供内置预设。

Detect endpoint compatibility 会发送一次小型付费测试请求，并在对话中显示结果。API Key 保存在本机 Unity EditorPrefs；可通过 **Forget saved API key** 删除插件保存的值。

## 写一个 Shader

1. 打开插件，配置模型。
2. 可用 Attach file 附加现有 Shader；需要时启用渲染管线与场景上下文。
3. 示例需求：为 Unity 2022.3 Built-in 管线编写一个青色 Unlit Shader，返回单个完整 ShaderLab 代码块。
4. 点击 Send，检查回复，然后点击 **Review latest generated code**。
5. 在 **Absolute Assets path** 填写完整路径，例如 D:/MyProject/Assets/Shaders/Cyan.shader。
6. 审阅全部代码，点击 **Apply with snapshot** 并确认。
7. 等待 Unity 编译，在 Console 检查错误。创建材质并绑定 Shader 后，在自己的场景验证显示效果。

URP/HDRP 请求应明确 Unity、渲染管线版本及目标平台。Shader 能导入不等于所有管线和 GPU 上渲染正确。

## 修复 C# 错误

使用 Attach file 附加相关脚本，点击 Grab errors 收集错误，描述复现步骤。要求模型返回完整单文件代码，再在审阅窗口指定原脚本路径。当前只提取最新回复中的第一个代码块；多个文件应分别审阅和应用。

## 回滚

打开 **Tools > Shader-CodeAgent4Unity > File review and rollback**，点击 **Rollback latest** 并确认。

- 每次写文件前保存快照，包括已有文件和 .meta 的原始字节。
- 恢复最新尚未撤销的快照；新建文件及其 .meta 会被删除。
- 快照保存在当前工程 Library/QuillSnapshots，编辑器重启后文件仍在；删除 Library 会丢失快照。
- 回滚会覆盖目标文件后续的人工修改；它不是三方合并或完整工程备份。
- 新建目录可能保留；场景和外部文件操作不在这套回滚范围内。


## 测试记录

在独立 Unity 2022.3.17f1c1 工程执行：

- DeepSeek Flash 真实 API 生成 Shader，Unity 导入检查通过。
- 修改后的文件及 .meta 回滚，逐字节比对通过。
- 新建文件及 .meta 回滚删除通过。
- 插件 C# 编译检查通过。

尚无窗口录屏 GIF；未完成所有渲染管线、当前用户场景及所有 Provider 的端到端测试。

## 注意事项

- 当前是人工审阅后写单文件的助手，尚未实现自主多步执行、Shell、场景或系统环境修改（以后会弄？）。
- 仅允许 Assets 内目标文件，不支持链接目录/文件；生成代码仍需人工审阅（咱就是说也别太懒）。
- 发送的上下文及附加文件会传给所选服务商，API 调用按供应商规则计费。
- EditorPrefs 不是加密密钥保险库。不要提交 Key、个人配置、请求日志或私有工程内容。
- 建议使用 Git 备份工程，并先在测试工程试用。

## License

本项目采用 [MIT License](LICENSE)

## 1.2 Quill 更新

- 工具菜单和窗口名称统一为 Shader-CodeAgent4Unity。
- 新增生成代码审阅入口、Assets 文件写入、持久快照及回滚。
- 更新 DeepSeek 和 GLM 预设，允许手动填写模型 ID。
- 修复 Windows 路径格式检查、对话上下文位置和接口检测流程。
- 增加本机已保存 Key 的删除按钮。

## 1.3 Quill 更新

- 更新模型：OpenAI 的 gpt-6-astra、gpt-6.1-sol、gpt-6-luna；Claude 的 claude-fable-5-1、claude-opus-5-5、claude-sonnet-5-5。
- 恢复可选择的模型预设，修复下拉框首项选择问题；支持从供应商刷新账号可用模型列表。


- 增加 Claude 原生 Messages 请求、鉴权和文本响应解析。
- Unity 2022.3 编译验证通过。OpenAI/Claude 未使用付费 Key 完成端到端调用验证；截图确认的是模型 ID，不代表所有模型支持当前 Chat Completions 接口。Responses 适配仍未实现。

# ENG

# Shader-CodeAgent4Unity

**1.3 Quill** · Unity **2022+** · MIT License

Write Shaders, analyze C# errors, and apply changes with LLMs inside the Unity Editor — with human review, file snapshots, and rollback.

## Requirements

- Unity 2022 or newer; Unity 2022.3 LTS recommended.
- Network access to your model provider, plus the corresponding API key. For Ollama, start the local service yourself.
- The plugin runs in the Editor only; it is not included in game runtime.

## Features

| Feature | How to use |
| --- | --- |
| Shader / C# chat | Describe what you need, attach relevant files, get complete code |
| Unity context | Optionally send the current scene, render pipeline, selected materials, and recent errors |
| File review | Send the first code block of the latest reply to a dedicated review window |
| File write | Write to files under Assets after confirming the target path and content |
| Persistent snapshots | Save the original file and its .meta before writing |
| Rollback | Restore existing files; undo newly created files and their .meta |
| Custom endpoint | Configure a Chat Completions–compatible URL and model ID |

## Installation

1. Download the repository ZIP or clone the repository.
2. Copy **both** `Assets/Editor/AIAssistantWindow.cs` and `Assets/Editor/QuillRollback.cs` into your project's `Assets/Editor`.
3. Wait for Unity to compile, then open **Tools > Shader-CodeAgent4Unity > Open**.
4. When upgrading from an older version, overwrite the corresponding files and avoid keeping copies of the same class in other directories.

This is currently a source-based install, not a Unity Package Manager package.

## Model configuration

Open Settings, choose a Provider, and fill in Endpoint, Model, and API Key. The Model field is always editable and should contain the actual API ID accepted by the provider.

| Service | Configuration |
| --- | --- |
| DeepSeek | Presets `deepseek-V41-flash`, `deepseek-v4-pro`; endpoint https://api.deepseek.com/chat/completions |
| GLM | Preset `glm-5.3`; endpoint https://open.bigmodel.cn/api/paas/v4/chat/completions |
| OpenAI | Use a model ID that supports Chat Completions; endpoint https://api.openai.com/v1/chat/completions |
| Claude | Native Messages API; presets `claude-fable-5-1`, `claude-opus-5-5`, `claude-sonnet-5-5` |
| Ollama | Defaults to http://localhost:11434/v1/chat/completions; enter an installed model ID |
| Custom | Provide the full Chat Completions endpoint, model ID, and key yourself |

Claude uses Messages; the other presets use Chat Completions and do not include Responses/Codex-specific protocol support. Whether a provider authorizes a given model depends on your account and the actual API response. Gemini and Qwen have no built-in presets.

**Detect endpoint compatibility** sends one small paid test request and shows the result in the chat. The API key is stored in the local Unity EditorPrefs; you can delete the value saved by the plugin via **Forget saved API key**.

## Writing a Shader

1. Open the plugin and configure a model.
2. Optionally use **Attach file** to attach an existing Shader; enable render pipeline and scene context if needed.
3. Example request: *Write a cyan Unlit Shader for the Unity 2022.3 Built-in pipeline, returning a single complete ShaderLab code block.*
4. Click **Send**, review the reply, then click **Review latest generated code**.
5. In **Absolute Assets path**, enter the full path, e.g. `D:/MyProject/Assets/Shaders/Cyan.shader`.
6. Review all the code, click **Apply with snapshot**, and confirm.
7. Wait for Unity to compile and check the Console for errors. Create a material, assign the Shader, and verify it in your own scene.

URP/HDRP requests should explicitly state the Unity version, render pipeline version, and target platform. A Shader that imports successfully is not guaranteed to render correctly on every pipeline and GPU.

## Fixing C# errors

Use **Attach file** to attach the relevant scripts, click **Grab errors** to collect errors, and describe the reproduction steps. Ask the model to return complete single-file code, then specify the original script path in the review window. Currently only the first code block of the latest reply is extracted; review and apply multiple files separately.

## Rollback

Open **Tools > Shader-CodeAgent4Unity > File review and rollback**, click **Rollback latest**, and confirm.

- A snapshot is saved before every file write, including the original bytes of existing files and their .meta.
- Restores the most recent snapshot that has not yet been undone; newly created files and their .meta are deleted.
- Snapshots are stored in the current project's `Library/QuillSnapshots` and survive an editor restart; deleting Library loses the snapshots.
- Rollback overwrites any manual changes made to the target file afterward; it is not a three-way merge or a full project backup.
- Newly created directories may remain; scene and external file operations are outside the scope of this rollback.

## Test log

Executed in a standalone Unity 2022.3.17f1c1 project:

- Shader generated via the real DeepSeek Flash API; Unity import check passed.
- Rollback of a modified file and its .meta, byte-for-byte comparison passed.
- Rollback and deletion of a newly created file and its .meta passed.
- Plugin C# compile check passed.

No window recording GIF yet; end-to-end tests across all render pipelines, the current user's scene, and all providers are not complete.

## Notes

- This is currently a human-reviewed, single-file writing assistant. Autonomous multi-step execution, Shell, and scene or system-level modifications are not implemented yet (maybe later?).
- Only target files under Assets are allowed; linked directories/files are not supported. Generated code still requires human review (come on, don't be that lazy).
- Sent context and attached files are transmitted to the selected provider; API calls are billed according to the provider's rules.
- EditorPrefs is not an encrypted key vault. Do not commit keys, personal configuration, request logs, or private project content.
- Back up your project with Git, and try it in a test project first.

## License

This project is licensed under the [MIT License](LICENSE).

## 1.2 Quill update

- Unified the tool menu and window name to Shader-CodeAgent4Unity.
- Added a generated-code review entry point, Assets file writing, persistent snapshots, and rollback.
- Updated the DeepSeek and GLM presets and allowed manual model ID entry.
- Fixed Windows path format checks, chat context position, and the endpoint detection flow.
- Added a button to delete the locally saved key.

## 1.3 Quill update

- Updated models: OpenAI's `gpt-6-astra`, `gpt-6.1-sol`, `gpt-6-luna`; Claude's `claude-fable-5-1`, `claude-opus-5-5`, `claude-sonnet-5-5`.
- Restored selectable model presets and fixed the dropdown first-item selection issue; added support for refreshing the account's available model list from the provider.
