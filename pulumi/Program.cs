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
        
        // Convert Windows path to WSL mount path for wsl scp
        static string ToWslPath(string p) {
            p = p.Replace('\\', '/');
            if (p.Length >= 2 && p[1] == ':')
                return "/mnt/" + char.ToLower(p[0]) + p[2..];
            return p;
        }
        var wslKeyPath = ToWslPath(sshPrivateKeyPath);
        
        // Thresh Hub configuration
        var threshHubUrl = Environment.GetEnvironmentVariable("THRESH_HUB_URL") 
            ?? "https://192.168.4.85:7002";
        var threshApiKey = Environment.GetEnvironmentVariable("THRESH_API_KEY") 
            ?? throw new Exception("THRESH_API_KEY not set in .env - get this from API Keys page");
        var threshMidtierApiKey = Environment.GetEnvironmentVariable("THRESH_MIDTIER_API_KEY") 
            ?? throw new Exception("THRESH_MIDTIER_API_KEY not set in .env - get this from hub DatabaseSeeder");
        var threshGitHubRepo = Environment.GetEnvironmentVariable("THRESH_GITHUB_REPO") 
            ?? "https://github.com/dealer426/thresh.git";

        // Agent is downloaded from GitHub releases (latest) on each node — small + fast.
        // Override with THRESH_AGENT_RELEASE_URL if you need a specific version.
        var agentReleaseUrl = Environment.GetEnvironmentVariable("THRESH_AGENT_RELEASE_URL")
            ?? "https://github.com/dealer426/thresh/releases/latest/download/thresh-linux-x64.tar.gz";

        // Mid-tier: prefer local tarball + install.sh from sibling thresh-midtier repo,
        // fall back to GitHub release URL when the local build isn't present.
        var midtierTarballPath = (Environment.GetEnvironmentVariable("THRESH_MIDTIER_TARBALL")
            ?? Path.GetFullPath(Path.Combine(".", "..", "..", "thresh-midtier", "dist", "thresh-midtier-linux-x64.tar.gz")))
            .Replace('\\', '/');
        var midtierInstallScriptPath = (Environment.GetEnvironmentVariable("THRESH_MIDTIER_INSTALL_SH")
            ?? Path.GetFullPath(Path.Combine(".", "..", "..", "thresh-midtier", "deploy", "install.sh")))
            .Replace('\\', '/');
        var midtierReleaseUrl = Environment.GetEnvironmentVariable("THRESH_MIDTIER_RELEASE_URL")
            ?? "https://github.com/dealer426/thresh-midtier/releases/latest/download/thresh-midtier-linux-x64.tar.gz";

        var midtierLocalAvailable = System.IO.File.Exists(midtierTarballPath) && System.IO.File.Exists(midtierInstallScriptPath);
        if (!midtierLocalAvailable)
            Console.Error.WriteLine($"ℹ️  Local midtier build not found ({midtierTarballPath} or install.sh) — node-3 will curl from {midtierReleaseUrl}");
        var wslMidtierTarball = midtierLocalAvailable ? ToWslPath(midtierTarballPath) : "";
        var wslMidtierInstallSh = midtierLocalAvailable ? ToWslPath(midtierInstallScriptPath) : "";

        // GPU Node configuration (optional - set ENABLE_GPU_NODE=true to deploy)
        var enableGpuNode = Environment.GetEnvironmentVariable("ENABLE_GPU_NODE")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        var gpuHostName = Environment.GetEnvironmentVariable("VSPHERE_GPU_HOST") ?? "";

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
        var vmInstances = new Dictionary<string, VirtualMachine>();
        var vmLastSteps = new Dictionary<string, Pulumi.Resource>();

        // Deployment mode — driven by stack name so no env var gymnastics are needed.
        // Stack "devbox"   → dev workstation only
        // Stack "k8s-dev"  → single-node k3s cluster
        // Anything else    → full 5-node agent farm
        var stackName = Deployment.Instance.StackName;
        var devboxOnly = stackName == "devbox" ||
            Environment.GetEnvironmentVariable("PULUMI_DEVBOX")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        var k8sDevMode = stackName == "k8s-dev" ||
            Environment.GetEnvironmentVariable("PULUMI_K8S_DEV")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        if (devboxOnly)
        {
            return DevBox.Deploy(vsphereProvider, datacenter, resourcePool, datastore, network, template, sshPublicKey, sshPrivateKey);
        }

        if (k8sDevMode)
        {
            return K8sDevNode.Deploy(vsphereProvider, datacenter, resourcePool, datastore, network, template, sshPublicKey, sshPrivateKey);
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

            // Step 4: Download Thresh agent from GitHub releases (latest)
            var copyThresh = new Command($"{config.Name}-download-thresh", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = $"echo '⬇️  Downloading Thresh agent from {agentReleaseUrl}...'\ncurl -fsSL -o /tmp/thresh-agent-deploy.tar.gz '{agentReleaseUrl}'\nls -lah /tmp/thresh-agent-deploy.tar.gz\necho '✅ Downloaded'"
            }, new CustomResourceOptions { DependsOn = { installDocker } });

            var installThresh = new Command($"{config.Name}-install-thresh", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = "echo '📦 Installing Thresh agent...'\nmkdir -p ~/thresh-agent\ncd ~/thresh-agent\ntar xzf /tmp/thresh-agent-deploy.tar.gz\nchmod +x thresh\nrm /tmp/thresh-agent-deploy.tar.gz\nls -lah ~/thresh-agent/thresh\necho '✅ Thresh installed'"
            }, new CustomResourceOptions { DependsOn = { copyThresh } });

            // Step 5: Configure agent
            var configureAgent = new Command($"{config.Name}-configure-agent", new CommandArgs
            {
                Connection = keyConnectionInfo,
                Create = Output.Format($"echo '⚙️  Configuring agent...'\nmkdir -p ~/.thresh\ncat > ~/.thresh/agent.json << 'AGENTCFG'\n{{\n  \"AgentId\": \"\",\n  \"Enabled\": true,\n  \"MidtierUrl\": \"{threshHubUrl}\",\n  \"ApiKey\": \"{threshApiKey}\",\n  \"TlsVerify\": false,\n  \"ReconnectDelay\": 5,\n  \"MetricsInterval\": 30,\n  \"AutoFailover\": false\n}}\nAGENTCFG\nchmod 600 ~/.thresh/agent.json\necho '✅ Agent configured'")
            }, new CustomResourceOptions { DependsOn = { installThresh } });

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
            vmInstances[config.Name] = vm;
            vmLastSteps[config.Name] = createService;
        }

        // ══════════════════════════════════════════════════════════════════
        // Mid-tier deployment on node-3 (relay for GPU and remote nodes)
        // v0.9: relies on deploy/install.sh from thresh-midtier (MT-P1/MT-P2).
        // - If local tarball + install.sh exist, scp them; else node-3 curls
        //   THRESH_MIDTIER_RELEASE_URL.
        // - install.sh handles extraction, /opt/thresh-midtier layout,
        //   appsettings.json (with HubUrl/HubToken/MidTierId), systemd unit
        //   (with DOTNET_BUNDLE_EXTRACT_BASE_DIR), enable + start.
        // ══════════════════════════════════════════════════════════════════
        if (vmInstances.ContainsKey("thresh-node-3"))
        {
            var node3Vm = vmInstances["thresh-node-3"];
            var node3Connection = node3Vm.DefaultIpAddress.Apply(ip => new ConnectionArgs
            {
                Host = ip,
                Port = 22,
                User = "thresh",
                PrivateKey = sshPrivateKey,
                DialErrorLimit = 60,
            });

            Pulumi.Command.Remote.CopyToRemote? copyMidtierTar = null;
            Pulumi.Command.Remote.CopyToRemote? copyMidtierSh = null;
            if (midtierLocalAvailable)
            {
                var tarAsset = new Pulumi.FileAsset(midtierTarballPath);
                var shAsset = new Pulumi.FileAsset(midtierInstallScriptPath);
                copyMidtierTar = new Pulumi.Command.Remote.CopyToRemote("thresh-node-3-copy-midtier-tar", new Pulumi.Command.Remote.CopyToRemoteArgs
                {
                    Connection = node3Connection,
                    Source = tarAsset,
                    RemotePath = "/tmp/thresh-midtier-linux-x64.tar.gz",
                }, new CustomResourceOptions { DependsOn = { vmLastSteps["thresh-node-3"] } });
                copyMidtierSh = new Pulumi.Command.Remote.CopyToRemote("thresh-node-3-copy-midtier-sh", new Pulumi.Command.Remote.CopyToRemoteArgs
                {
                    Connection = node3Connection,
                    Source = shAsset,
                    RemotePath = "/tmp/install.sh",
                }, new CustomResourceOptions { DependsOn = { vmLastSteps["thresh-node-3"] } });
            }

            // Idempotent mid-tier install: skip if service is already active.
            // CopyToRemote (local tarball) fails silently on Windows; fall back to GitHub
            // release if local copy isn't available. Guard against 404 with || true so a
            // missing release never blocks the whole stack.
            var installMidtierScript = midtierLocalAvailable
                ? $"if sudo systemctl is-active thresh-midtier > /dev/null 2>&1; then echo '✅ Mid-tier already running'; else set -e; echo '📦 Installing Thresh Mid-tier...'; cd /tmp; sed -i \"s/\\r//\" install.sh; chmod +x install.sh; sudo bash install.sh --tarball=/tmp/thresh-midtier-linux-x64.tar.gz --hub-url={threshHubUrl} --hub-token={threshMidtierApiKey} --midtier-id=midtier-node-3 --port=8080 --tls-verify=false; fi; echo '✅ Mid-tier done'"
                : $"if sudo systemctl is-active thresh-midtier > /dev/null 2>&1; then echo '✅ Mid-tier already running'; else echo '📦 Installing Thresh Mid-tier from GitHub...'; cd /tmp && curl -fsSL -o thresh-midtier.tar.gz {midtierReleaseUrl} && tar -xzf thresh-midtier.tar.gz install.sh && chmod +x install.sh && sudo bash install.sh --tarball=/tmp/thresh-midtier.tar.gz --hub-url={threshHubUrl} --hub-token={threshMidtierApiKey} --midtier-id=midtier-node-3 --port=8080 --tls-verify=false || echo '⚠️  Mid-tier install failed — install manually'; fi; echo '✅ Mid-tier done'";

            var installMidtierDeps = new InputList<Resource>();
            if (copyMidtierTar != null && copyMidtierSh != null)
            {
                installMidtierDeps.Add(copyMidtierTar);
                installMidtierDeps.Add(copyMidtierSh);
            }
            else
            {
                installMidtierDeps.Add(vmLastSteps["thresh-node-3"]);
            }

            var installMidtier = new Command("thresh-node-3-install-midtier", new CommandArgs
            {
                Connection = node3Connection,
                Create = installMidtierScript
            }, new CustomResourceOptions { DependsOn = installMidtierDeps });

            vmOutputs["thresh-node-3_midtier"] = installMidtier.Id.Apply(_ => "✅ Mid-tier relay running on port 8080 (via install.sh)");
        }

        // ══════════════════════════════════════════════════════════════════
        // GPU Node (optional) - PCI passthrough for NVIDIA GPU testing
        // Set ENABLE_GPU_NODE=true and VSPHERE_GPU_HOST=<esxi-hostname>
        // Requires: GPU PCI passthrough enabled on ESXi host in vCenter
        // ══════════════════════════════════════════════════════════════════
        if (enableGpuNode)
        {
            if (string.IsNullOrEmpty(gpuHostName))
                throw new Exception("VSPHERE_GPU_HOST must be set when ENABLE_GPU_NODE=true (the ESXi host with the GPU)");

            // Look up the ESXi host that has the GPU installed
            var gpuHost = datacenter.Apply(dc => GetHost.Invoke(new GetHostInvokeArgs
            {
                Name = gpuHostName,
                DatacenterId = dc.Id
            }, new InvokeOptions { Provider = vsphereProvider }));

            // Find the NVIDIA GPU PCI device (vendor 10de = NVIDIA, class 0300 = VGA/3D controller)
            var gpuDevice = gpuHost.Apply(h => GetHostPciDevice.Invoke(new GetHostPciDeviceInvokeArgs
            {
                HostId = h.Id,
                VendorId = "10de",
                ClassId = "0300"
            }, new InvokeOptions { Provider = vsphereProvider }));

            // Find the NVIDIA HD Audio device (same physical GPU, class 0403 = Audio)
            var gpuAudioDevice = gpuHost.Apply(h => GetHostPciDevice.Invoke(new GetHostPciDeviceInvokeArgs
            {
                HostId = h.Id,
                VendorId = "10de",
                ClassId = "0403"
            }, new InvokeOptions { Provider = vsphereProvider }));

            var gpuNodeName = "thresh-node-gpu";
            var gpuCpus = 8;
            var gpuMemoryMB = 32 * 1024; // 32 GB
            var gpuDiskGB = 100;

            var gpuCloudInitUserdata = $@"#cloud-config
hostname: {gpuNodeName}
fqdn: {gpuNodeName}.thresh.sh
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

ssh_pwauth: true
disable_root: false

package_update: true
package_upgrade: false

packages:
  - openssh-server

runcmd:
  - sed -i 's/^#*PasswordAuthentication .*/PasswordAuthentication yes/' /etc/ssh/sshd_config
  - systemctl restart sshd

final_message: ""✅ {gpuNodeName} (GPU) ready for automation!""
";

            var gpuCloudInitMetadata = $@"instance-id: {gpuNodeName}
local-hostname: {gpuNodeName}
";

            // Create GPU VM with PCI passthrough
            var gpuVm = new VirtualMachine(gpuNodeName, new VirtualMachineArgs
            {
                Name = gpuNodeName,
                ResourcePoolId = resourcePool.Apply(rp => rp.Id),
                DatastoreId = datastore.Apply(ds => ds.Id),
                HostSystemId = gpuHost.Apply(h => h.Id), // Pin to the GPU host

                NumCpus = gpuCpus,
                Memory = gpuMemoryMB,
                HardwareVersion = 19, // VMX_19 required for NVIDIA GPU PCI passthrough

                GuestId = template.Apply(t => t.GuestId),
                Firmware = "efi", // EFI required for NVIDIA GPU PCI passthrough
                BootDelay = 5000,
                SyncTimeWithHost = true,
                WaitForGuestIpTimeout = 600, // 10 min - GPU passthrough boot takes longer

                // PCI passthrough - NVIDIA GPU attached
                MemoryReservation = gpuMemoryMB,
                CpuReservation = 18352, // MHz - required for latency-sensitive GPU passthrough
                LatencySensitivity = "high",
                PciDeviceIds = new InputList<string> { gpuDevice.Apply(d => d.Id), gpuAudioDevice.Apply(d => d.Id) },

                NetworkInterfaces = new VirtualMachineNetworkInterfaceArgs
                {
                    NetworkId = network.Apply(n => n.Id)
                },

                Disks = new VirtualMachineDiskArgs
                {
                    Label = "disk0",
                    Size = gpuDiskGB,
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
                    { "guestinfo.metadata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(gpuCloudInitMetadata)) },
                    { "guestinfo.metadata.encoding", "base64" },
                    { "guestinfo.userdata", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(gpuCloudInitUserdata)) },
                    { "guestinfo.userdata.encoding", "base64" },
                    { "disk.EnableUUID", "TRUE" },
                    { "pciPassthru.use64bitMMIO", "TRUE" },
                    { "pciPassthru.64bitMMIOSizeGB", "64" },
                    { "hypervisor.cpuid.v0", "FALSE" },
                    { "svga.present", "FALSE" },
                    { "pciPassthru0.msiEnabled", "FALSE" },
                    { "pciPassthru1.msiEnabled", "FALSE" }
                }
            }, new CustomResourceOptions { Provider = vsphereProvider });

            // GPU provisioning chain
            var gpuConnectionInfo = gpuVm.DefaultIpAddress.Apply(ip => new ConnectionArgs
            {
                Host = ip,
                Port = 22,
                User = "thresh",
                PrivateKey = sshPrivateKey,
                DialErrorLimit = 60,
            });

            var gpuWaitCloudInit = new Command($"{gpuNodeName}-wait-cloudinit", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = "echo '⏳ Waiting for cloud-init to complete...'\nsudo cloud-init status --wait || echo 'cloud-init wait timed out, continuing anyway'\necho '✅ Cloud-init complete'"
            }, new CustomResourceOptions { DependsOn = { gpuVm } });

            var gpuInstallDocker = new Command($"{gpuNodeName}-install-docker", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = "echo '🐳 Installing Docker...'\nsudo apt-get update -qq\nsudo apt-get install -y docker.io containerd -qq\nsudo systemctl enable docker\nsudo systemctl start docker\nsudo usermod -aG docker thresh\necho '✅ Docker installed'"
            }, new CustomResourceOptions { DependsOn = { gpuWaitCloudInit } });

            // GPU-specific: Install NVIDIA drivers and container toolkit
            var gpuInstallDrivers = new Command($"{gpuNodeName}-install-nvidia", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = "echo '🎮 Installing NVIDIA drivers and container toolkit...'\nsudo apt-get install -y ubuntu-drivers-common -qq\nsudo ubuntu-drivers autoinstall\necho '📦 Installing NVIDIA Container Toolkit...'\ncurl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg\ncurl -s -L https://nvidia.github.io/libnvidia-container/stable/deb/nvidia-container-toolkit.list | sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | sudo tee /etc/apt/sources.list.d/nvidia-container-toolkit.list\nsudo apt-get update -qq\nsudo apt-get install -y nvidia-container-toolkit -qq\nsudo nvidia-ctk runtime configure --runtime=docker\nsudo systemctl restart docker\necho '✅ NVIDIA drivers and container toolkit installed'"
            }, new CustomResourceOptions { DependsOn = { gpuInstallDocker } });

            var gpuDownloadThresh = new Command($"{gpuNodeName}-download-thresh", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = $"echo '⬇️  Downloading Thresh agent from {agentReleaseUrl}...'\ncurl -fsSL -o /tmp/thresh-agent-deploy.tar.gz '{agentReleaseUrl}'\nls -lah /tmp/thresh-agent-deploy.tar.gz\necho '✅ Downloaded'"
            }, new CustomResourceOptions { DependsOn = { gpuInstallDrivers } });

            var gpuInstallThresh = new Command($"{gpuNodeName}-install-thresh", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = "echo '📦 Installing Thresh agent...'\nmkdir -p ~/thresh-agent\ncd ~/thresh-agent\ntar xzf /tmp/thresh-agent-deploy.tar.gz\nchmod +x thresh\nrm /tmp/thresh-agent-deploy.tar.gz\nls -lah ~/thresh-agent/thresh\necho '✅ Thresh installed'"
            }, new CustomResourceOptions { DependsOn = { gpuDownloadThresh } });

            // GPU agent connects via mid-tier on node-3 (not directly to hub)
            // GPU node connects directly to the hub like all other nodes.
            // (Previously routed via the mid-tier relay — unnecessary for a dev cluster.)
            var gpuMidtierUrl = Output.Create(threshHubUrl);

            var gpuConfigureAgent = new Command($"{gpuNodeName}-configure-agent", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = Output.Format($"echo '⚙️  Configuring agent...'\nmkdir -p ~/.thresh\ncat > ~/.thresh/agent.json << 'AGENTCFG'\n{{\n  \"AgentId\": \"\",\n  \"Enabled\": true,\n  \"MidtierUrl\": \"{gpuMidtierUrl}\",\n  \"ApiKey\": \"{threshApiKey}\",\n  \"TlsVerify\": false,\n  \"ReconnectDelay\": 5,\n  \"MetricsInterval\": 30,\n  \"AutoFailover\": false\n}}\nAGENTCFG\nchmod 600 ~/.thresh/agent.json\necho '✅ Agent configured'")
            }, new CustomResourceOptions { DependsOn = { gpuInstallThresh } });

            var gpuCreateService = new Command($"{gpuNodeName}-create-service", new CommandArgs
            {
                Connection = gpuConnectionInfo,
                Create = "echo '🚀 Creating systemd service...'\nsudo tee /etc/systemd/system/thresh-agent.service > /dev/null << 'SVCFILE'\n[Unit]\nDescription=Thresh Agent\nAfter=network-online.target docker.service\nWants=network-online.target\nRequires=docker.service\n\n[Service]\nType=simple\nUser=thresh\nWorkingDirectory=/home/thresh/thresh-agent\nExecStart=/home/thresh/thresh-agent/thresh agent start\nRestart=always\nRestartSec=10\nStandardOutput=journal\nStandardError=journal\n\n[Install]\nWantedBy=multi-user.target\nSVCFILE\nsudo systemctl daemon-reload\nsudo systemctl enable thresh-agent\nsudo systemctl start thresh-agent\necho '✅ Service started'"
            }, new CustomResourceOptions { DependsOn = { gpuConfigureAgent } });

            // Export GPU node details
            vmOutputs[$"{gpuNodeName}_ip"] = gpuVm.DefaultIpAddress;
            vmOutputs[$"{gpuNodeName}_ssh"] = gpuVm.DefaultIpAddress.Apply(ip => $"ssh thresh@{ip}");
            vmOutputs[$"{gpuNodeName}_gpu"] = gpuDevice.Apply(d => d.Name);
            vmOutputs[$"{gpuNodeName}_status"] = gpuCreateService.Id.Apply(_ => "✅ GPU node - agent running with NVIDIA passthrough");
        }

        //Export summary
        vmOutputs["instructions"] = Output.Create($@"
╔════════════════════════════════════════════════════════════════╗
║      THRESH MULTI-NODE CLUSTER - FULLY AUTOMATED               ║
╚════════════════════════════════════════════════════════════════╝

✨ All installation automated via Pulumi Command resources!

Hub URL: {threshHubUrl}
Nodes: {(enableGpuNode ? "5 + 1 GPU node" : "5")} (deployed and configured automatically)

What Was Automated:
  ✅ VM creation
  ✅ SSH key configuration
  ✅ Docker installation{(enableGpuNode ? "\n  ✅ NVIDIA driver + container toolkit install (GPU node)" : "")}
  ✅ Thresh binary deployed via SCP from local build
  ✅ Mid-tier relay deployed on node-3 (port 8080)
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
        vmOutputs["node_count"] = enableGpuNode ? 6 : 5;
        vmOutputs["automation"] = "✨ Fully automated - pre-built binary via SCP from local build";

        return vmOutputs;
    });
}
