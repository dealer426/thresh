# Branch Protection Setup Guide

This guide shows how to configure branch protection rules to enforce the dev → main workflow.

## GitHub Web Interface Setup

### 1. Navigate to Branch Protection Settings

1. Go to https://github.com/dealer426/thresh
2. Click **Settings** tab
3. Click **Branches** in left sidebar
4. Click **Add branch protection rule**

---

## Rule 1: Protect `main` Branch

**Branch name pattern:** `main`

**Settings to enable:**

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: 1
  - ✅ Dismiss stale pull request approvals when new commits are pushed
  
- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Search and add: `build-and-release` (if available from workflows)

- ✅ **Require conversation resolution before merging**

- ✅ **Do not allow bypassing the above settings**
  - ⚠️ **Include administrators** (recommended)

- ✅ **Restrict who can push to matching branches**
  - Add: Your username (dealer426)
  - This prevents direct commits - only PR merges allowed

**Click "Create" or "Save changes"**

---

## Rule 2: Protect `dev` Branch (Optional but Recommended)

**Branch name pattern:** `dev`

**Settings to enable:**

- ✅ **Require a pull request before merging**
  - Require approvals: 0 (or 1 if you want review for all changes)
  
- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging

- ✅ **Require conversation resolution before merging**

**Click "Create" or "Save changes"**

---

## Rule 3: Set Default Branch to `dev`

**This makes all PRs target `dev` by default:**

1. Go to https://github.com/dealer426/thresh/settings
2. Under **Default branch**, click the switch icon
3. Select `dev`
4. Click **Update**
5. Confirm the change

---

## Result

After setup:

✅ PRs automatically target `dev` by default  
✅ Direct pushes to `main` are blocked  
✅ Only you can merge to `main` (via PR from dev)  
✅ PRs require review before merging to `main`  
✅ Contributors must fork and PR to `dev`

---

## Quick Commands Reference

### For You (Maintainer)

```bash
# Regular development
git checkout dev
# ... make changes ...
git commit -m "feat: new feature"
git push origin dev

# Release to production
git checkout main
git merge dev
git push origin main
git tag v1.3.0
git push origin v1.3.0
```

### For Contributors

```bash
# Fork repo, then:
git checkout dev
git checkout -b feature/my-feature
# ... make changes ...
git push origin feature/my-feature
# Create PR targeting dev on GitHub
```

---

## Testing Branch Protection

After setup, try this to verify:

```bash
git checkout main
echo "test" >> test.txt
git add test.txt
git commit -m "test: direct commit to main"
git push origin main
```

**Expected result:** ❌ Push rejected by GitHub

---

## Notes

- Branch protection requires GitHub Pro for private repos (free for public repos like thresh)
- You can always temporarily disable protection if needed (Settings → Branches)
- Protection rules persist even if branches are deleted and recreated

---

## Have Questions?

See: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches
