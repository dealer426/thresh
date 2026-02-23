packer {
  required_version = ">= 1.7.0"
  required_plugins {
    vsphere = {
      version = ">= 1.0.3"
      source  = "github.com/hashicorp/vsphere"
    }
  }
}

# Variables
variable "vsphere_server" {
  type = string
}

variable "vsphere_user" {
  type = string
}

variable "vsphere_password" {
  type      = string
  sensitive = true
}

variable "vsphere_insecure" {
  type    = bool
  default = true
}

variable "vsphere_datacenter" {
  type = string
}

variable "vsphere_cluster" {
  type = string
}

variable "vsphere_datastore" {
  type = string
}

variable "vsphere_network" {
  type = string
}

variable "ssh_username" {
  type    = string
  default = "thresh"
}

variable "ssh_password" {
  type      = string
  sensitive = true
}

variable "iso_url" {
  type    = string
  default = "https://releases.ubuntu.com/22.04.5/ubuntu-22.04.5-live-server-amd64.iso"
}

variable "iso_checksum" {
  type    = string
  default = "sha256:9bc6028870aef3f74f4e16b900008179e78b130e6b0b9a140635434a46aa98b0"
}

# Source definition
source "vsphere-iso" "ubuntu" {
  vcenter_server      = var.vsphere_server
  username            = var.vsphere_user
  password            = var.vsphere_password
  insecure_connection = var.vsphere_insecure

  datacenter = var.vsphere_datacenter
  cluster    = var.vsphere_cluster
  datastore  = var.vsphere_datastore

  vm_name       = "packer-ubuntu-22.04"
  guest_os_type = "ubuntu64Guest"
  
  CPUs                 = 2
  RAM                  = 4096
  disk_controller_type = ["pvscsi"]
  
  storage {
    disk_size             = 20480
    disk_thin_provisioned = true
  }

  network_adapters {
    network      = var.vsphere_network
    network_card = "vmxnet3"
  }

  iso_url      = var.iso_url
  iso_checksum = var.iso_checksum

  http_directory = "http"
  boot_wait      = "3s"
  boot_command = [
    "c<wait>",
    "linux /casper/vmlinuz autoinstall ds=nocloud-net\\;s=http://{{.HTTPIP}}:{{.HTTPPort}}/ ---<enter>",
    "initrd /casper/initrd<enter>",
    "boot<enter>"
  ]

  ssh_username         = var.ssh_username
  ssh_password         = var.ssh_password
  ssh_timeout          = "30m"
  ssh_handshake_attempts = 100

  shutdown_command = "echo '${var.ssh_password}' | sudo -S shutdown -P now"
  shutdown_timeout = "15m"

  convert_to_template = true
}

# Build
build {
  sources = ["source.vsphere-iso.ubuntu"]

  # Wait for cloud-init to finish
  provisioner "shell" {
    inline = [
      "while [ ! -f /var/lib/cloud/instance/boot-finished ]; do echo 'Waiting for cloud-init...'; sleep 1; done",
      "sudo apt-get update",
      "sudo apt-get install -y open-vm-tools",
      "sudo systemctl enable open-vm-tools",
      "sudo systemctl start open-vm-tools"
    ]
  }

  # Clean up
  provisioner "shell" {
    inline = [
      "sudo apt-get clean",
      "sudo cloud-init clean --logs --seed",
      "sudo rm -rf /var/lib/cloud/instances/*",
      "sudo truncate -s 0 /etc/machine-id",
      "sudo rm -f /var/lib/dbus/machine-id",
      "sudo ln -s /etc/machine-id /var/lib/dbus/machine-id",
      "sudo sync"
    ]
  }
}
