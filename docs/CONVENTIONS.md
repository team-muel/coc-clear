# Conventions

Enforceable at review. If a PR violates one of these, it is reverted, not discussed.

## 1. The gate

- `pwsh -NoProfile -File scripts/gate.ps1` → **GATE PASS**, with the count (`n/n`) pasted into the PR.
- Close the Unity editor first — single project lock.
- Never add `-quit` to `-runTests`. The editor exits before the runner finishes and reports a success with zero tests run.
- Windows build: `Errors=0`.

## 2. Determinism

- Procedural and simulation code seeds from `CocClear.Core.DeterministicRandom`.
- **No `UnityEngine.Random` in code.** (Comments and docs are not code. This rule has been misread by a worker before and turned into `// No engine RNG` sed-vandalism across a repo.)
- `CocClear.Core` has `noEngineReferences: true`. Keep it that way.
- Any generator ships with a test: same seed twice → identical hash.

## 3. Assemblies

| asmdef | May reference |
|---|---|
| `CocClear.Core` | nothing. no engine. |
| `CocClear.Runtime` | Core + UnityEngine |
| `CocClear.Editor` | Core + Runtime, `includePlatforms: ["Editor"]` |
| `CocClear.Tests.EditMode` | Core + Runtime + TestRunner |

Third-party packs that ship Editor scripts inside a runtime asmdef **break the player build**. Give them their own Editor asmdef before committing.

## 4. Ownership

- **Scenes, `.meta`, render-pipeline assets: one owner.** Merge conflicts in Unity YAML are not resolvable in practice.
- Stage only your own files. Never `git add -A`.
- Generated CSV under `Data/Generated/` is committed — regenerate, don't hand-edit.

## 5. Tasks

- One task, one branch: `task/T<N>-<slug>`.
- Brief at `docs/tasks/T<N>.md`, **committed to `main` before work starts**, and it must include a **Non-scope** section.
- Parallel work uses `git worktree` (separate projectPath → each worktree can run its own gate).
- Merge = squash. Cleanup (`git worktree remove` + branch delete) is part of the task.

## 6. Review

- One approval from the other person. No self-approve. In a two-person team, review is the only outside eye this code will ever get.

## 7. Assets

- `docs/licenses.md` records source, license, modifications, and gotchas — before the asset is committed.
- CC0 (Poly Haven, ambientCG, Kenney, Quaternius) is fine. Anything with a per-asset license gets checked first. Fonts under OFL are **not** CC0 — ship the LICENSE file.
- Nobody, human or agent, types credentials into a signup form on someone else's behalf.

## 8. Scenes and the build

- Work that exists only in an editor preview does not exist. Save it into a build scene and rebuild.
- Values tuned in Play mode are lost. Write them to the script default **and** the scene instance.
- Strip runtime-generated objects (`_Generated*`) before saving a scene.
- Edit scenes in chunks, saving each chunk. A single script that instantiates hundreds of prefabs will take the editor down with it.

## 9. Honesty of the record

- "I committed it" and "it is on `origin`" are different sentences. Verify with `git rev-parse origin/main` before writing a hash anywhere.
- New trap? Append it to the vault playbook (§H) and link it from the PR. That document is the asset.
