# Universal Coding Agent for Unity

A Unity Editor proxy window for shader authoring and bug fixing. It sends prompts plus optional Unity context to any OpenAI Chat Completions compatible API.

## Supported presets

- Codex / OpenAI
- DeepSeek
- GLM (智谱)
- Ollama (local)
- Custom endpoint and model

Open **Tools → Universal Coding Agent**. Configure the provider, endpoint, model and API key. Keys are stored in Unity `EditorPrefs` and are never written into the project files.

Attach `.shader`, `.compute`, `.hlsl`, `.shadergraph`, `.cs` or text files for targeted review. The window can include the active scene, render pipeline, selected materials/shaders and recent Console errors.

## API compatibility

The adapter sends `POST` JSON with `model`, `messages`, `temperature` and `stream:false`, and reads `choices[0].message.content`. This matches OpenAI-compatible gateways exposed by the listed providers. Providers that use a different protocol can be used through a compatible gateway or a custom adapter in a future version.

## Security

Do not commit API keys. For team use, prefer environment-backed gateways or a local secret manager. This plugin only acts as a proxy window; it does not edit files automatically.
