# thresh Pulumi + vCenter Setup Guide

Infrastructure-as-code for provisioning thresh test VMs in vCenter.

## Quick Start

### 1. Install Prerequisites

```powershell
# Install Pulumi CLI
choco install pulumi

# Verify installation
pulumi version
dotnet --version  # Should be 10.0+
```

### 2. Configure vCenter

Create `pulumi/.env` (copy from `.env.example`):

```env
# vCenter Connection
VSPHERE_SERVER=vcenter.thresh.sh
VSPHERE_USER=administrator@vsphere.local
VSPHERE_PASSWORD=your-password

# Infrastructure Details  
VSPHERE_DATACENTER=thresh
VSPHERE_CLUSTER=thresh-cluster
VSPHERE_DATASTORE=your-datastore-name      # ← Get this from vCenter
VSPHERE_NETWORK=VM Network                 # ← Get this from vCenter
VSPHERE_RESOURCE_POOL=Resources

# VM Configuration
VM_NAME=thresh-ubuntu-test
VM_CPUS=4
VM_MEMORY_MB=8192
VM_DISK_GB=60

# Templates (build with Packer first!)
UBUNTU_TEMPLATE=packer-ubuntu-22.04
WINDOWS_TEMPLATE=packer-windows-2022

# SSH Key
SSH_PUBLIC_KEY_PATH=~/.ssh/id_ed25519.pub

# Optional
DEPLOY_WINDOWS=false
```

### 3. Build VM Templates with Packer

**See [PACKER_GUIDE.md](PACKER_GUIDE.md) for complete instructions.**

Quick version:

```bash
cd packer
# Create credentials.pkrvars.hcl with your vCenter details
# Upload Ubuntu 22.04 ISO to datastore

cd ubuntu-22.04
packer build -var-file=../credentials.pkrvars.hcl ubuntu.pkr.hcl
```

This creates `packer-ubuntu-22.04` template in vCenter (~20 minutes).

### 4. Get Datastore and Network Names

**Option 1: Use govc (CLI)**

```bash
# Install govc
choco install govc

# Configure
export GOVC_URL=https://vcenter.thresh.sh/sdk
export GOVC_USERNAME=administrator@vsphere.local
export GOVC_PASSWORD=your-password
export GOVC_INSECURE=true

# List datastores
govc ls -t Datastore /thresh/datastore/*

# List networks
govc ls -t Network /thresh/network/*
```

**Option 2: Use vSphere Client (GUI)**

1. Open https://vcenter.thresh.sh
2. Log in with administrator@vsphere.local
3. **Datastore**: Navigate to Datacenter → Storage → Note the datastore name
4. **Network**: Navigate to Datacenter → Networks → Note the network name

### 5. Initialize Pulumi

```bash
cd pulumi

# Login to Pulumi (local backend)
pulumi login --local

# Create new stack
pulumi stack init vcenter

# Preview deployment
pulumi preview
```

### 6. Deploy Test VM

```bash
pulumi up

# Wait 3-5 minutes, then get VM IP
pulumi stack output vmIp

# SSH into VM
ssh thresh@<ip-address>

# Test thresh
cd ~/thresh
./thresh/Thresh/bin/Debug/net10.0/thresh --version
```

---

## vCenter Cluster Setup (Single ESXi)

**Question:** Can I create a cluster with a single ESXi server?  
**Answer:** **Yes!** This is a common setup.

### Steps to Create Cluster in vCenter:

1. **Add ESXi Host to vCenter**:
   - vCenter → Hosts and Clusters
   - Right-click Datacenter → Add Host
   - Enter ESXi IP/hostname and credentials
   - Accept certificate

2. **Create Cluster**:
   - Right-click Datacenter → New Cluster
   - Name: `thresh-cluster`
   - Enable DRS (optional, but recommended)
   - Enable HA (optional)

3. **Move Host to Cluster**:
   - Drag ESXi host into cluster
   - Or: Right-click host → Move To → Select cluster

4. **Verify Resource Pool**:
   - Cluster → Resource Pools
   - Default pool should be "Resources"
   - Update `VSPHERE_RESOURCE_POOL=Resources` in `.env`

---

## Architecture

### Current Setup (Simple)

```
vCenter (vcenter.thresh.sh)
└── Datacenter: thresh
    ├── Cluster: thresh-cluster
    │   └── ESXi Host (128GB RAM, 24 vCPUs)
    │       └── Resources (Resource Pool)
    ├── Datastores
    │   └── [your-datastore]
    └── Networks
        └── VM Network

Templates (Built with Packer):
├── packer-ubuntu-22.04     ← Ubuntu 22.04 LTS + cloud-init
└── packer-windows-2022     ← Windows Server 2022 (optional)

Test VMs (Deployed with Pulumi):
└── thresh-ubuntu-test      ← Cloned from template
```

### Workflow

```
1. Packer builds templates (once)
   └── Creates: packer-ubuntu-22.04

2. Pulumi clones template (many times)
   └── Creates: thresh-ubuntu-test (with cloud-init)
   
3. Cloud-init provisions VM
   └── Installs: Docker, .NET 10, thresh repo
   
4. Test thresh
   └── SSH → build → test → destroy
```

---

## Resource Allocation

**Your Server:**
- **Memory:** 128GB  
- **CPUs:** 24 (Xeon E5-2670 v3 @ 2.30GHz)

**Current VM Configuration** (from `.env`):
- **Test VM:** 4 vCPUs, 8GB RAM, 60GB disk

**Capacity:**
- Can run ~12-15 test VMs simultaneously
- Adequate for parallel testing (Phase 1.5+)

**Recommendations:**
- Start with 1 VM to validate setup
- Scale to 3-5 VMs for multi-OS testing  
- Use resource pools to limit CPU/RAM

---

## Common Operations

### Deploy Test VM

```bash
pulumi up
```

### Get VM Details

```bash
pulumi stack output
pulumi stack output vmIp
pulumi stack output sshCommand
```

### SSH into VM

```bash
ssh thresh@$(pulumi stack output vmIp)
```

### Destroy Test VM

```bash
pulumi destroy --yes
```

### Update Template

```bash
# Rebuild Packer template
cd packer/ubuntu-22.04
packer build -force -var-file=../credentials.pkrvars.hcl ubuntu.pkr.hcl

# Destroy old VMs, deploy new
cd ../../
pulumi destroy --yes
pulumi up
```

### Change VM Resources

Edit `.env`:
```env
VM_CPUS=8            # Increase CPUs
VM_MEMORY_MB=16384   # Increase to 16GB
VM_DISK_GB=100       # Increase disk
```

Then:
```bash
pulumi destroy && pulumi up
```

---

## Troubleshooting

### "Template not found"

```bash
# Error: error fetching virtual machine
# Resolution: Build template first with Packer
cd packer/ubuntu-22.04
packer build -var-file=../credentials.pkrvars.hcl ubuntu.pkr.hcl
```

### "Datastore not found"

```bash
# Get datastore name from vCenter
govc ls -t Datastore /thresh/datastore/*
# Update VSPHERE_DATASTORE in .env
```

### "Network not found"

```bash
# Get network name
govc ls -t Network /thresh/network/*
# Update VSPHERE_NETWORK in .env
```

### VM doesn't get IP address

```bash
# Check cloud-init logs via vCenter console
# Verify network has DHCP
# Increase wait time in pulumi up
```

### SSH connection refused

```bash
# Wait for cloud-init to complete (3-5 minutes)
# Check VM console in vCenter
# Verify SSH key is correct
```

---

## Next Steps

1. ✅ **Setup vCenter cluster** (single ESXi is fine)
2. ✅ **Get datastore and network names** (via govc or GUI)
3. ⏭️ **Update `pulumi/.env`** with your details
4. ⏭️ **Build Packer template** (see [PACKER_GUIDE.md](PACKER_GUIDE.md))
5. ⏭️ **Deploy first test VM** with `pulumi up`
6. ⏭️ **Add Windows template** (optional)
7. ⏭️ **Multi-OS testing** (Phase 1.5+)

---

## File Structure

```
pulumi/
├── .env                      ← Your configuration (gitignored)
├── .env.example              ← Template configuration
├── Program.cs                ← Main Pulumi infrastructure code
├── pulumi.csproj             ← .NET project file
├── Pulumi.yaml               ← Pulumi project metadata
├── Pulumi.vcenter.yaml       ← Stack configuration
├── README.md                 ← This file
├── PACKER_GUIDE.md           ← Packer template building guide
├── QUICKSTART.md             ← Old quick start (deprecated)
└── packer/
    ├── credentials.pkrvars.hcl    ← Packer vCenter credentials
    ├── ubuntu-22.04/
    │   ├── ubuntu.pkr.hcl         ← Ubuntu Packer template
    │   ├── http/
    │   │   └── user-data          ← Ubuntu autoinstall config
    │   └── scripts/
    │       └── setup.sh           ← Post-install script
└── windows-2022/
        ├── windows.pkr.hcl         ← Windows Packer template
        ├── autounattend.xml        ← Windows unattend config
        └── scripts/
            └── setup.ps1           ← Post-install script
```

---

## Reference

- [Pulumi VSphere Provider](https://www.pulumi.com/registry/packages/vsphere/)
- [Packer Guide](PACKER_GUIDE.md)
- [VMware vSphere Documentation](https://docs.vmware.com/en/VMware-vSphere/)
- [Cloud-init Documentation](https://cloudinit.readthedocs.io/)
