# Fix Boot Manager Issue - Empty VM Created

## Problem
The VM is sitting at boot manager because Pulumi created an empty VM with no operating system installed.

## Solution: Delete Empty VM and Import Ubuntu Template Properly

### Step 1: Delete the Empty VM

1. Open ESXi web interface: https://192.168.4.205
2. Login with root / RangeR88**
3. Find the VM named "ubuntu-22.04-cloud-init" (or similar)
4. If it's powered on:
   - Right-click → Power → Power Off
5. Delete it:
   - Right-click → Delete from disk

### Step 2: Import Ubuntu Cloud Image as Template

Run the import script from the `pulumi` directory:

```bash
cd /c/Users/burns/source/repos/thresh/pulumi
chmod +x import-ubuntu-template.sh
./import-ubuntu-template.sh
```

This script will:
1. Download Ubuntu 22.04 Cloud Image OVA (~650MB)
2. Import it to ESXi using govc
3. Mark it as a template
4. Template will be ready for cloning

### Step 3: Deploy Test VM with Pulumi

Once the template is imported, run Pulumi to create the test VM:

```bash
cd /c/Users/burns/source/repos/thresh/pulumi
../pulumi-cli/pulumi/bin/pulumi.exe up
```

This will:
1. Clone the template
2. Configure cloud-init with Docker, .NET 10, thresh repo
3. Create a powered-on VM ready for testing
4. VM will have SSH access with your key

### Step 4: Connect to Test VM

After deployment completes (2-3 minutes for cloud-init):

```bash
# Get VM IP from Pulumi output or ESXi console
ssh thresh@<vm-ip>

# Verify installation
docker --version
dotnet --version
ls ~/thresh  # Should show cloned repository
```

## Alternative: Manual Import (if script fails)

If the import script doesn't work, you can manually import:

1. Download Ubuntu Cloud Image:
   ```bash
   cd /c/Users/burns/source/repos/thresh/pulumi
   curl -L -o ubuntu-22.04-server-cloudimg-amd64.ova \
        https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova
   ```

2. Import via govc:
   ```bash
   export GOVC_URL=192.168.4.205
   export GOVC_USERNAME=root
   export GOVC_PASSWORD='RangeR88**'
   export GOVC_INSECURE=true
   
   ../govc.exe import.ova \
       -name="ubuntu-22.04-cloud-init" \
       -ds="nvme2" \
       -pool="Resources" \
       -net="VM Network" \
       ubuntu-22.04-server-cloudimg-amd64.ova
   
   ../govc.exe vm.markastemplate "ubuntu-22.04-cloud-init"
   ```

3. Then run Pulumi as in Step 3 above

## What Changed
- `.env`: SET `CREATE_TEMPLATE=false` (we use the import script instead)
- Created: `import-ubuntu-template.sh` (automated template import)
- Next: Pulumi will clone the template instead of creating empty VMs
