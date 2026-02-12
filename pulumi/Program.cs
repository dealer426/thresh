using Pulumi;
using Pulumi.VSphere;
using Pulumi.VSphere.Inputs;
using DotNetEnv;

class Program
{
    static Task<int> Main() => Deployment.RunAsync(() =>
    {
        // Load environment variables from .env file
        Env.Load();
        
        // Get configuration from environment
        var vSphereServer = Environment.GetEnvironmentVariable("VSPHERE_SERVER") 
            ?? throw new Exception("VSPHERE_SERVER not set in .env");
        var datacenterName = Environment.GetEnvironmentVariable("VSPHERE_DATACENTER") 
            ?? throw new Exception("VSPHERE_DATACENTER not set in .env");
        var clusterName = Environment.GetEnvironmentVariable("VSPHERE_CLUSTER") 
            ?? throw new Exception("VSPHERE_CLUSTER not set in .env");
        var datastoreName = Environment.GetEnvironmentVariable("VSPHERE_DATASTORE") 
            ?? throw new Exception("VSPHERE_DATASTORE not set in .env");
        var networkName = Environment.GetEnvironmentVariable("VSPHERE_NETWORK") 
            ?? throw new Exception("VSPHERE_NETWORK not set in .env");
        var resourcePoolName = Environment.GetEnvironmentVariable("VSPHERE_RESOURCE_POOL") 
            ?? "Resources";
        var ubuntuTemplate = Environment.GetEnvironmentVariable("UBUNTU_TEMPLATE") 
            ?? "ubuntu-22.04-template";
        var sshPublicKeyPath = Environment.GetEnvironmentVariable("SSH_PUBLIC_KEY_PATH") 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa.pub");

        // Get vSphere datacenter
        var datacenter = GetDatacenter.Invoke(new GetDatacenterInvokeArgs
        {
            Name = datacenterName
        });

        // Get compute cluster
        var cluster = datacenter.Apply(dc => GetComputeCluster.Invoke(new GetComputeClusterInvokeArgs
        {
            Name = clusterName,
            DatacenterId = dc.Id
        }));

        // Get resource pool
        var resourcePool = cluster.Apply(c => GetResourcePool.Invoke(new GetResourcePoolInvokeArgs
        {
            Name = resourcePoolName,
            DatacenterId = datacenter.Apply(dc => dc.Id)
        }));

        // Get datastore
        var datastore = datacenter.Apply(dc => GetDatastore.Invoke(new GetDatastoreInvokeArgs
        {
            Name = datastoreName,
            DatacenterId = dc.Id
        }));

        // Get network
        var network = datacenter.Apply(dc => GetNetwork.Invoke(new GetNetworkInvokeArgs
        {
            Name = networkName,
            DatacenterId = dc.Id
        }));

        // Get VM template
        var template = datacenter.Apply(dc => GetVirtualMachine.Invoke(new GetVirtualMachineInvokeArgs
        {
            Name = ubuntuTemplate,
            DatacenterId = dc.Id
        }));

        // Read SSH public key
        var sshPublicKey = File.Exists(sshPublicKeyPath) 
            ? File.ReadAllText(sshPublicKeyPath).Trim()
            : throw new Exception($"SSH public key not found at {sshPublicKeyPath}");

        // Cloud-init configuration for Ubuntu
        var cloudInitMetadata = @"
instance-id: thresh-ubuntu-test
local-hostname: thresh-ubuntu-test
";

        var cloudInitUserdata = $@"
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
      - {sshPublicKey}

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
  - echo ""deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"" | tee /etc/apt/sources.list.d/docker.list > /dev/null
  - apt-get update
  - apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
  - usermod -aG docker thresh
  
  # Install .NET 10 SDK (adjust when .NET 10 is released)
  - wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  - chmod +x dotnet-install.sh
  - ./dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet
  - ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet
  
  # Clone thresh repository
  - su - thresh -c ""git clone https://github.com/dealer426/thresh.git /home/thresh/thresh""
  
  # Ensure Docker service is running
  - systemctl enable docker
  - systemctl start docker

final_message: ""thresh Ubuntu test VM is ready! SSH as thresh@${{public_ip}}""
";

        // Create Ubuntu 22.04 VM for testing
        var ubuntuVM = new VirtualMachine("thresh-ubuntu-test", new VirtualMachineArgs
        {
            Name = "thresh-ubuntu-test",
            ResourcePoolId = resourcePool.Apply(rp => rp.Id),
            DatastoreId = datastore.Apply(ds => ds.Id),
            
            NumCpus = 2,
            Memory = 4096, // 4GB RAM
            
            GuestId = template.Apply(t => t.GuestId),
            Firmware = "efi",
            
            NetworkInterfaces = new VirtualMachineNetworkInterfaceArgs
            {
                NetworkId = network.Apply(n => n.Id)
            },
            
            Disks = new VirtualMachineDiskArgs
            {
                Label = "disk0",
                Size = 50, // 50GB disk
                ThinProvisioned = true
            },
            
            Clone = new VirtualMachineCloneArgs
            {
                TemplateUuid = template.Apply(t => t.Id),
                
                Customize = new VirtualMachineCloneCustomizeArgs
                {
                    LinuxOptions = new VirtualMachineCloneCustomizeLinuxOptionsArgs
                    {
                        HostName = "thresh-ubuntu-test",
                        Domain = "local"
                    },
                    
                    NetworkInterfaces = new VirtualMachineCloneCustomizeNetworkInterfaceArgs
                    {
                        // Use DHCP for simplicity
                    }
                }
            },
            
            // Inject cloud-init configuration
            Cdrom = new VirtualMachineCdromArgs
            {
                ClientDevice = true
            },
            
            VappTransport = new[] { "com.vmware.guestInfo" },
            
            ExtraConfig = 
            {
                { "guestinfo.metadata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitMetadata)) },
                { "guestinfo.metadata.encoding", "base64" },
                { "guestinfo.userdata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitUserdata)) },
                { "guestinfo.userdata.encoding", "base64" }
            }
        });

        // Export VM details
        return new Dictionary<string, object?>
        {
            ["vmName"] = ubuntuVM.Name,
            ["vmIp"] = ubuntuVM.DefaultIpAddress,
            ["vmId"] = ubuntuVM.Id,
            ["sshCommand"] = ubuntuVM.DefaultIpAddress.Apply(ip => $"ssh thresh@{ip}"),
            ["instructions"] = Output.Create(@"
To connect to the Ubuntu test VM:
1. Wait for cloud-init to complete (2-3 minutes)
2. SSH into the VM: ssh thresh@<ip-address>
3. Navigate to thresh: cd ~/thresh
4. Build thresh: dotnet build thresh/Thresh/Thresh.csproj
5. Run thresh: cd thresh/Thresh/bin/Debug/net10.0 && ./thresh --version

The VM has Docker, containerd, .NET 10 SDK, and the thresh repository pre-installed.
")
        };
    });
}
