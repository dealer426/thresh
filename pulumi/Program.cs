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
        var vSphereUser = Environment.GetEnvironmentVariable("VSPHERE_USER") 
            ?? throw new Exception("VSPHERE_USER not set in .env");
        var vSpherePassword = Environment.GetEnvironmentVariable("VSPHERE_PASSWORD") 
            ?? throw new Exception("VSPHERE_PASSWORD not set in .env");
        var datacenterName = Environment.GetEnvironmentVariable("VSPHERE_DATACENTER") 
            ?? throw new Exception("VSPHERE_DATACENTER not set in .env");
        var clusterName = Environment.GetEnvironmentVariable("VSPHERE_CLUSTER") 
            ?? "";
        var datastoreName = Environment.GetEnvironmentVariable("VSPHERE_DATASTORE") 
            ?? throw new Exception("VSPHERE_DATASTORE not set in .env");
        var networkName = Environment.GetEnvironmentVariable("VSPHERE_NETWORK") 
            ?? throw new Exception("VSPHERE_NETWORK not set in .env");
        var resourcePoolName = Environment.GetEnvironmentVariable("VSPHERE_RESOURCE_POOL") 
            ?? "Resources";
        var ubuntuTemplate = Environment.GetEnvironmentVariable("UBUNTU_TEMPLATE") 
            ?? "ubuntu-22.04-cloud-init";
        var createTemplate = Environment.GetEnvironmentVariable("CREATE_TEMPLATE")?.ToLower() == "true";
        var ubuntuOvaPath = Environment.GetEnvironmentVariable("UBUNTU_OVA_PATH") 
            ?? "[datastore1] ISO/ubuntu-22.04-server-cloudimg-amd64.ova";
        var sshPublicKeyPath = Environment.GetEnvironmentVariable("SSH_PUBLIC_KEY_PATH") 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_ed25519.pub");

        // Create vSphere provider with explicit credentials
        var vsphereProvider = new Provider("vsphere", new ProviderArgs
        {
            User = vSphereUser,
            Password = vSpherePassword,
            VsphereServer = vSphereServer,
            AllowUnverifiedSsl = true
        });

        // Get vSphere datacenter
        var datacenter = GetDatacenter.Invoke(new GetDatacenterInvokeArgs
        {
            Name = datacenterName
        }, new InvokeOptions { Provider = vsphereProvider });

        // Get resource pool (skip cluster lookup for standalone ESXi)
        Output<GetResourcePoolResult> resourcePool;
        if (!string.IsNullOrEmpty(clusterName))
        {
            // Get compute cluster if specified
            var cluster = datacenter.Apply(dc => GetComputeCluster.Invoke(new GetComputeClusterInvokeArgs
            {
                Name = clusterName,
                DatacenterId = dc.Id
            }, new InvokeOptions { Provider = vsphereProvider }));

            resourcePool = cluster.Apply(c => GetResourcePool.Invoke(new GetResourcePoolInvokeArgs
            {
                Name = resourcePoolName,
                DatacenterId = datacenter.Apply(dc => dc.Id)
            }, new InvokeOptions { Provider = vsphereProvider }));
        }
        else
        {
            // For standalone ESXi, get resource pool directly
            resourcePool = datacenter.Apply(dc => GetResourcePool.Invoke(new GetResourcePoolInvokeArgs
            {
                Name = resourcePoolName,
                DatacenterId = dc.Id
            }, new InvokeOptions { Provider = vsphereProvider }));
        }

        // Get datastore
        var datastore = datacenter.Apply(dc => GetDatastore.Invoke(new GetDatastoreInvokeArgs
        {
            Name = datastoreName,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        // Get network
        var network = datacenter.Apply(dc => GetNetwork.Invoke(new GetNetworkInvokeArgs
        {
            Name = networkName,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        // Read SSH public key
        var sshPublicKey = System.IO.File.Exists(sshPublicKeyPath) 
            ? System.IO.File.ReadAllText(sshPublicKeyPath).Trim()
            : throw new Exception($"SSH public key not found at {sshPublicKeyPath}");

        // === TEMPLATE CREATION SECTION ===
        // If CREATE_TEMPLATE=true, create Ubuntu Cloud-Init template VM
        VirtualMachine? templateVM = null;
        
        if (createTemplate)
        {
            Pulumi.Log.Info("Creating Ubuntu Cloud-Init template from scratch...");
            
            // Create a minimal Ubuntu VM that will become the template
            templateVM = new VirtualMachine("ubuntu-cloud-init-template", new VirtualMachineArgs
            {
                Name = ubuntuTemplate,
                ResourcePoolId = resourcePool.Apply(rp => rp.Id),
                DatastoreId = datastore.Apply(ds => ds.Id),
                
                NumCpus = 2,
                Memory = 2048,
                
                GuestId = "ubuntu64Guest",
                Firmware = "efi",
                
                NetworkInterfaces = new VirtualMachineNetworkInterfaceArgs
                {
                    NetworkId = network.Apply(n => n.Id)
                },
                
                Disks = new VirtualMachineDiskArgs
                {
                    Label = "disk0",
                    Size = 20,
                    ThinProvisioned = true
                },
                
                // This VM will be converted to template manually or via vCenter automation
                // For OVA import, you'd use vCenter UI or govc CLI
                ExtraConfig = {
                    { "disk.EnableUUID", "TRUE" }
                }
            }, new CustomResourceOptions { Provider = vsphereProvider });
            
            Pulumi.Log.Warn($@"
================================================================================
TEMPLATE CREATION INSTRUCTIONS:
================================================================================

A placeholder VM '{ubuntuTemplate}' has been created.

To complete template setup, you have TWO options:

OPTION 1 - Use Ubuntu Cloud Image (RECOMMENDED):
------------------------------------------------
1. Download Ubuntu Cloud Image OVA:
   wget https://cloud-images.ubuntu.com/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.ova

2. Upload to vCenter datastore via vSphere Client

3. Deploy OVF Template in vCenter:
   - Right-click datacenter → Deploy OVF Template
   - Select the downloaded OVA file
   - Name it: {ubuntuTemplate}
   - Select datastore: {datastoreName}
   - Select network: {networkName}

4. After deployment, convert to template:
   - Right-click VM → Template → Convert to Template

5. Delete the placeholder VM created by Pulumi (optional)

6. Re-run: pulumi up (will now use the real template)

OPTION 2 - Use govc CLI (AUTOMATED):
-------------------------------------
1. Install govc: https://github.com/vmware/govmomi/releases

2. Set environment:
   export GOVC_URL=https://{vSphereServer}/sdk
   export GOVC_USERNAME=$VSPHERE_USER
   export GOVC_PASSWORD=$VSPHERE_PASSWORD
   export GOVC_INSECURE=true

3. Import OVA:
   govc import.ova -name={ubuntuTemplate} \\
     -ds={datastoreName} \\
     -pool={resourcePoolName} \\
     ubuntu-22.04-server-cloudimg-amd64.ova

4. Mark as template:
   govc vm.markastemplate {ubuntuTemplate}

5. Re-run: pulumi up

================================================================================
After template is ready, set CREATE_TEMPLATE=false in .env
================================================================================
");
            
            // Export placeholder VM details
            return new Dictionary<string, object?>
            {
                ["templateName"] = templateVM.Name,
                ["templateId"] = templateVM.Id,
                ["instructions"] = Output.Create(@"
Template placeholder created. Follow the instructions above to import Ubuntu Cloud Image.

Once template is ready:
1. Set CREATE_TEMPLATE=false in .env
2. Run: pulumi up (to create the test VM)
")
            };
        }

        // === TEST VM CREATION SECTION ===
        // Only proceed if CREATE_TEMPLATE=false (template should already exist)
        
        // Get VM template
        var template = datacenter.Apply(dc => GetVirtualMachine.Invoke(new GetVirtualMachineInvokeArgs
        {
            Name = ubuntuTemplate,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

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
            
            // Inject cloud-init configuration via ExtraConfig
            ExtraConfig = 
            {
                { "guestinfo.metadata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitMetadata)) },
                { "guestinfo.metadata.encoding", "base64" },
                { "guestinfo.userdata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitUserdata)) },
                { "guestinfo.userdata.encoding", "base64" }
            }
        }, new CustomResourceOptions { Provider = vsphereProvider });

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
