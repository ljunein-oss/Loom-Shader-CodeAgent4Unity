# Universal Coding Agent for Unity — v1.1 Loom

Unity Editor 内的通用 Shader / C# coding agent，支持 OpenAI/Codex、DeepSeek、GLM 以及任意 OpenAI Chat Completions 兼容接口。

## 1.1 Loom

- 支持模型生成结构化 `uca-actions` 计划，用于写入工程文件、运行 shell、刷新或保存场景。
- 所有动作执行前显示确认；Shell 命令必须单独确认。
- 执行前自动备份被覆盖文件到 `Library/UniversalCodingAgent/snapshots/<timestamp>`。
- `Rollback` 恢复最近一次快照。
- 路径限制在 Unity 工程根目录，API Key 仅保存在本机 EditorPrefs。

## 安装

复制 `Assets/Editor/AIAssistantWindow.cs` 到 Unity 工程，等待编译，在 `Tools > Universal Coding Agent` 打开。

## 配置模型

在 Settings 中填写 Provider、OpenAI 兼容 Endpoint、Model 和付费 API Key。Codex 使用 OpenAI API 兼容地址；DeepSeek 默认 `https://api.deepseek.com/chat/completions`；GLM 默认 `https://open.bigmodel.cn/api/paas/v4/chat/completions`。不要把 Key 写入 Git。

## 工作流

提出写 Shader、修编译错误或修改场景的请求。模型返回计划后，先检查摘要和会话内容，再点击 Apply plan。涉及 shell 会再次询问。需要撤销时点击 Rollback。

## 发布

本版本标签为 `v1.1-Loom`。
