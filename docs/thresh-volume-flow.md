# How Thresh Implements Volume Mounting

## The Complete Execution Flow

### Step 1: Blueprint Definition
```json
{
  "name": "postgres-dev",
  "base": "ubuntu-22.04",
  "ports": ["5432:5432"],
  "volumes": [
    {
      "name": "postgres-data",
      "mount": "/var/lib/postgresql/data"
    }
  ]
}
```

### Step 2: Blueprint Parsing
When you run `thresh up postgres-dev`, the blueprint is loaded and deserialized:

```csharp
// Location: Services/BlueprintService.cs
var blueprint = JsonSerializer.Deserialize(
    jsonContent, 
    BlueprintJsonContext.Default.Blueprint
);
```

Creates a `Blueprint` object with:
- `Name = "postgres-dev"`
- `Base = "ubuntu-22.04"`
- `Ports = ["5432:5432"]`
- `Volumes = [{ Name="postgres-data", Mount="/var/lib/postgresql/data" }]`

### Step 3: ImportEnvironmentAsync
The provisioning service calls `ContainerdService.ImportEnvironmentAsync()`:

```csharp
public async Task<bool> ImportEnvironmentAsync(
    string environmentName,      // "postgres-dev"
    string sourcePath,            // "ubuntu:22.04"
    string installPath,           // (ignored for Docker)
    string? blueprintName,        // "postgres-dev"
    Blueprint? blueprint          // The parsed blueprint object
)
{
    var containerName = ThreshPrefix + environmentName;  // "thresh-postgres-dev"
    var tool = await GetAvailableToolAsync();             // "docker"
    
    List<string> createArgs = new();
    
    // Start building the docker create command
    createArgs.Add(tool);                                 // "docker"
    createArgs.AddRange(new[] { "create", "--name", containerName, "-it" });
    
    // Add blueprint label
    createArgs.AddRange(new[] { "--label", $"thresh.blueprint={blueprintName}" });
    
    // THIS IS WHERE THE MAGIC HAPPENS
    AddContainerArgs(createArgs, blueprint);
    
    // Add base image and shell
    createArgs.AddRange(new[] { sourcePath, "/bin/sh" });
    
    // Execute the command
    result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
}
```

At this point, `createArgs` contains:
```
["docker", "create", "--name", "thresh-postgres-dev", "-it", "--label", "thresh.blueprint=postgres-dev"]
```

### Step 4: AddContainerArgs - The Key Method
This is where volumes are translated into Docker CLI arguments:

```csharp
private void AddContainerArgs(List<string> args, Blueprint? blueprint)
{
    if (blueprint == null) return;
    
    // Add port mappings (-p HOST:CONTAINER)
    if (blueprint.Ports != null && blueprint.Ports.Count > 0)
    {
        foreach (var port in blueprint.Ports)
        {
            args.AddRange(new[] { "-p", port });
        }
    }
    
    // Add volumes (-v VOLUME:MOUNT_PATH)
    if (blueprint.Volumes != null && blueprint.Volumes.Count > 0)
    {
        foreach (var volume in blueprint.Volumes)
        {
            // THIS LINE ADDS THE VOLUME MOUNT
            args.AddRange(new[] { "-v", $"{volume.Name}:{volume.Mount}" });
        }
    }
    
    // Add bind mounts (-v HOST_PATH:CONTAINER_PATH[:ro])
    if (blueprint.BindMounts != null && blueprint.BindMounts.Count > 0)
    {
        foreach (var bindMount in blueprint.BindMounts)
        {
            var mountSpec = bindMount.ReadOnly 
                ? $"{bindMount.Host}:{bindMount.Container}:ro"
                : $"{bindMount.Host}:{bindMount.Container}";
            args.AddRange(new[] { "-v", mountSpec });
        }
    }
    
    // Add tmpfs mounts (--tmpfs PATH)
    if (blueprint.Tmpfs != null && blueprint.Tmpfs.Count > 0)
    {
        foreach (var tmpfs in blueprint.Tmpfs)
        {
            args.AddRange(new[] { "--tmpfs", tmpfs });
        }
    }
}
```

### Step 5: Final Command Array
After `AddContainerArgs()` completes, `createArgs` now contains:

```
[
  "docker",
  "create",
  "--name", "thresh-postgres-dev",
  "-it",
  "--label", "thresh.blueprint=postgres-dev",
  "-p", "5432:5432",
  "-v", "postgres-data:/var/lib/postgresql/data",
  "ubuntu:22.04",
  "/bin/sh"
]
```

### Step 6: Command Execution
ProcessHelper executes the command:

```csharp
result = await ProcessHelper.ExecuteAsync(createArgs.ToArray());
```

This runs:
```bash
docker create \
  --name thresh-postgres-dev \
  -it \
  --label thresh.blueprint=postgres-dev \
  -p 5432:5432 \
  -v postgres-data:/var/lib/postgresql/data \
  ubuntu:22.04 \
  /bin/sh
```

### Step 7: Docker Handles the Volume
Docker sees the `-v postgres-data:/var/lib/postgresql/data` and:

1. **Checks if volume exists**:
   ```bash
   docker volume inspect postgres-data
   ```
   
2. **If doesn't exist, creates it**:
   ```bash
   docker volume create postgres-data
   ```
   - Creates storage at `/var/lib/docker/volumes/postgres-data/_data`

3. **Mounts the volume**:
   - Bind mounts the host directory to the container path
   - Host: `/var/lib/docker/volumes/postgres-data/_data`
   - Container: `/var/lib/postgresql/data`

4. **Returns container ID**: `5c489c2edd02...`

## Result

Container `thresh-postgres-dev` now has:
- ✅ Port 5432 mapped to host
- ✅ Volume `postgres-data` mounted at `/var/lib/postgresql/data`
- ✅ Data persists in `/var/lib/docker/volumes/postgres-data/_data`

## The Beauty of This Approach

**thresh doesn't manage storage directly** - it just:
1. Parses blueprint JSON
2. Converts to Docker CLI arguments
3. Delegates to Docker

Docker handles:
- Volume creation
- Mount point management
- Permission handling
- Storage drivers
- Data persistence

This makes thresh lightweight and leverages Docker's mature storage system!

## Code Locations

| Component | File | Purpose |
|-----------|------|---------|
| Blueprint Model | `Models/Blueprint.cs` | Defines storage properties |
| Volume Conversion | `Services/ContainerdService.cs:354` | AddContainerArgs() |
| Container Creation | `Services/ContainerdService.cs:289` | ImportEnvironmentAsync() |
| CLI Execution | `Utilities/ProcessHelper.cs` | Runs docker commands |
| Volume Management | `Services/ContainerdService.cs:489` | List/Create/Delete |

