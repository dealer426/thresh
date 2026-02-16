#!/bin/bash
# Import Ubuntu Cloud Image as template for testing

set -e

# Load ESXi credentials
source .env

# Set govc environment variables
export GOVC_URL="$VSPHERE_SERVER"
export GOVC_USERNAME="$VSPHERE_USER"
export GOVC_PASSWORD="$VSPHERE_PASSWORD"
export GOVC_INSECURE=true
export GOVC_DATACENTER="$VSPHERE_DATACENTER"
export GOVC_DATASTORE="$VSPHERE_DATASTORE"
export GOVC_NETWORK="$VSPHERE_NETWORK"
export GOVC_RESOURCE_POOL="$VSPHERE_RESOURCE_POOL"

echo "=== Downloading Ubuntu 22.04 Cloud Image OVA ==="
if [ ! -f "ubuntu-22.04-server-cloudimg-amd64.ova" ]; then
    curl -L -o ubuntu-22.04-server-cloudimg-amd64.ova \
        https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova
    echo "✓ Downloaded Ubuntu Cloud Image"
else
    echo "✓ Ubuntu Cloud Image already downloaded"
fi

echo ""
echo "=== Importing OVA to ESXi as Template ==="
../govc.exe import.ova \
    -name="$UBUNTU_TEMPLATE" \
    -ds="$VSPHERE_DATASTORE" \
    -pool="$VSPHERE_RESOURCE_POOL" \
    -net="$VSPHERE_NETWORK" \
    ubuntu-22.04-server-cloudimg-amd64.ova

echo ""
echo "=== Marking VM as Template ==="
../govc.exe vm.markastemplate "$UBUNTU_TEMPLATE"

echo ""
echo "✅ SUCCESS! Template '$UBUNTU_TEMPLATE' created and ready for cloning"
echo ""
echo "Next steps:"
echo "1. Set CREATE_TEMPLATE=false in .env"
echo "2. Run: ./pulumi-cli/pulumi/bin/pulumi.exe up"
echo "   This will clone the template and create your test VM"
