#!/bin/bash
# Clone Ubuntu template and set up test VM using govc

set -e

# Load ESXi credentials
source .env

# Set govc environment variables
export GOVC_URL="$VSPHERE_SERVER"
export GOVC_USERNAME="$VSPHERE_USER"
export GOVC_PASSWORD="$VSPHERE_PASSWORD"
export GOVC_INSECURE=true

VM_NAME="thresh-ubuntu-test"
TEMPLATE_NAME="$UBUNTU_TEMPLATE"
DATASTORE="$VSPHERE_DATASTORE"

echo "=== Cloning Ubuntu Template to Test VM ==="

# Check if test VM already exists and delete it
if ../govc.exe vm.info "$VM_NAME" &>/dev/null; then
    echo "VM $VM_NAME already exists, deleting..."
    ../govc.exe vm.destroy "$VM_NAME" || true
fi

# Clone the template
echo "Cloning $TEMPLATE_NAME to $VM_NAME..."
../govc.exe vm.clone -vm="$TEMPLATE_NAME" -on=false "$VM_NAME"

echo "✓ Cloned successfully"

# Customize VM resources
echo ""
echo "=== Customizing VM Resources ==="
../govc.exe vm.change -vm="$VM_NAME" -c=2 -m=4096 -e="disk.enableUUID=1"

# Expand disk to 50GB
echo "Expanding disk to 50GB..."
../govc.exe vm.disk.change -vm="$VM_NAME" -disk.label="Hard disk 1" -size=50G

echo "✓ Resources configured: 2 CPU, 4GB RAM, 50GB disk"

# Read SSH public key
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
  - echo "Cloud-init complete" > /var/lib/cloud/instance/boot-finished
  
power_state:
  mode: poweroff
  timeout: 300
  condition: True
EOF
)

# Encode cloud-init data to base64 (required for guestinfo)
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
echo "✅ SUCCESS! Test VM created and powered on"
echo ""
echo "VM Details:"
../govc.exe vm.info "$VM_NAME"

echo ""
echo "Cloud-init is running (takes 2-3 minutes)..."
echo "Monitor progress: ../govc.exe vm.console $VM_NAME"
echo ""
echo "Once cloud-init completes, VM will power off automatically."
echo "Then power it on and SSH: ssh thresh@<vm-ip>"
