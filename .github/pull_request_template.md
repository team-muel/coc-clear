# T<N> — <title>

## Verification
- [ ] `scripts/gate.ps1` → **GATE PASS**, EditMode `___ / ___` (paste output)
- [ ] Windows build `Errors=0` (if the change touches runtime code)
- [ ] Determinism: same seed twice → identical hash (if a generator changed)

## Contract
- [ ] `docs/tasks/T<N>.md` exists on `main` and Non-scope is respected
- [ ] Scene / `.meta` / render-pipeline assets touched: **no** / yes → why: ___
- [ ] No `UnityEngine.Random` in code
- [ ] `CocClear.Core` still has `noEngineReferences: true`
- [ ] New assets recorded in `docs/licenses.md`

## Hygiene
- [ ] Only my own files staged
- [ ] Worktree removed, branch deleted after merge

## Review
- [ ] Approved by the other person (no self-approve)

## New trap?
If something bit you that wasn't in the playbook, append it to the vault §H and link it here.
