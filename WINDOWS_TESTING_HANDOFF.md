# Windows Testing Handoff - Phase 1.5 Volume Support

**Date:** February 26, 2026  
**Branch:** `dev`  
**Commit:** `c66d61e` - feat: Phase 1.5 - Add port mapping and persistent volume support  
**Status:** Linux testing complete ✅ | Windows testing pending ⏳

---

## 🎯 What Was Completed on Linux

### Implementation (Ubuntu 22.04 + Docker 28.2.2)
- ✅ Port mapping with `-p` flags (tested: 8080:80, 8443:443)
- ✅ Exposed ports with `--expose` (tested: 9090)
- ✅ Named volume creation and management
- ✅ Volume mounting to containers
- ✅ Volume persistence across container lifecycle
- ✅ Blueprint integration for volumes, bind mounts, tmpfs
- ✅ Volume CLI commands (list, create, delete, inspect)

### Code Changes
**New Files:**
- `thresh/Thresh/Models/VolumeInfo.cs` - Volume info model
- `thresh/Thresh/blueprints/webserver-nginx.json` - Port mapping example
- `thresh/Thresh/blueprints/postgres-dev.json` - Volume example
- `docs/thresh-volume-flow.md` - Implementation walkthrough
- `docs/user-journey-storage.md` - User guide (11KB)
- `docs/json-blueprint-creation.md` - JSON syntax guide (6KB)

**Modified Files:**
- `thresh/Thresh/Program.cs` - Added `AddVolumeCommand()` with 4 subcommands
- `thresh/Thresh/Services/IContainerService.cs` - Added volume lifecycle methods
- `thresh/Thresh/Services/ContainerdService.cs` - Implemented volume management
- `thresh/Thresh/Services/WslService.cs` - Added volume stubs (to be fully implemented)
- `thresh/Thresh/Services/ContainerdJsonContext.cs` - Added AOT serialization
- `PHASE_1.5_TESTING.md` - Linux test results
- `docs/ROADMAP_2026.md` - Phase 1.5 progress
- `.gitignore` - Excluded packer credentials

---

## 🚀 What Needs Testing on Windows

### Environment Setup
1. **Pull latest dev branch:**
   ```powershell
   cd C:\code\thresh
   git checkout dev
   git pull origin dev
   ```

2. **Verify WSL2 is running:**
   ```powershell
   wsl --list --verbose
   # Should show at least one distro in "Running" state
   ```

3. **Build thresh for Windows:**
   ```powershell
   cd thresh\Thresh
   dotnet publish -c Release -r win-x64 -o ..\..\build-output\win-x64
   ```

### Test Plan

#### Test 1: Port Mapping on WSL2
**Blueprint:** `webserver-nginx.json`
```json
{
  "name": "webserver-nginx",
  "base": "ubuntu-22.04",
  "ports": ["8080:80", "8443:443"],
  "expose": [9090],
  "packages": ["nginx"]
}
```

**Commands:**
```powershell
# Navigate to build output
cd C:\code\thresh\build-output\win-x64

# Provision with port mapping
.\thresh.exe up webserver-nginx

# Verify environment created
.\thresh.exe list

# Test ports from Windows host
curl http://localhost:8080
# Should connect (may see error if nginx not configured, but port should be accessible)
```

**Expected Results:**
- ✅ Environment provisions successfully
- ✅ Ports appear in `thresh list` output
- ✅ Windows can access localhost:8080 and localhost:8443 (WSL2 auto-forwards to Windows)
- ✅ Port 9090 is exposed but not accessible from Windows (container-only)

**Known Issues to Check:**
- Any port conflicts with existing services?

---

#### Test 2: Volume Management Commands
**Commands:**
```powershell
cd C:\code\thresh\build-output\win-x64

# Create a test volume
.\thresh.exe volume create test-data

# List volumes
.\thresh.exe volume list

# Inspect volume
.\thresh.exe volume inspect test-data

# Delete volume
.\thresh.exe volume delete test-data
```

**Expected Results:**
- ✅ `volume create` creates volume successfully
- ✅ `volume list` shows volume name, driver, mountpoint
- ✅ `volume inspect` shows detailed JSON information
- ✅ `volume delete` removes volume
- ✅ All operations work with WSL distro

**Known Issues to Check:**
- Does WSL volume path show correctly? (e.g., `\\wsl$\distro\...`)
- Are volume operations calling WSL correctly?
- Any permission issues?

---

#### Test 3: Blueprint with Persistent Volume
**Blueprint:** `postgres-dev.json`
```json
{
  "name": "postgres-dev",
  "base": "ubuntu-22.04",
  "ports": ["5432:5432"],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    }
  ],
  "packages": ["postgresql"]
}
```

**Commands:**
```powershell
cd C:\code\thresh\build-output\win-x64

# Provision with volume
.\thresh.exe up postgres-dev

# Verify volume created
.\thresh.exe volume list
# Should show: postgres-data

# Start the environment
.\thresh.exe start postgres-dev

# Write test data to volume
wsl -d thresh-postgres-dev sh -c "echo 'test data' > /var/lib/postgresql/data/test.txt"

# Read data back
wsl -d thresh-postgres-dev cat /var/lib/postgresql/data/test.txt
# Should output: test data

# Stop environment
.\thresh.exe stop postgres-dev

# Destroy container
.\thresh.exe destroy postgres-dev
# Answer 'y' to confirmation

# Verify volume STILL exists
.\thresh.exe volume list
# Should still show: postgres-data

# Recreate environment
.\thresh.exe up postgres-dev

# Start it
.\thresh.exe start postgres-dev

# Verify data persisted
wsl -d thresh-postgres-dev cat /var/lib/postgresql/data/test.txt
# Should STILL output: test data ✅ PERSISTENCE WORKS!
```

**Expected Results:**
- ✅ Volume created automatically with environment
- ✅ Volume mounted to container at correct path
- ✅ Data written to volume is accessible
- ✅ Volume survives container destruction
- ✅ Data persists when container is recreated
- ✅ Volume only deleted when explicitly commanded

**Known Issues to Check:**
- Does volume persistence work across WSL restarts?
- Are volume paths accessible from both Windows and WSL?
- Any permission issues writing to volumes?

---

#### Test 4: Bind Mount (Future Feature)
**Blueprint:** Create `test-bind-mount.json` in `build-output\win-x64\blueprints\`:
```json
{
  "name": "test-bind-mount",
  "base": "alpine",
  "bind_mounts": [
    {
      "host": "C:\\code\\thresh",
      "container": "/workspace",
      "readonly": false
    }
  ]
}
```

**Commands:**
```powershell
# Provision with bind mount
.\thresh.exe up test-bind-mount

# Start environment
.\thresh.exe start test-bind-mount

# List files in mounted directory
wsl -d thresh-test-bind-mount ls /workspace
# Should show thresh project files
```

**Expected Results:**
- ✅ Windows path converted to WSL path correctly
- ✅ Host directory accessible in container
- ✅ Files readable/writable based on readonly setting

**Known Issues to Check:**
- Windows path format (C:\\ vs /mnt/c/)
- WSL path translation working?
- Performance of bind mounts from Windows to WSL?

---

## 🐛 Potential Issues & Debugging

### Issue 1: Volume Commands Not Working
**Symptom:** `thresh volume list` shows error or no volumes

**Debug Steps:**
```powershell
# Check if WSL distro exists
wsl --list --verbose

# Try manual docker command in WSL
wsl docker volume ls

# Check if docker is running in WSL
wsl docker ps
```

**Fix:** WslService may need implementation updates for volume management

---

### Issue 2: Port Forwarding Not Working
**Symptom:** `curl localhost:8080` fails from Windows

**Debug Steps:**
```powershell
# Test from within WSL first
wsl curl localhost:8080

# Check if service is actually running
wsl docker ps
```

**Fix:** Likely a service configuration issue, not port forwarding (WSL2 auto-forwards ports)

---

### Issue 3: Permission Errors
**Symptom:** "Access denied" when creating volumes

**Debug Steps:**
```powershell
# Run as Administrator
# Check WSL user permissions
wsl whoami
wsl docker info
```

**Fix:** May need elevated permissions or docker group membership in WSL

---

## 📊 Test Results Template

Copy this to `PHASE_1.5_TESTING.md` after testing:

```markdown
## Windows WSL2 Testing (Feb 26, 2026)

**Environment:**
- OS: Windows 11 [version]
- WSL Version: [2.x.x]
- Distro: [Ubuntu/Debian]
- Docker: [version in WSL]
- .NET: [version]

**Test Results:**

### Port Mapping ✅/❌
- [ ] Port mapping creates successfully
- [ ] Ports accessible from Windows host (WSL2 auto-forwards)
- [ ] Multiple ports work simultaneously
- [ ] Exposed ports not accessible from host

### Volume Management ✅/❌
- [ ] volume create works
- [ ] volume list shows volumes
- [ ] volume inspect shows details
- [ ] volume delete removes volumes
- [ ] WSL paths displayed correctly

### Blueprint Integration ✅/❌
- [ ] Volumes auto-created from blueprint
- [ ] Volumes mounted to correct paths
- [ ] Data writable to volumes
- [ ] Data persists after destroy
- [ ] Volume reattaches on recreate

### Bind Mounts ✅/❌
- [ ] Windows paths translated to WSL
- [ ] Host directory accessible
- [ ] Files readable in container
- [ ] Files writable (if not readonly)

**Issues Found:**
1. [Describe any issues]
2. [Include error messages]
3. [Note any workarounds]

**Success Rate:** X/Y tests passed
```

---

## 📝 Next Steps After Windows Testing

1. **If all tests pass:**
   - Update PHASE_1.5_TESTING.md with Windows results
   - Mark Phase 1.5 as complete in ROADMAP_2026.md
   - Update version to v1.5.0-rc1
   - Create release candidate

2. **If issues found:**
   - Document issues in PHASE_1.5_TESTING.md
   - Create GitHub issues for bugs
   - Fix critical issues in WslService.cs
   - Retest on Windows

3. **Documentation updates:**
   - Add Windows-specific volume examples to docs
   - Update getting-started-windows.md with volume examples
   - Add troubleshooting section for common Windows issues
   - Create blog post about Phase 1.5 release

---

## 🔗 Useful References

- **Implementation Details:** `docs/thresh-volume-flow.md`
- **User Guide:** `docs/user-journey-storage.md`
- **JSON Syntax:** `docs/json-blueprint-creation.md`
- **Linux Testing Results:** `PHASE_1.5_TESTING.md`
- **Roadmap:** `docs/ROADMAP_2026.md`

---

## 💡 Key Implementation Notes

### Volume Management in WslService
The WslService currently has stub implementations for volume commands. They need to:
1. Translate volume operations to WSL docker commands
2. Handle Windows path to WSL path conversion
3. Manage WSL distro lifecycle

### No Rebuild Required for Blueprints
Users can edit blueprints directly in `build-output\win-x64\blueprints\` without rebuilding. Blueprints are loaded from the filesystem at runtime!

### Docker Group Membership
On Linux, users needed `newgrp docker` to avoid sudo. On Windows, this shouldn't be necessary as WSL handles permissions differently.

---

## ✅ Linux Testing Summary (Reference)

**Platform:** Ubuntu 22.04 LTS  
**Container Runtime:** Docker 28.2.2  
**Binary Size:** 13MB (Native AOT, linux-x64)

**All Tests Passed:**
- ✅ Port mapping: 8080:80, 8443:443 working
- ✅ Exposed ports: 9090 working (container-only)
- ✅ Volume creation: test-data, postgres-data, app-cache
- ✅ Volume mounting: Correctly mounted to containers
- ✅ Volume persistence: Data survived destroy/recreate cycle
- ✅ Volume commands: All 4 commands working (list, create, delete, inspect)
- ✅ Blueprint integration: postgres-dev working perfectly
- ✅ No sudo required: After `newgrp docker`

**Tested Blueprints:**
1. `webserver-nginx.json` - Port mapping (8080:80, 8443:443, expose 9090)
2. `postgres-dev.json` - Persistent volume (postgres-data → /var/lib/postgresql/data)
3. `my-app.json` - Created on-the-fly (app-data → /data)

---

**Good luck with Windows testing! 🚀**

Questions? Check the docs or review the Linux testing session for reference.
