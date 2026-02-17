# Quick Start: Pulumi Template Setup

Get your Ubuntu Cloud-Init template ready for thresh testing in 5 minutes.

## Fastest Method: Manual Import (5 minutes)

### Step 1: Download Ubuntu Cloud Image

```powershell
# PowerShell
Invoke-WebRequest -Uri "https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova" -OutFile "ubuntu-22.04-cloud.ova"
```

### Step 2: Import to vCenter

1. Open vSphere Client
2. Right-click your **Datacenter** → **Deploy OVF Template**
3. Select file: **ubuntu-22.04-cloud.ova**
4. Name: **ubuntu-22.04-cloud-init**
5. Select your datastore
6. Select your network
7. Click **Finish**

### Step 3: Convert to Template

1. Wait for deployment to complete (~30 seconds)
2. Right-click the new VM → **Template** → **Convert to Template**
3. Confirm the conversion

### Step 4: Configure Pulumi

```bash
cd pulumi
cp .env.example .env

# Edit .env with your vCenter details:
# - VSPHERE_SERVER=your-vcenter.local
# - VSPHERE_USER=administrator@vsphere.local
# - VSPHERE_PASSWORD=YourPassword
# - UBUNTU_TEMPLATE=ubuntu-22.04-cloud-init
# - CREATE_TEMPLATE=false (use the template you just created)
```

### Step 5: Deploy Test VM

```bash
# Restore dependencies
dotnet restore

# Login to Pulumi (local state)
pulumi login --local

# Select stack
pulumi stack select dev

# Deploy!
pulumi up
```

✅ **Done!** Your Ubuntu VM will boot with cloud-init, Docker, .NET 10, and thresh pre-installed.

---

## Alternative: Guided Setup (10 minutes)

If you want Pulumi to help guide you through template creation:

### Step 1: Enable Template Creation Mode

```bash
cd pulumi
cp .env.example .env

# Edit .env:
# CREATE_TEMPLATE=true  (enable guided mode)
# Fill in other vCenter details
```

### Step 2: Run Pulumi

```bash
dotnet restore
pulumi login --local
pulumi stack select dev
pulumi up
```

### Step 3: Follow Instructions

Pulumi will print detailed instructions for:
- Downloading Ubuntu Cloud Image
- Importing via vSphere Client or govc CLI
- Converting to template

### Step 4: Complete Setup

After importing the template:

```bash
# Set CREATE_TEMPLATE=false in .env
pulumi up  # Deploy actual test VM
```

---

## Verify Template is Ready

Before running `pulumi up`, verify your template exists:

1. Open vSphere Client
2. Navigate to your datacenter
3. Look for: **ubuntu-22.04-cloud-init** (with template icon)
4. Ensure it's marked as a **Template**, not a VM

---

## Next Steps

After successful deployment:

```bash
# Get VM IP address
pulumi stack output vmIp

# SSH into VM (password-less with your key)
ssh thresh@<vm-ip>

# Test thresh
cd thresh
dotnet build thresh/Thresh/Thresh.csproj
cd thresh/Thresh/bin/Debug/net10.0
./thresh --version
```

---

## Troubleshooting

### Template not found error

**Error**: `Template 'ubuntu-22.04-cloud-init' not found`

**Solution**:
1. Check template name in vCenter matches `.env` exactly
2. Ensure template is in the correct datacenter
3. Verify template is marked as "Template" not "VM"

### OVA download fails

**Solution**: Download directly from browser:
https://cloud-images.ubuntu.com/releases/22.04/release/

Look for: `ubuntu-22.04-server-cloudimg-amd64.ova`

### vCenter authentication fails

**Solution**:
1. Verify VSPHERE_SERVER (no https://, just hostname)
2. Verify username format: `administrator@vsphere.local`
3. Test credentials in vSphere Client first
4. Set `vsphere:allowUnverifiedSsl: "true"` in `Pulumi.dev.yaml`

### VM boots but cloud-init doesn't run

**Solution**:
- Ubuntu Cloud Images have cloud-init pre-installed
- Wait 2-3 minutes for cloud-init to complete
- Check logs: `ssh thresh@<ip> sudo cat /var/log/cloud-init-output.log`
- Verify your SSH key is in `.env` correctly

---

## Clean Up

When done testing:

```bash
# Destroy the test VM
pulumi destroy

# Confirm destruction
yes
```

The template remains in vCenter for future use.
