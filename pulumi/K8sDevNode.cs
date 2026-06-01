using System.Text;
using Pulumi;
using Pulumi.VSphere;
using Pulumi.VSphere.Inputs;
using Pulumi.Command.Remote;
using Pulumi.Command.Remote.Inputs;

/// <summary>
/// Provisions a single-node k3s cluster on vSphere for K8s readiness testing.
/// Usage: pulumi up --stack k8s-dev  (stack name drives mode automatically)
///
/// What gets deployed:
///   - 1 Ubuntu 24.04 VM (4 vCPU / 12 GB RAM / 80 GB disk)
///   - k3s with Traefik enabled (standard k3s default)
///   - thresh namespace + thresh-hub-secrets secret
///   - Redis 7 (NodePort 30379)
///   - PostgreSQL 16 (NodePort 30432, password: thresh)
///   - Traefik IngressRoute for thresh-hub (HTTP, no TLS — dev only)
///   - PodDisruptionBudget
///   - kubeconfig written to ~/.kube/config on the local machine
///
/// After provisioning, push the thresh-hub Docker image and then apply:
///   kubectl apply -f deploy/k8s/statefulset.yaml
/// </summary>
public static class K8sDevNode
{
    // Postgres password matches deploy/k8s/secrets.yaml
    private const string PostgresPassword = "thresh";

    private const string RedisManifest = @"apiVersion: v1
kind: Namespace
metadata:
  name: thresh
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
  namespace: thresh
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
      - name: redis
        image: redis:7-alpine
        ports:
        - containerPort: 6379
---
apiVersion: v1
kind: Service
metadata:
  name: redis
  namespace: thresh
spec:
  type: NodePort
  selector:
    app: redis
  ports:
  - port: 6379
    targetPort: 6379
    nodePort: 30379";

    private const string PostgresManifest = @"apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: thresh
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16-alpine
        env:
        - name: POSTGRES_DB
          value: threshhub
        - name: POSTGRES_USER
          value: hubuser
        - name: POSTGRES_PASSWORD
          value: thresh
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: data
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: data
        emptyDir: {}
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: thresh
spec:
  type: NodePort
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
    nodePort: 30432";

    // Kubernetes Secret matching deploy/k8s/secrets.yaml values.
    private const string SecretsManifest = @"apiVersion: v1
kind: Secret
metadata:
  name: thresh-hub-secrets
  namespace: thresh
type: Opaque
stringData:
  postgres-connection: ""Host=postgres;Port=5432;Database=threshhub;Username=hubuser;Password=thresh""
  redis-connection: ""redis:6379""";

    // PodDisruptionBudget — applied now so it is in place when the StatefulSet lands.
    private const string PdbManifest = @"apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: thresh-hub-pdb
  namespace: thresh
spec:
  maxUnavailable: 1
  selector:
    matchLabels:
      app: thresh-hub";

    // Dev IngressRoute — HTTP only, no TLS, catches all traffic on port 80.
    // Access thresh-hub at http://<node-ip>/ after applying statefulset.yaml.
    private const string DevIngressManifest = @"apiVersion: traefik.io/v1alpha1
kind: IngressRoute
metadata:
  name: thresh-hub-dev
  namespace: thresh
spec:
  entryPoints:
    - web
  routes:
    - match: PathPrefix(`/`)
      kind: Rule
      services:
        - name: thresh-hub
          port: 7200
          sticky:
            cookie:
              name: thresh-affinity
              httpOnly: true";

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
        const string nodeName = "thresh-k8s-dev";

        var cloudInitMetadata = $@"instance-id: {nodeName}
local-hostname: {nodeName}
";

        var cloudInitUserdata = $@"#cloud-config
hostname: {nodeName}
fqdn: {nodeName}.thresh.sh
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
  - curl

runcmd:
  - sed -i 's/^#*PasswordAuthentication .*/PasswordAuthentication yes/' /etc/ssh/sshd_config
  - systemctl restart sshd

final_message: ""✅ {nodeName} ready for k3s!""
";

        var vm = new VirtualMachine(nodeName, new VirtualMachineArgs
        {
            Name = nodeName,
            ResourcePoolId = resourcePool.Apply(rp => rp.Id),
            DatastoreId = datastore.Apply(ds => ds.Id),
            NumCpus = 4,
            Memory = 12 * 1024,
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
                Size = 80,
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
                { "guestinfo.metadata", Convert.ToBase64String(Encoding.UTF8.GetBytes(cloudInitMetadata)) },
                { "guestinfo.metadata.encoding", "base64" },
                { "guestinfo.userdata", Convert.ToBase64String(Encoding.UTF8.GetBytes(cloudInitUserdata)) },
                { "guestinfo.userdata.encoding", "base64" },
                { "disk.EnableUUID", "TRUE" }
            }
        }, new CustomResourceOptions { Provider = vsphereProvider });

        var conn = vm.DefaultIpAddress.Apply(ip => new ConnectionArgs
        {
            Host = ip,
            Port = 22,
            User = "thresh",
            PrivateKey = sshPrivateKey,
            DialErrorLimit = 60,
        });

        // 1. Wait for cloud-init
        var waitCloudInit = new Command("k8s-dev-wait-cloudinit", new CommandArgs
        {
            Connection = conn,
            Create = "sudo cloud-init status --wait || echo 'cloud-init timed out, continuing'"
        }, new CustomResourceOptions { DependsOn = { vm } });

        // 2. Install k3s with Traefik enabled (standard default — no --disable traefik).
        //    Wait until both the node and Traefik pods are Ready before proceeding.
        var installK3s = new Command("k8s-dev-install-k3s", new CommandArgs
        {
            Connection = conn,
            Create =
                "curl -sfL https://get.k3s.io | sh - && " +
                "timeout 180 bash -c 'until sudo k3s kubectl get nodes 2>/dev/null | grep -q \" Ready\"; do sleep 3; done' && " +
                "timeout 180 bash -c 'until sudo k3s kubectl -n kube-system get pods -l app.kubernetes.io/name=traefik 2>/dev/null | grep -q Running; do sleep 5; done' && " +
                "echo '✅ k3s + Traefik ready'"
        }, new CustomResourceOptions { DependsOn = { waitCloudInit } });

        // 3a. Create GHCR imagePullSecret so k3s can pull ghcr.io/dealer426/thresh-hub.
        //     Token read from GHCR_TOKEN in .env — never hard-coded.
        var ghcrToken = Environment.GetEnvironmentVariable("GHCR_TOKEN") ?? "";
        var createPullSecret = new Command("k8s-dev-ghcr-secret", new CommandArgs
        {
            Connection = conn,
            Create = string.IsNullOrEmpty(ghcrToken)
                ? "echo 'GHCR_TOKEN not set — skipping imagePullSecret (set in .env for auto-creation)'"
                : $"sudo k3s kubectl create secret docker-registry ghcr-pull-secret " +
                  $"--docker-server=ghcr.io --docker-username=dealer426 " +
                  $"--docker-password={ghcrToken} --docker-email=burnssamuel01@gmail.com " +
                  $"-n thresh --dry-run=client -o yaml | sudo k3s kubectl apply -f - && " +
                  $"echo '✅ ghcr-pull-secret created'"
        }, new CustomResourceOptions { DependsOn = { installK3s } });

        // 3b. Apply all base manifests: namespace, redis, postgres, secrets, PDB, dev ingress
        var deployManifests = new Command("k8s-dev-deploy-manifests", new CommandArgs
        {
            Connection = conn,
            Create =
                $"cat <<'MANIFEST_EOF' | sudo k3s kubectl apply -f -\n{RedisManifest}\nMANIFEST_EOF\n" +
                $"cat <<'MANIFEST_EOF' | sudo k3s kubectl apply -f -\n{PostgresManifest}\nMANIFEST_EOF\n" +
                $"cat <<'MANIFEST_EOF' | sudo k3s kubectl apply -f -\n{SecretsManifest}\nMANIFEST_EOF\n" +
                $"cat <<'MANIFEST_EOF' | sudo k3s kubectl apply -f -\n{PdbManifest}\nMANIFEST_EOF\n" +
                $"cat <<'MANIFEST_EOF' | sudo k3s kubectl apply -f -\n{DevIngressManifest}\nMANIFEST_EOF\n" +
                "echo '✅ Base manifests applied'"
        }, new CustomResourceOptions { DependsOn = { createPullSecret } });

        // 4. Export kubeconfig with node IP substituted for 127.0.0.1
        var exportKubeconfig = new Command("k8s-dev-export-kubeconfig", new CommandArgs
        {
            Connection = conn,
            Create = vm.DefaultIpAddress.Apply(ip =>
                $"sudo cat /etc/rancher/k3s/k3s.yaml | sed 's/127.0.0.1/{ip}/g' > /home/thresh/kubeconfig.yaml && " +
                "chmod 644 /home/thresh/kubeconfig.yaml && " +
                "echo '✅ kubeconfig exported'"
            )
        }, new CustomResourceOptions { DependsOn = { deployManifests } });

        // 5. Pull kubeconfig to Windows ~/.kube/config via base64 to avoid escaping issues
        var getKubeconfigB64 = new Command("k8s-dev-get-kubeconfig-b64", new CommandArgs
        {
            Connection = conn,
            Create = "base64 -w0 /home/thresh/kubeconfig.yaml"
        }, new CustomResourceOptions { DependsOn = { exportKubeconfig } });

        var kubeconfigLocalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
        var kubeconfigLocalDir = Path.GetDirectoryName(kubeconfigLocalPath)!;
        var psPath = kubeconfigLocalPath.Replace("\\", "\\\\");
        var psDir  = kubeconfigLocalDir.Replace("\\", "\\\\");

        var writeKubeconfig = new Pulumi.Command.Local.Command("k8s-dev-write-kubeconfig",
            new Pulumi.Command.Local.CommandArgs
            {
                Create = getKubeconfigB64.Stdout.Apply(b64 =>
                    $"New-Item -ItemType Directory -Force -Path '{psDir}' | Out-Null; " +
                    $"[System.IO.File]::WriteAllBytes('{psPath}', " +
                    $"[System.Convert]::FromBase64String('{b64.Trim()}'))"
                ),
                Interpreter = new InputList<string> { "pwsh", "-Command" }
            }, new CustomResourceOptions { DependsOn = { getKubeconfigB64 } });

        var outputs = new Dictionary<string, object?>();
        outputs["k8s_dev_ip"]         = vm.DefaultIpAddress;
        outputs["k8s_dev_ssh"]        = vm.DefaultIpAddress.Apply(ip => $"ssh thresh@{ip}");
        outputs["k8s_dev_kubeconfig"] = kubeconfigLocalPath;
        outputs["k8s_dev_hub_url"]    = vm.DefaultIpAddress.Apply(ip => $"http://{ip}");
        outputs["k8s_dev_status"]     = writeKubeconfig.Id.Apply(_ =>
            "✅ k3s + Traefik running — base manifests applied — kubeconfig written");

        outputs["k8s_dev_redis_cs"]    = vm.DefaultIpAddress.Apply(ip => $"{ip}:30379");
        outputs["k8s_dev_postgres_cs"] = vm.DefaultIpAddress.Apply(ip =>
            $"Host={ip};Port=30432;Database=threshhub;Username=hubuser;Password={PostgresPassword}");

        outputs["k8s_dev_instructions"] = vm.DefaultIpAddress.Apply(ip => $@"
╔══════════════════════════════════════════════════════════════╗
║                thresh K8s Dev Cluster                       ║
╚══════════════════════════════════════════════════════════════╝

Node IP  : {ip}
Hub URL  : http://{ip}   (Traefik IngressRoute, HTTP)
SSH      : ssh thresh@{ip}
kubectl  : kubectl get nodes

In-cluster services:
  Redis    : {ip}:30379
  Postgres : {ip}:30432  (db=threshhub user=hubuser pass={PostgresPassword})

Next step — deploy thresh-hub:
  1. Push image:    docker push ghcr.io/dealer426/thresh-hub:latest
  2. Apply:         kubectl apply -f deploy/k8s/statefulset.yaml

Verify:
  kubectl get pods -n thresh
  kubectl logs -n thresh -l app=thresh-hub

Destroy:
  pulumi destroy --yes --stack k8s-dev
");

        return outputs;
    }
}
