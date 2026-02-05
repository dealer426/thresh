namespace EknovaCli.Services;

/// <summary>
/// Registry of supported Linux distributions and their rootfs download URLs
/// </summary>
public class RootfsRegistry
{
    public enum PackageManager
    {
        Apt,
        Apk,
        Dnf,
        Yum,
        Pacman,
        Zypper
    }

    public enum DistributionSource
    {
        Vendor,      // Direct download from vendor (Ubuntu cloud images, Alpine, etc.)
        MicrosoftStore  // Installed via wsl --install from MS Store
    }

    public class DistributionInfo
    {
        public string Name { get; }
        public string Version { get; }
        public string RootfsUrl { get; }
        public PackageManager PackageManager { get; }
        public string UpdateCommand { get; }
        public string InstallCommand { get; }
        public DistributionSource Source { get; }
        public string? WslInstallName { get; }  // Name used for 'wsl --install <name>'

        public DistributionInfo(string name, string version, string rootfsUrl, PackageManager packageManager, DistributionSource source = DistributionSource.Vendor, string? wslInstallName = null)
        {
            Name = name;
            Version = version;
            RootfsUrl = rootfsUrl;
            PackageManager = packageManager;
            Source = source;
            WslInstallName = wslInstallName;

            // Build commands based on package manager
            (UpdateCommand, InstallCommand) = packageManager switch
            {
                PackageManager.Apt => ("apt-get update -qq", "apt-get install -y -qq"),
                PackageManager.Apk => ("apk update -q", "apk add --no-cache"),
                PackageManager.Dnf => ("dnf check-update -q || true", "dnf install -y -q"),
                PackageManager.Yum => ("yum check-update -q || true", "yum install -y -q"),
                PackageManager.Pacman => ("pacman -Sy --noconfirm", "pacman -S --noconfirm"),
                PackageManager.Zypper => ("zypper refresh -q", "zypper install -y"),
                _ => (string.Empty, string.Empty)
            };
        }

        public string GetFullName() => $"{Name}-{Version}";
    }

    private readonly Dictionary<string, DistributionInfo> _distributions = new();
    private readonly ConfigurationService? _configService;

    public RootfsRegistry(ConfigurationService? configService = null)
    {
        _configService = configService;
        RegisterDistributions();
        LoadCustomDistributions();
    }

    private void RegisterDistributions()
    {
        // Ubuntu distributions
        _distributions["ubuntu-22.04"] = new DistributionInfo(
            "ubuntu",
            "22.04",
            "https://cloud-images.ubuntu.com/wsl/jammy/current/ubuntu-jammy-wsl-amd64-ubuntu22.04lts.rootfs.tar.gz",
            PackageManager.Apt
        );

        _distributions["ubuntu-24.04"] = new DistributionInfo(
            "ubuntu",
            "24.04",
            "https://cloud-images.ubuntu.com/wsl/noble/current/ubuntu-noble-wsl-amd64-ubuntu24.04lts.rootfs.tar.gz",
            PackageManager.Apt
        );

        _distributions["ubuntu-20.04"] = new DistributionInfo(
            "ubuntu",
            "20.04",
            "https://cloud-images.ubuntu.com/wsl/focal/current/ubuntu-focal-wsl-amd64-ubuntu20.04lts.rootfs.tar.gz",
            PackageManager.Apt
        );

        // Alpine Linux distributions (very lightweight!)
        _distributions["alpine-3.19"] = new DistributionInfo(
            "alpine",
            "3.19",
            "https://dl-cdn.alpinelinux.org/alpine/v3.19/releases/x86_64/alpine-minirootfs-3.19.0-x86_64.tar.gz",
            PackageManager.Apk
        );

        _distributions["alpine-3.18"] = new DistributionInfo(
            "alpine",
            "3.18",
            "https://dl-cdn.alpinelinux.org/alpine/v3.18/releases/x86_64/alpine-minirootfs-3.18.5-x86_64.tar.gz",
            PackageManager.Apk
        );

        _distributions["alpine-edge"] = new DistributionInfo(
            "alpine",
            "edge",
            "https://dl-cdn.alpinelinux.org/alpine/edge/releases/x86_64/alpine-minirootfs-edge-x86_64.tar.gz",
            PackageManager.Apk
        );

        // Debian distributions
        _distributions["debian-12"] = new DistributionInfo(
            "debian",
            "12",
            "https://github.com/debuerreotype/docker-debian-artifacts/raw/dist-amd64/bookworm/rootfs.tar.xz",
            PackageManager.Apt,
            DistributionSource.Vendor
        );

        _distributions["debian-11"] = new DistributionInfo(
            "debian",
            "11",
            "https://github.com/debuerreotype/docker-debian-artifacts/raw/dist-amd64/bullseye/rootfs.tar.xz",
            PackageManager.Apt,
            DistributionSource.Vendor
        );

        // Microsoft Store distributions (installed via wsl --install)
        _distributions["kali"] = new DistributionInfo(
            "kali",
            "rolling",
            string.Empty,  // No direct URL, uses wsl --install
            PackageManager.Apt,
            DistributionSource.MicrosoftStore,
            "kali-linux"
        );

        _distributions["oracle-9"] = new DistributionInfo(
            "oracle",
            "9.5",
            string.Empty,
            PackageManager.Dnf,
            DistributionSource.MicrosoftStore,
            "OracleLinux_9_5"
        );

        _distributions["oracle-8"] = new DistributionInfo(
            "oracle",
            "8.10",
            string.Empty,
            PackageManager.Dnf,
            DistributionSource.MicrosoftStore,
            "OracleLinux_8_10"
        );

        _distributions["opensuse-leap"] = new DistributionInfo(
            "opensuse",
            "15.6",
            string.Empty,
            PackageManager.Zypper,
            DistributionSource.MicrosoftStore,
            "openSUSE-Leap-15.6"
        );

        _distributions["opensuse-tumbleweed"] = new DistributionInfo(
            "opensuse",
            "tumbleweed",
            string.Empty,
            PackageManager.Zypper,
            DistributionSource.MicrosoftStore,
            "openSUSE-Tumbleweed"
        );
    }

    private void LoadCustomDistributions()
    {
        if (_configService == null)
            return;

        var settings = _configService.Load();
        foreach (var (key, customDistro) in settings.CustomDistributions)
        {
            var packageManager = customDistro.PackageManager.ToLowerInvariant() switch
            {
                "apt" or "apt-get" => PackageManager.Apt,
                "apk" => PackageManager.Apk,
                "dnf" => PackageManager.Dnf,
                "yum" => PackageManager.Yum,
                "pacman" => PackageManager.Pacman,
                "zypper" => PackageManager.Zypper,
                _ => PackageManager.Apt
            };

            _distributions[key] = new DistributionInfo(
                customDistro.Name,
                customDistro.Version,
                customDistro.RootfsUrl,
                packageManager
            );
        }
    }

    /// <summary>
    /// Get distribution info by key (e.g., "ubuntu-22.04")
    /// </summary>
    public DistributionInfo? GetDistribution(string key)
    {
        _distributions.TryGetValue(key.ToLowerInvariant(), out var info);
        return info;
    }

    /// <summary>
    /// Check if a distribution is supported
    /// </summary>
    public bool IsSupported(string key)
    {
        return _distributions.ContainsKey(key.ToLowerInvariant());
    }

    /// <summary>
    /// Get all supported distribution keys
    /// </summary>
    public string[] GetSupportedDistributions()
    {
        return _distributions.Keys.ToArray();
    }

    /// <summary>
    /// Parse a base string (e.g., "Ubuntu-22.04" or "ubuntu-22.04") to normalized key
    /// </summary>
    public string NormalizeDistributionKey(string baseKey)
    {
        return baseKey.ToLowerInvariant().Trim();
    }

    /// <summary>
    /// Get the cache filename for a distribution
    /// </summary>
    public string GetCacheFilename(string distributionKey)
    {
        var info = GetDistribution(distributionKey);
        if (info == null)
            return $"{distributionKey}.tar.gz";

        // Use .tar.xz for Debian, .tar.gz for others
        var extension = info.RootfsUrl.EndsWith(".tar.xz") ? ".tar.xz" : ".tar.gz";
        return $"{distributionKey}{extension}";
    }
}
