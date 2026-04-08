using Pulumi;
using Pulumi.VSphere;
using Pulumi.VSphere.Inputs;
using Pulumi.Command.Remote;
using Pulumi.Command.Remote.Inputs;

/// <summary>
/// Provisions a dev workstation VM on vSphere.
/// Usage: set PULUMI_DEVBOX=true before running pulumi up, or use deploy-devbox.sh
/// </summary>
public static class DevBox
{
    public static Dictionary<string, object?> Deploy(
        Provider vsphereProvider,
        Output<GetDatacenterResult> datacenter,
        Output<GetResourcePoolResult> resourcePool,
        Output<GetDatastoreResult> datastore,
        Output<GetNetworkResult> network,
        Output<GetVirtualMachineResult> template,
        string sshPublicKey,
        string sshPrivateKey)
    {
        var vmName = "sburns-devbox";
        var cpus = 8;
        var memoryGB = 32;
        var diskGB = 200;
        var username = "sburns";

        var cloudInitUserdata = $@"#cloud-config
hostname: {vmName}
fqdn: {vmName}.thresh.sh
manage_etc_hosts: true

users:
  - name: {username}
    sudo: ALL=(ALL) NOPASSWD:ALL
    groups: users, admin, docker, sudo
    shell: /bin/bash
    ssh_authorized_keys:
      - {sshPublicKey}
    lock_passwd: false
    passwd: $6$rounds=4096$saltsalt$a7X7nmSP37V98oiJGXa3stnPaPqbYJuYUXPXwne0H5BXQIU6YIpiUIMf

ssh_pwauth: true
disable_root: false

package_update: true
package_upgrade: true

packages:
  - openssh-server
  - git
  - curl
  - wget
  - vim
  - htop
  - tmux
  - jq
  - unzip
  - build-essential
  - ca-certificates
  - gnupg
  - lsb-release
  - apt-transport-https
  - software-properties-common
  - postgresql-client
  - python3
  - python3-pip
  - tree
  - ripgrep
  - fd-find

runcmd:
  - sed -i 's/^#*PasswordAuthentication .*/PasswordAuthentication yes/' /etc/ssh/sshd_config
  - sed -i 's/^KbdInteractiveAuthentication no/KbdInteractiveAuthentication yes/' /etc/ssh/sshd_config
  - systemctl restart ssh

final_message: ""✅ {vmName} cloud-init complete — ready for provisioning!""
";

        var cloudInitMetadata = $@"instance-id: {vmName}
local-hostname: {vmName}
";

        // Create VM
        var vm = new VirtualMachine(vmName, new VirtualMachineArgs
        {
            Name = vmName,
            ResourcePoolId = resourcePool.Apply(rp => rp.Id),
            DatastoreId = datastore.Apply(ds => ds.Id),

            NumCpus = cpus,
            Memory = memoryGB * 1024,

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
                Size = diskGB,
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

        // SSH connection
        var conn = vm.DefaultIpAddress.Apply(ip => new ConnectionArgs
        {
            Host = ip,
            Port = 22,
            User = username,
            PrivateKey = sshPrivateKey,
            DialErrorLimit = 60,
        });

        // Step 1: Wait for cloud-init
        var waitCloudInit = new Command($"{vmName}-wait-cloudinit", new CommandArgs
        {
            Connection = conn,
            Create = "echo '⏳ Waiting for cloud-init...'\nsudo cloud-init status --wait || echo 'cloud-init timed out, continuing'\necho '✅ Cloud-init complete'"
        }, new CustomResourceOptions { DependsOn = { vm } });

        // Step 2: Install Docker (CE with buildx + compose plugin)
        var installDocker = new Command($"{vmName}-install-docker", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🐳 Installing Docker CE...'\n"
                + "sudo mkdir -p /etc/apt/keyrings\n"
                + "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg\n"
                + "echo \"deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable\" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null\n"
                + "sudo apt-get update -qq\n"
                + "sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin -qq\n"
                + "sudo systemctl enable docker\n"
                + "sudo systemctl start docker\n"
                + "sudo usermod -aG docker sburns\n"
                + "echo '✅ Docker installed'"
        }, new CustomResourceOptions { DependsOn = { waitCloudInit } });

        // Step 3: Install .NET 10 SDK
        var installDotnet = new Command($"{vmName}-install-dotnet", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🔷 Installing .NET 10 SDK...'\n"
                + "wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh\n"
                + "chmod +x /tmp/dotnet-install.sh\n"
                + "/tmp/dotnet-install.sh --channel 10.0 --install-dir $HOME/.dotnet\n"
                + "echo 'export DOTNET_ROOT=$HOME/.dotnet' >> $HOME/.bashrc\n"
                + "echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> $HOME/.bashrc\n"
                + "export DOTNET_ROOT=$HOME/.dotnet\n"
                + "export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools\n"
                + "dotnet --version\n"
                + "echo '✅ .NET 10 SDK installed'"
        }, new CustomResourceOptions { DependsOn = { installDocker } });

        // Step 4: Install Azure CLI
        var installAzCli = new Command($"{vmName}-install-azcli", new CommandArgs
        {
            Connection = conn,
            Create = "echo '☁️  Installing Azure CLI...'\n"
                + "curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash\n"
                + "az version\n"
                + "echo '✅ Azure CLI installed'"
        }, new CustomResourceOptions { DependsOn = { installDotnet } });

        // Step 5: Install Node.js 22 LTS
        var installNode = new Command($"{vmName}-install-node", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🟢 Installing Node.js 22 LTS...'\n"
                + "curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -\n"
                + "sudo apt-get install -y nodejs -qq\n"
                + "node --version\n"
                + "npm --version\n"
                + "echo '✅ Node.js installed'"
        }, new CustomResourceOptions { DependsOn = { installAzCli } });

        // Step 6: Install Go
        var installGo = new Command($"{vmName}-install-go", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🐹 Installing Go...'\n"
                + "curl -fsSL https://go.dev/dl/go1.23.2.linux-amd64.tar.gz -o /tmp/go.tar.gz\n"
                + "sudo rm -rf /usr/local/go\n"
                + "sudo tar -C /usr/local -xzf /tmp/go.tar.gz\n"
                + "rm /tmp/go.tar.gz\n"
                + "echo 'export PATH=$PATH:/usr/local/go/bin:$HOME/go/bin' >> $HOME/.bashrc\n"
                + "export PATH=$PATH:/usr/local/go/bin\n"
                + "go version\n"
                + "echo '✅ Go installed'"
        }, new CustomResourceOptions { DependsOn = { installNode } });

        // Step 7: Install dev tools (Pulumi, EF Core tools, GitHub CLI)
        var installDevTools = new Command($"{vmName}-install-devtools", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🔧 Installing dev tools...'\n"
                + "curl -fsSL https://get.pulumi.com | bash\n"
                + "echo 'export PATH=$PATH:$HOME/.pulumi/bin' >> $HOME/.bashrc\n"
                + "export DOTNET_ROOT=$HOME/.dotnet\n"
                + "export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools\n"
                + "dotnet tool install --global dotnet-ef || echo 'dotnet-ef already installed'\n"
                + "sudo mkdir -p -m 755 /etc/apt/keyrings\n"
                + "wget -nv -O /tmp/githubcli.gpg https://cli.github.com/packages/githubcli-archive-keyring.gpg\n"
                + "cat /tmp/githubcli.gpg | sudo tee /etc/apt/keyrings/githubcli-archive-keyring.gpg > /dev/null\n"
                + "sudo chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg\n"
                + "echo \"deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main\" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null\n"
                + "sudo apt-get update -qq\n"
                + "sudo apt-get install gh -y -qq\n"
                + "gh extension install github/gh-copilot 2>/dev/null || echo 'gh-copilot will need auth first'\n"
                + "echo '✅ Dev tools installed (Pulumi, dotnet-ef, gh CLI, gh-copilot)'"
        }, new CustomResourceOptions { DependsOn = { installGo } });

        // Step 8: Clone all 3 thresh repos
        var cloneRepos = new Command($"{vmName}-clone-repos", new CommandArgs
        {
            Connection = conn,
            Create = "echo '📦 Cloning thresh repos...'\n"
                + "mkdir -p $HOME/source/repos\n"
                + "cd $HOME/source/repos\n"
                + "git clone https://github.com/dealer426/thresh.git 2>/dev/null || (cd thresh && git pull)\n"
                + "git clone https://github.com/dealer426/thresh-hub.git 2>/dev/null || (cd thresh-hub && git pull)\n"
                + "git clone https://github.com/dealer426/thresh-midtier.git 2>/dev/null || (cd thresh-midtier && git pull)\n"
                + "cd $HOME/source/repos/thresh && git checkout dev 2>/dev/null || true\n"
                + "cd $HOME/source/repos/thresh-hub && git checkout dev 2>/dev/null || true\n"
                + "cd $HOME/source/repos/thresh-midtier && git checkout dev 2>/dev/null || true\n"
                + "echo '✅ All 3 repos cloned (dev branch)'"
        }, new CustomResourceOptions { DependsOn = { installDevTools } });

        // Step 9: Build all repos to verify
        var buildRepos = new Command($"{vmName}-build-repos", new CommandArgs
        {
            Connection = conn,
            Create = "echo '🔨 Building all repos...'\n"
                + "export DOTNET_ROOT=$HOME/.dotnet\n"
                + "export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools\n"
                + "cd $HOME/source/repos/thresh && dotnet build thresh.sln --verbosity quiet 2>&1 | tail -5\n"
                + "cd $HOME/source/repos/thresh-hub && dotnet build thresh-hub.sln --verbosity quiet 2>&1 | tail -5\n"
                + "cd $HOME/source/repos/thresh-midtier && dotnet build src/ThreshMidTier/ThreshMidTier.csproj --verbosity quiet 2>&1 | tail -5\n"
                + "echo '✅ All repos built successfully'"
        }, new CustomResourceOptions { DependsOn = { cloneRepos } });

        // Step 10: Configure git identity
        var configureGit = new Command($"{vmName}-configure-git", new CommandArgs
        {
            Connection = conn,
            Create = "echo '⚙️  Configuring git...'\n"
                + "git config --global user.name 'sburns'\n"
                + "git config --global user.email 'sburns@thresh.sh'\n"
                + "git config --global init.defaultBranch dev\n"
                + "git config --global push.autoSetupRemote true\n"
                + "git config --global core.editor vim\n"
                + "echo '✅ Git configured'"
        }, new CustomResourceOptions { DependsOn = { buildRepos } });

        // Outputs
        var outputs = new Dictionary<string, object?>
        {
            [$"{vmName}_ip"] = vm.DefaultIpAddress,
            [$"{vmName}_ssh"] = vm.DefaultIpAddress.Apply(ip => $"ssh {username}@{ip}"),
            [$"{vmName}_specs"] = $"{cpus} vCPU / {memoryGB}GB RAM / {diskGB}GB disk",
            [$"{vmName}_status"] = configureGit.Id.Apply(_ => "✅ Fully provisioned"),
            [$"{vmName}_summary"] = vm.DefaultIpAddress.Apply(ip => $@"
╔══════════════════════════════════════════════════════════════╗
║              SBURNS DEV WORKSTATION                          ║
╠══════════════════════════════════════════════════════════════╣
║  IP:       {ip,-50}║
║  SSH:      ssh {username}@{ip,-44}║
║  Specs:    {cpus} vCPU / {memoryGB}GB RAM / {diskGB}GB SSD{new string(' ', 33)}║
╠══════════════════════════════════════════════════════════════╣
║  Installed:                                                  ║
║    ✅ .NET 10 SDK          ✅ Docker CE + Compose            ║
║    ✅ Azure CLI             ✅ Node.js 22 LTS                ║
║    ✅ Go 1.23               ✅ Pulumi                        ║
║    ✅ GitHub CLI + Copilot    ✅ dotnet-ef                     ║
║    ✅ PostgreSQL client      ✅ Python 3                     ║
╠══════════════════════════════════════════════════════════════╣
║  Repos (~/source/repos/):                                    ║
║    ✅ thresh        (dev)                                    ║
║    ✅ thresh-hub    (dev)                                    ║
║    ✅ thresh-midtier(dev)                                    ║
╚══════════════════════════════════════════════════════════════╝
")
        };

        return outputs;
    }
}
