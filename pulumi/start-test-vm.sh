#!/bin/bash
# Configure and start the Ubuntu VM for testing (no cloning needed on ESXi standalone)

set -e

# Load ESXi credentials
source .env

# Set govc environment variables
export GOVC_URL="$VSPHERE_SERVER"
export GOVC_USERNAME="$VSPHERE_USER"
export GOVC_PASSWORD="$VSPHERE_PASSWORD"
export GOVC_INSECURE=true

VM_NAME="$UBUNTU_TEMPLATE"

echo "=== Configuring Ubuntu VM for Testing ==="
echo "VM: $VM_NAME"

# Customize VM resources (2 CPU, 4GB RAM, disk expansion)
echo ""
echo "=== Customizing VM Resources ==="
../govc.exe vm.change -vm="$VM_NAME" -c=2 -m=4096 -e="disk.enableUUID=1"
echo "✓ Set 2 CPU, 4096MB RAM"

# Read SSH public key
if [[ ! -f "$SSH_PUBLIC_KEY_PATH" ]]; then
    echo "ERROR: SSH public key not found at $SSH_PUBLIC_KEY_PATH"
    exit 1
fi

SSH_KEY=$(cat "$SSH_PUBLIC_KEY_PATH" | tr -d '\r\n')

# Create cloud-init metadata
METADATA=$(cat <<'EOF'
instance-id: thresh-ubuntu-test
local-hostname: thresh-ubuntu-test
EOF
)

# Create cloud-init userdata  
USERDATA=$(cat <<EOF
#cloud-config
hostname: thresh-ubuntu-test
fqdn: thresh-ubuntu-test.local
manage_etc_hosts: true

users:
  - name: thresh
    sudo: ALL=(ALL) NOPASSWD:ALL
    groups: users, admin, docker
    shell: /bin/bash
    ssh_authorized_keys:
      - $SSH_KEY

package_update: true
package_upgrade: true

packages:
  - git
  - curl
  - wget
  - vim
  - htop
  - build-essential
  - ca-certificates
  - gnupg
  - lsb-release

runcmd:
  # Install Docker
  - mkdir -p /etc/apt/keyrings
  - curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
  - echo "deb [arch=\$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \$(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
  - apt-get update
  - apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
  - usermod -aG docker thresh
  
  # Install .NET 10
  - wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
  - chmod +x /tmp/dotnet-install.sh
  - sudo -u thresh /tmp/dotnet-install.sh --channel 10.0 --install-dir /home/thresh/.dotnet
  - echo 'export DOTNET_ROOT=/home/thresh/.dotnet' >> /home/thresh/.bashrc
  - echo 'export PATH=\$PATH:/home/thresh/.dotnet' >> /home/thresh/.bashrc
  
  # Clone thresh repository
  - sudo -u thresh git clone https://github.com/dealer426/thresh.git /home/thresh/thresh
  
  # Signal completion
  - echo "Cloud-init complete - $(date)" > /var/log/cloud-init-done.log
  
final_message: "Thresh test VM ready! Uptime: \$UPTIME"
EOF
)

# Encode cloud-init data to base64
METADATA_B64=$(echo "$METADATA" | base64 -w 0)
USERDATA_B64=$(echo "$USERDATA" | base64 -w 0)

echo ""
echo "=== Configuring Cloud-Init ==="
../govc.exe vm.change -vm="$VM_NAME" \
    -e="guestinfo.metadata=$METADATA_B64" \
    -e="guestinfo.metadata.encoding=base64" \
    -e="guestinfo.userdata=$USERDATA_B64" \
    -e="guestinfo.userdata.encoding=base64"

echo "✓ Cloud-init configured"

# Power on the VM
echo ""
echo "=== Powering On VM ==="
../govc.exe vm.power -on "$VM_NAME"

echo ""
echo "✅ SUCCESS! Test VM powered on"
echo ""
echo "VM Details:"
../govc.exe vm.info "$VM_NAME"

echo ""
echo "════════════════════════════════════════════════════════════"
echo "  Cloud-init is running (takes 3-5 minutes)"
echo "════════════════════════════════════════════════════════════"
echo ""
echo "Monitor installation progress:"
echo "  1. Open ESXi console: https://192.168.4.205"
echo "  2. Click on VM: $VM_NAME"
echo "  3. Launch console to watch cloud-init"
echo ""
echo "When installation completes:"
echo "  1. Get VM IP from ESXi console or:"
echo "     ../govc.exe vm.ip $VM_NAME"
echo ""
echo "  2. SSH into VM:"
echo "     ssh thresh@<vm-ip>"
echo ""
echo "  3. Test thresh:"
echo "     cd ~/thresh"
echo "     dotnet build thresh/Thresh/Thresh.csproj"
echo "     ./thresh/Thresh/bin/Debug/net10.0/Thresh --version"
echo ""
echo "════════════════════════════════════════════════════════════"
