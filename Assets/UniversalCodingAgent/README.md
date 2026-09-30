# Shader-CodeAgent4Unity — 1.2 Quill

Unity 2022.3 Editor Shader/C# assistant with reviewed file writes and persistent rollback.

## 更新内容
- 工具统一命名 Shader-CodeAgent4Unity。
- 移除 Gemini、Qwen 的可选入口和模型预设；旧配置迁移到 DeepSeek 并清除旧密钥。
- DeepSeek 预设 deepseek-flash（V4.1 Flash）、deepseek-v4-pro；GLM 预设 glm-5.3。
- 模型 ID 可编辑；不再把未经核实的 OpenAI/Claude 型号标为最新版。
- Review latest generated code 将回复的第一个代码块送入审阅窗口。
- 写文件前持久保存快照；回滚恢复原始字节及 meta，新建文件回滚时删除文件及 meta。
- 修复 Windows 路径检查、对话上下文索引和接口检测未完成的问题。

## 安装与使用
1. 将 Assets/Editor 中的两个 C# 文件一起复制到工程 Assets/Editor，等待编译。
2. 打开 Tools > Shader-CodeAgent4Unity > Open。
3. Settings 中选择服务商，填写 Endpoint、API Key 和准确的 API 模型 ID。
4. DeepSeek 默认地址为 https://api.deepseek.com/chat/completions 。可附加 Shader/C# 文件并选择场景、选中对象、Console 上下文。
5. 输入需求，要求模型返回完整文件和 fenced code block。发送会产生服务商费用。
6. 点击 Review latest generated code，检查全部代码，将 Absolute Assets path 改成目标文件的完整路径。
7. 点击 Apply with snapshot，确认后写入文件，等待 Unity 编译。
8. 需要撤销时打开 Tools > Shader-CodeAgent4Unity > File review and rollback，点击 Rollback latest。
9. 不再使用时点击 Forget saved API key。

## 验证与限制
Unity 2022.3.17f1c1 独立工程中实测：DeepSeek Flash 真实 API 生成 Shader；Shader 导入无错误；已有文件及 meta 逐字节恢复；新建文件及 meta 撤销。未完成当前 Sci_scene 场景渲染验收及窗口录屏 GIF，不提供虚构演示。

本版本是人工审阅后写单个文件的助手，尚无自动多步 agent、Shell 执行、场景编辑或环境修改。仅允许 Assets 内文件，不支持链接目录。回滚会覆盖后续人工编辑；快照位于 Library/QuillSnapshots，删除 Library 会丢失快照。它不是工程级版本控制，建议保留 Git 备份。

请求使用 Chat Completions 协议。OpenAI 仅限兼容该接口的模型，未实现 Responses/Codex 专用适配。Claude 原生 Messages 接口尚未实现，入口会提示使用 Custom 兼容网关。GLM 型号已核对文档但未做付费调用测试；Ollama 需手填本机模型名。官方模型权限与兼容性以服务商为准。

API Key 存在本机 EditorPrefs（并非加密保险库），不写入工程。上下文、附加文件会发送给所选服务商，发送前检查敏感内容。发布包不含测试密钥、用户场景或本机日志。已在聊天中暴露的测试密钥应在服务商后台撤销；本插件不能删除平台聊天记录。

模型参考：https://api-docs.deepseek.com/ 、https://docs.bigmodel.cn/cn/guide/models/text/glm-5.3 。OpenAI/Claude 最新公开型号因文档访问受限未核实。
