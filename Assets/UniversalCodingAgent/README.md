# Shader-CodeAgent4Unity

**1.2 Quill** · Unity **2022+** · MIT License

在 Unity Editor 内使用大模型编写 Shader、分析 C# 错误，并通过人工审阅、文件快照和回滚应用修改。

## 环境要求

- Unity 2022 或更新版本，推荐 Unity 2022.3 LTS。
- 已验证环境：Windows、Unity 2022.3.17f1c1。其他 Unity 版本与操作系统尚未逐一测试。
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
| DeepSeek | 预设 deepseek-flash、deepseek-v4-pro；地址 https://api.deepseek.com/chat/completions |
| GLM | 预设 glm-5.3；地址 https://open.bigmodel.cn/api/paas/v4/chat/completions |
| OpenAI | 使用支持 Chat Completions 的模型 ID；地址 https://api.openai.com/v1/chat/completions |
| Claude | 当前通过 Custom 配置 Chat Completions 兼容网关；原生 Messages 适配尚未实现 |
| Ollama | 默认 http://localhost:11434/v1/chat/completions，填写已安装的模型 ID |
| Custom | 自行填写完整 Chat Completions endpoint、模型 ID 和 Key |

本版请求协议为 Chat Completions，不包含 Responses/Codex 专用协议适配。供应商是否授权某个模型，以账号和接口实际响应为准。Gemini 和 Qwen 不提供内置预设。

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

## 1.2 Quill 更新

- 工具菜单和窗口名称统一为 Shader-CodeAgent4Unity。
- 新增生成代码审阅入口、Assets 文件写入、持久快照及回滚。
- 移除 Gemini、Qwen 预设；旧 Provider 配置迁移到 DeepSeek，清除关联旧 Key。
- 更新 DeepSeek 和 GLM 预设，允许手动填写模型 ID。
- 修复 Windows 路径格式检查、对话上下文位置和接口检测流程。
- 增加本机已保存 Key 的删除按钮。

## 测试记录

在独立 Unity 2022.3.17f1c1 工程执行：

- DeepSeek Flash 真实 API 生成 Shader，Unity 导入检查通过。
- 修改后的文件及 .meta 回滚，逐字节比对通过。
- 新建文件及 .meta 回滚删除通过。
- 插件 C# 编译检查通过。

尚无窗口录屏 GIF；未完成所有渲染管线、当前用户场景及所有 Provider 的端到端测试。

## 注意事项

- 当前是人工审阅后写单文件的助手，尚未实现自主多步执行、Shell、场景或系统环境修改。
- 仅允许 Assets 内目标文件，不支持链接目录/文件；生成代码仍需人工审阅。
- 发送的上下文及附加文件会传给所选服务商，API 调用按供应商规则计费。
- EditorPrefs 不是加密密钥保险库。不要提交 Key、个人配置、请求日志或私有工程内容。
- 建议使用 Git 备份工程，并先在测试工程试用。

## License

本项目采用 [MIT License](LICENSE)。允许使用、修改和分发，包括商业用途；分发时保留版权及许可声明。软件按原样提供，不附带担保。第三方模型服务另受其服务条款约束。
