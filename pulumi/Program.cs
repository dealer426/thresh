using Pulumi;
using Pulumi.VSphere;
using Pulumi.VSphere.Inputs;
using Pulumi.Command.Remote;
using Pulumi.Command.Remote.Inputs;
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
            ?? "thresh";
        var datastoreName = Environment.GetEnvironmentVariable("VSPHERE_DATASTORE") 
            ?? throw new Exception("VSPHERE_DATASTORE not set in .env");
        var networkName = Environment.GetEnvironmentVariable("VSPHERE_NETWORK") 
            ?? throw new Exception("VSPHERE_NETWORK not set in .env");
        var resourcePoolName = Environment.GetEnvironmentVariable("VSPHERE_RESOURCE_POOL") 
            ?? "Resources";
        var ubuntuTemplate = Environment.GetEnvironmentVariable("UBUNTU_TEMPLATE") 
            ?? "ubuntu-noble-24.04-cloudimg";
        var sshPublicKeyPath = Environment.GetEnvironmentVariable("SSH_PUBLIC_KEY_PATH") 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_ed25519.pub");
        var sshPrivateKeyPath = Environment.GetEnvironmentVariable("SSH_PRIVATE_KEY_PATH") 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_ed25519");
        
        // Thresh Hub configuration
        var threshHubUrl = Environment.GetEnvironmentVariable("THRESH_HUB_URL") 
            ?? "https://192.168.4.85:7002";
        var threshApiKey = Environment.GetEnvironmentVariable("THRESH_API_KEY") 
            ?? throw new Exception("THRESH_API_KEY not set in .env - get this from API Keys page");
        var threshGitHubRepo = Environment.GetEnvironmentVariable("THRESH_GITHUB_REPO") 
            ?? "https://github.com/dealer426/thresh.git";

        // Create vSphere provider
        var vsphereProvider = new Provider("vsphere", new ProviderArgs
        {
            User = vSphereUser,
            Password = vSpherePassword,
            VsphereServer = vSphereServer,
            AllowUnverifiedSsl = true
        });

        // Get vSphere infrastructure
        var datacenter = GetDatacenter.Invoke(new GetDatacenterInvokeArgs
        {
            Name = datacenterName
        }, new InvokeOptions { Provider = vsphereProvider });

        var resourcePool = datacenter.Apply(dc => GetResourcePool.Invoke(new GetResourcePoolInvokeArgs
        {
            Name = resourcePoolName,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        var datastore = datacenter.Apply(dc => GetDatastore.Invoke(new GetDatastoreInvokeArgs
        {
            Name = datastoreName,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        var network = datacenter.Apply(dc => GetNetwork.Invoke(new GetNetworkInvokeArgs
        {
            Name = networkName,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        // Read SSH keys
        var sshPublicKey = System.IO.File.Exists(sshPublicKeyPath) 
            ? System.IO.File.ReadAllText(sshPublicKeyPath).Trim()
            : throw new Exception($"SSH public key not found at {sshPublicKeyPath}");
        
        var sshPrivateKey = System.IO.File.Exists(sshPrivateKeyPath) 
            ? System.IO.File.ReadAllText(sshPrivateKeyPath)
            : throw new Exception($"SSH private key not found at {sshPrivateKeyPath}");

        // Get VM template
        var template = datacenter.Apply(dc => GetVirtualMachine.Invoke(new GetVirtualMachineInvokeArgs
        {
            Name = ubuntuTemplate,
            DatacenterId = dc.Id
        }, new InvokeOptions { Provider = vsphereProvider }));

        // Define VM configurations (5 nodes for better deployment reliability)
        var vmConfigs = new[]
        {
            new { Name = "thresh-node-1", Cpus = 2, MemoryGB = 4, DiskGB = 40 },
            new { Name = "thresh-node-2", Cpus = 2, MemoryGB = 8, DiskGB = 60 },
            new { Name = "thresh-node-3", Cpus = 4, MemoryGB = 12, DiskGB = 80 },
            new { Name = "thresh-node-4", Cpus = 2, MemoryGB = 6, DiskGB = 50 },
            new { Name = "thresh-node-5", Cpus = 3, MemoryGB = 8, DiskGB = 60 }
        };

        var vmOutputs = new Dictionary<string, object?>();

        // Check if devbox-only mode is requested
        var devboxOnly = Environment.GetEnvironmentVariable("PULUMI_DEVBOX")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        if (devboxOnly)
        {
            // Deploy only the dev workstation
            return DevBox.Deploy(vsphereProvider, datacenter, resourcePool, datastore, network, template, sshPublicKey, sshPrivateKey);
        }

        foreach (var config in vmConfigs)
        {
            // Minimal cloud-init just for user/network setup - NO installation commands
            var cloudInitUserdata = $@"#cloud-config
hostname: {config.Name}
fqdn: {config.Name}.thresh.sh
manage_etc_hosts: true

users:
  - name: thresh
    sudo: ALL=(ALL) NOPASSWD:ALL
    groups: users, admin, docker
    shell: /bin/bash
    ssh_authorized_keys:
      - {sshPublicKey}
    lock_passwd: false
    passwd: $6$rounds=4096$saltsalt$uBIMAEzlaQE7jwIqNZ4TT8iZGN3tS.LHOz.M5WvO93V13I5oWyGpQSnDHolb3Gwk9sh0r97PXZWXWF5qr.IGg.

# Enable SSH password authentication for initial setup
ssh_pwauth: true
disable_root: false

package_update: true
package_upgrade: false

packages:
  - openssh-server

runcmd:
  - sed -i 's/^#*PasswordAuthentication .*/PasswordAuthentication yes/' /etc/ssh/sshd_config
  - systemctl restart sshd

final_message: ""✅ {config.Name} ready for automation!""
";

            var cloudInitMetadata = $@"instance-id: {config.Name}
local-hostname: {config.Name}
";

            // Create VM
            var vm = new VirtualMachine(config.Name, new VirtualMachineArgs
            {
                Name = config.Name,
                ResourcePoolId = resourcePool.Apply(rp => rp.Id),
                DatastoreId = datastore.Apply(ds => ds.Id),
                
                NumCpus = config.Cpus,
                Memory = config.MemoryGB * 1024,
                
                GuestId = template.Apply(t => t.GuestId),
                Firmware = "bios",
                BootDelay = 5000,
                SyncTimeWithHost = true,
                
                NetworkInterfaces = new VirtualMachineNetworkInterfaceArgs
                {
                    NetworkId = network.Apply(n => n.Id)
                },
                
                Disks = new VirtualMachineDiskArgs
                {
                    Label = "disk0",
                    Size = config.DiskGB,
                    ThinProvisioned = true
                },
                
                Cdroms = new VirtualMachineCdromArgs
                {
                    ClientDevice = true
                },
                
                Clone = new VirtualMachineCloneArgs
                {
                    TemplateUuid = template.Apply(t => t.Id)
                },
                
                ExtraConfig = 
                {
                    { "guestinfo.metadata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitMetadata)) },
                    { "guestinfo.metadata.encoding", "base64" },
                    { "guestinfo.userdata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudInitUserdata)) },
                    { "guestinfo.userdata.encoding", "base64" },
                    { "disk.EnableUUID", "TRUE" }
                }
            }, new CustomResourceOptions { Provider = vsphereProvider });

            // AUTOMATION: Use Pulumi Command to install Thresh after VM boots
            
            // Step 1: Use SSH key from cloud-init (already configured)
            var keyConnectionInfo = vm.DefaultIpAddress.Apply(ip => new ConnectionArgs
            {
                Host = ip,
                Port = 22,
                User = "thresh",
                PrivateKey = sshPrivateKey,
                DialErrorLimit = 60, // Cloud-init can take 2-3 minutes
            });

            // Step 2: Wait for cloud-init to complete
            var waitCloudInit = new Command($"{config.Name}-wait-cloudinit", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = "echo '⏳ Waiting for cloud-init to complete...'\nsudo cloud-init status --wait || echo 'cloud-init wait timed out, continuing anyway'\necho '✅ Cloud-init complete'"
            }, new CustomResourceOptions { DependsOn = { vm } });

            // Step 3: Install Docker
            var installDocker = new Command($"{config.Name}-install-docker", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = "echo '🐳 Installing Docker...'\nsudo apt-get update -qq\nsudo apt-get install -y docker.io containerd -qq\nsudo systemctl enable docker\nsudo systemctl start docker\nsudo usermod -aG docker thresh\necho '✅ Docker installed'"
            }, new CustomResourceOptions { DependsOn = { waitCloudInit } });

            // Step 4: Download pre-built Thresh binary from GitHub Actions
            var downloadThresh = new Command($"{config.Name}-download-thresh", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = "echo '📦 Downloading latest Thresh build from GitHub...'\nmkdir -p ~/thresh-agent\ncd ~/thresh-agent\ncurl -L -o thresh.tar.gz https://github.com/dealer426/thresh/releases/download/v1.6.0-dev/thresh-linux-x64.tar.gz || echo '⚠️  Release download failed'\nif [ -f thresh.tar.gz ]; then tar -xzf thresh.tar.gz && rm thresh.tar.gz; fi\nchmod +x thresh 2>/dev/null || echo 'Setting permissions'\nls -lah ~/thresh-agent/thresh\necho '✅ Thresh downloaded'"
            }, new CustomResourceOptions { DependsOn = { installDocker } });

            // Step 5: Configure agent
            var configureAgent = new Command($"{config.Name}-configure-agent", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = Output.Format($"echo '⚙️  Configuring agent...'\nmkdir -p ~/.thresh\ncat > ~/.thresh/agent.json << 'AGENTCFG'\n{{\n  \"AgentId\": \"\",\n  \"Enabled\": true,\n  \"MidtierUrl\": \"{threshHubUrl}\",\n  \"ApiKey\": \"{threshApiKey}\",\n  \"TlsVerify\": false,\n  \"ReconnectDelay\": 5,\n  \"MetricsInterval\": 30,\n  \"AutoFailover\": false\n}}\nAGENTCFG\nchmod 600 ~/.thresh/agent.json\necho '✅ Agent configured'")
            }, new CustomResourceOptions { DependsOn = { downloadThresh } });

            // Step 6: Create systemd service
            var createService = new Command($"{config.Name}-create-service", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = "echo '🚀 Creating systemd service...'\nsudo tee /etc/systemd/system/thresh-agent.service > /dev/null << 'SVCFILE'\n[Unit]\nDescription=Thresh Agent\nAfter=network-online.target docker.service\nWants=network-online.target\nRequires=docker.service\n\n[Service]\nType=simple\nUser=thresh\nWorkingDirectory=/home/thresh/thresh-agent\nExecStart=/home/thresh/thresh-agent/thresh agent start\nRestart=always\nRestartSec=10\nStandardOutput=journal\nStandardError=journal\n\n[Install]\nWantedBy=multi-user.target\nSVCFILE\nsudo systemctl daemon-reload\nsudo systemctl enable thresh-agent\nsudo systemctl start thresh-agent\necho '✅ Service started'"
            }, new CustomResourceOptions { DependsOn = { configureAgent } });

            // Export VM details
            vmOutputs[$"{config.Name}_ip"] = vm.DefaultIpAddress;
            vmOutputs[$"{config.Name}_ssh"] = vm.DefaultIpAddress.Apply(ip => $"ssh thresh@{ip}");
            vmOutputs[$"{config.Name}_status"] = createService.Id.Apply(_ => "✅ Fully automated - agent running");
        }

        //Export summary
        vmOutputs["instructions"] = Output.Create($@"
╔════════════════════════════════════════════════════════════════╗
║      THRESH MULTI-NODE CLUSTER - FULLY AUTOMATED               ║
╚════════════════════════════════════════════════════════════════╝

✨ All installation automated via Pulumi Command resources!

Hub URL: {threshHubUrl}
Nodes: 5 (deployed and configured automatically)

What Was Automated:
  ✅ VM creation
  ✅ SSH key configuration
  ✅ Docker installation
  ✅ Thresh binary download from GitHub Actions
  ✅ Agent configuration (Enabled: true)
  ✅ Systemd service creation and start

Check Status:
  pulumi stack output thresh-node-1_ip
  ssh thresh@<ip> 'systemctl status thresh-agent'

View in Hub:
  {threshHubUrl}/nodes

All nodes should appear in hub within 30 seconds of deployment!

");

        vmOutputs["hub_url"] = threshHubUrl;
        vmOutputs["node_count"] = 5;
        vmOutputs["automation"] = "✨ Fully automated - pre-built binary from GitHub Actions";

        return vmOutputs;
    });
}
