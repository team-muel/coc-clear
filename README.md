# CoC-Clear

Unity game project. Two people. Unity **6000.3.19f1**.

> **Design lives in the Obsidian vault, not here.** The vault zip (`CoC-Clear` folder) is the design source of truth: pitfalls playbook, process, collaboration protocol, Codex onboarding. Read `65 Engineering Pitfalls Playbook.md` before your first PR — every entry in it was paid for.

## Quick start

```powershell
pwsh -NoProfile -File scripts\launch.ps1   # open the editor (with the right env)
pwsh -NoProfile -File scripts\gate.ps1     # the merge gate (close Unity first)
```

`gate.ps1` exit 0 = every EditMode test passed. Nothing merges without it.

## Layout

| Path | What |
|---|---|
| `Assets/CocClear/Core/` | engine-free C#. `noEngineReferences: true` — `UnityEngine.Random` is unreachable here, by construction. |
| `Assets/CocClear/Runtime/` | MonoBehaviours, engine-facing code |
| `Assets/CocClear/Editor/` | editor-only tools (`includePlatforms: ["Editor"]`) |
| `Assets/CocClear/Tests/EditMode/` | the gate |
| `Assets/CocClear/Data/Source/` | authoring xlsx |
| `Assets/CocClear/Data/Generated/` | validated CSV (committed) |
| `tools/DataSchema/Records/` | the data schema, as C# records → [`tools/data/README.md`](tools/data/README.md) |
| `docs/tasks/` | task briefs. One per task. Committed **before** work starts. |
| `docs/licenses.md` | every third-party asset, or it doesn't ship |

## The rules that get PRs rejected

See [`docs/CONVENTIONS.md`](docs/CONVENTIONS.md). The short version:

1. Gate green, all of it. `n/n`.
2. No engine RNG **in code**. Seed from `CocClear.Core.DeterministicRandom`.
3. `Core` never references `UnityEngine`.
4. Scenes, `.meta`, and render-pipeline assets are owned by one person.
5. Unlisted assets don't get committed. `docs/licenses.md` or nothing.
6. Push before you close the day. **Work that isn't on `origin` doesn't exist.**
