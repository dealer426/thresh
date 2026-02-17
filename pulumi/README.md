# Thresh Pulumi Infrastructure

This directory contains Pulumi infrastructure-as-code for provisioning test VMs in vCenter for thresh cross-platform development and testing.

## Prerequisites

1. **Pulumi CLI** - Install from https://www.pulumi.com/docs/install/
2. **.NET 10 SDK** - Already installed for thresh development
3. **vCenter Access** - Credentials and network access to your vCenter server
4. **SSH Key** - Public key for VM access (default: `~/.ssh/id_rsa.pub`)

## Setup

### 1. Install Pulumi CLI

```powershell
# Windows (via Chocolatey)
choco install pulumi

# Or download from https://www.pulumi.com/docs/install/
```

### 2. Configure vCenter Credentials

```bash
cd pulumi

# Copy the example environment file
cp .env.example .env

# Edit .env with your vCenter details
# IMPORTANT: Never commit .env to git!
```

### 3. Update `.env` File

Fill in your actual vCenter configuration:

```env
VSPHERE_SERVER=your-vcenter.example.com
VSPHERE_USER=administrator@vsphere.local
VSPHERE_PASSWORD=your-actual-password

VSPHERE_DATACENTER=YourDatacenter
VSPHERE_CLUSTER=YourCluster
VSPHERE_DATASTORE=YourDatastore
VSPHERE_NETWORK=VM Network
VSPHERE_RESOURCE_POOL=Resources

UBUNTU_TEMPLATE=ubuntu-22.04-cloud-init
CREATE_TEMPLATE=false
SSH_PUBLIC_KEY_PATH=C:\Users\YourUser\.ssh\id_rsa.pub
```

### 4. Setup Ubuntu Cloud-Init Template

You need an Ubuntu 22.04 cloud-init enabled template in vCenter. **Two options:**

#### Option A: Download and Import Ubuntu Cloud Image (Recommended)

```bash
# 1. Download Ubuntu Cloud Image OVA
wget https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova

# 2. Import to vCenter via vSphere Client:
#    - Right-click Datacenter → Deploy OVF Template
#    - Select the downloaded OVA
#    - Name it: ubuntu-22.04-cloud-init
#    - Select your datastore and network
#    - Finish deployment

# 3. Convert to Template:
#    - Right-click the deployed VM → Template → Convert to Template

# Done! Template is ready for Pulumi
```

#### Option B: Use Pulumi to Guide Template Creation

```bash
# 1. Set CREATE_TEMPLATE=true in .env
echo "CREATE_TEMPLATE=true" >> .env

# 2. Run Pulumi
pulumi up

# 3. Follow the detailed instructions printed by Pulumi

# 4. After template is ready, set back to false
sed -i 's/CREATE_TEMPLATE=true/CREATE_TEMPLATE=false/' .env

# 5. Run Pulumi again with real template
pulumi up
```

#### Option C: Use govc CLI (Automated)

```bash
# 1. Install govc
# Download from: https://github.com/vmware/govmomi/releases

# 2. Download Ubuntu Cloud Image
wget https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova

# 3. Set govc environment
export GOVC_URL=https://your-vcenter.example.com/sdk
export GOVC_USERNAME=administrator@vsphere.local
export GOVC_PASSWORD=your-password
export GOVC_INSECURE=true

# 4. Import OVA and mark as template
govc import.ova \
  -name=ubuntu-22.04-cloud-init \
  -ds=YourDatastore \
  -pool=YourCluster/Resources \
  ubuntu-22.04-server-cloudimg-amd64.ova

govc vm.markastemplate ubuntu-22.04-cloud-init

# Done! Ready for Pulumi
```

### 5. Login to Pulumi

```bash
# Login to Pulumi (can use local backend or Pulumi Cloud)
pulumi login --local  # For local state storage

# Or use Pulumi Cloud (free for individuals)
pulumi login
```

### 6. Initialize Pulumi Stack

```bash
# Restore .NET dependencies
dotnet restore

# Initialize dev stack (already configured)
pulumi stack select dev

# Or create a new stack
pulumi stack init dev
```

## Usage

### Check Template Availability

```bash
# Verify your template exists before deploying
# This should be set in .env and match your vCenter template name
echo $UBUNTU_TEMPLATE
```

### Preview Changes

```bash
pulumi preview
```

### Deploy Infrastructure

```bash
# Deploy the Ubuntu test VM
pulumi up

# Review changes and confirm
```

### Get VM IP Address

```bash
# After deployment, get the VM IP
pulumi stack output vmIp

# Get SSH command
pulumi stack output sshCommand
```

### Connect to VM

```bash
# SSH into the Ubuntu test VM
ssh thresh@<vm-ip-address>

# The VM will have:
# - Docker and containerd installed
# - .NET 10 SDK installed
# - thresh repository cloned to ~/thresh
# - User 'thresh' with sudo access
```

### Destroy Infrastructure

```bash
# Remove all VMs (when testing is complete)
pulumi destroy

# Review resources to be deleted and confirm
```

## VM Configuration

### Ubuntu 22.04 Test VM

- **Name**: thresh-ubuntu-test
- **CPU**: 2 cores
- **RAM**: 4 GB
- **Disk**: 50 GB (thin provisioned)
- **OS**: Ubuntu 22.04 LTS
- **User**: thresh (with sudo access)
- **Pre-installed**:
  - Docker CE
  - containerd
  - .NET 10 SDK
  - Git, curl, wget, vim, htop
  - Build essentials

### Cloud-Init Setup

The VM uses cloud-init to automatically:
1. Create `thresh` user with your SSH key
2. Install Docker and containerd
3. Install .NET 10 SDK
4. Clone thresh repository
5. Configure Docker permissions

**Boot time**: 2-3 minutes for cloud-init to complete

## Testing Workflow

### 1. Deploy VM

```bash
cd pulumi
pulumi up
```

### 2. Wait for Cloud-Init

```bash
# Cloud-init takes 2-3 minutes
# Watch cloud-init logs:
ssh thresh@<vm-ip> sudo tail -f /var/log/cloud-init-output.log
```

### 3. Build thresh on Linux

```bash
ssh thresh@<vm-ip>
cd ~/thresh
dotnet build thresh/Thresh/Thresh.csproj
```

### 4. Test thresh Commands

```bash
cd thresh/Thresh/bin/Debug/net10.0
./thresh --version
./thresh up python-dev
./thresh list
./thresh destroy python-dev
```

### 5. Iterate and Debug

- Make changes on Windows dev machine
- Push to GitHub (dev branch)
- Pull changes on Ubuntu VM: `git pull`
- Rebuild and test

### 6. Cleanup When Done

```bash
# Back on Windows
cd pulumi
pulumi destroy
```

## Troubleshooting

### VM Not Accessible

```bash
# Check VM status in vCenter
pulumi stack output vmId

# Check if VM got an IP address
pulumi stack output vmIp
```

### Cloud-Init Issues

```bash
# SSH into VM and check cloud-init status
ssh thresh@<vm-ip> cloud-init status

# View cloud-init logs
ssh thresh@<vm-ip> sudo cat /var/log/cloud-init-output.log
```

### Template Not Found

Ensure your vCenter has an Ubuntu 22.04 template named correctly in `.env`:
- Template must exist in vCenter
- Template must have cloud-init installed
- Update `UBUNTU_TEMPLATE` in `.env` to match your template name

### SSH Key Issues

```bash
# Verify your SSH public key exists
cat ~/.ssh/id_rsa.pub

# If not, generate one:
ssh-keygen -t rsa -b 4096 -C "your_email@example.com"
```

## Security Notes

- ⚠️ **NEVER commit `.env` file** - Contains sensitive credentials
- ✅ `.env` is in `.gitignore` by default
- ✅ Use strong passwords for vCenter
- ✅ Restrict SSH key access
- ✅ Use Pulumi secrets for production environments

## Files

- `pulumi.csproj` - .NET project file with Pulumi dependencies
- `Program.cs` - Infrastructure code (VM definitions)
- `Pulumi.yaml` - Pulumi project configuration
- `Pulumi.dev.yaml` - Dev stack configuration
- `.env.example` - Template for credentials
- `.env` - **Your actual credentials (gitignored)**
- `README.md` - This file

## Next Steps

After successful Linux testing:
1. Add AlmaLinux VM for RHEL-like testing
2. Implement GitHub Actions multi-platform builds
3. Test macOS builds (GitHub hosted runners)
4. Keep VMs running for ongoing development
5. Update thresh documentation with Linux instructions

## Support

For issues with:
- **Pulumi**: https://www.pulumi.com/docs/
- **vSphere Provider**: https://www.pulumi.com/registry/packages/vsphere/
- **thresh**: https://github.com/dealer426/thresh
