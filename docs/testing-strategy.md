# Testing Strategy for containerd Functionality

## Overview
This document provides a strategy to test the `containerd` functionality for Linux and macOS environments. It also includes detailed steps to utilize vCenter 7 for setting up custom runners and recommendations for using Pulumi and Terraform to automate builds for testing.

---

## Goals
1. Test `containerd` functionality thoroughly across Linux distributions and macOS.
2. Ensure the reliability and consistency of workflows that depend on `containerd`.
3. Automate build and testing environments using modern IaC tools (Pulumi, Terraform).

---

## Testing Strategy

### 1. **Environment Setup**
#### Linux Environment:
- Test major distributions:
  - Ubuntu (latest LTS and rolling versions)
  - Alpine Linux
  - Fedora
  - Debian
- Ensure `containerd` version compatibility matches the CI/CD environment.

#### macOS Environment:
- Test on macOS Big Sur, Monterey, Ventura.
- Use VMware Fusion or bare-metal machines if required.
- Install `containerd` via Homebrew or manual installation.

---

### 2. **Custom Runner Setup via vCenter 7**
- Deploy virtualized test environments using vCenter.
- Add custom GitHub Actions Runners linked to the virtualized Linux distributions.
- Install dependencies for Pulumi and Terraform provisioning and automate runner retirements after testing cycles.

**Steps:**
1. Create VMs in vCenter configured for:
   - Ubuntu (for CI base validation).
   - macOS (license permitting).
2. Deploy GitHub Actions runner configurations:
   ```bash
   ./config.sh --url $GITHUB_URL --token $GITHUB_RUNNER_TOKEN
   ```

---

### 3. **Automation with Pulumi and Terraform**
#### Using Terraform:
- Define Infrastructure-as-Code definitions to spin up test environments.
  - Example: Virtual Machines, networking, and runners.
- Automate the creation and cleanup processes.

#### Using Pulumi:
- Write automation scripts in TypeScript or Python to:
  - Initiate build environments (for containerd tests).
  - Trigger tests and collect logs.

---

### 4. **Test Workflow**

#### 1. containerd Installation Verification:
- Validate installation using commands:
  ```bash
  containerd --version
  ctr --version
  ```
- Check service status:
  ```bash
  systemctl status containerd
  ```

#### 2. Load Testing with containerd:
- Run multiple images and validate:
  - `ctr pull` for various image sources.
  - `ctr run` to test container execution.

#### 3. macOS Compatibility:
- Validate Homebrew installation or manual setup.
- Ensure compatibility across Mac hardware variants.

#### 4. CI/CD Logging:
- Ensure logs from test runs are retained in the CI pipelines for debugging and reporting.
- Use GitHub Actions artifacts to collect container logs.

---

### 5. **Reporting and Results Compilation**
- Generate performance metrics and success/fail counts.
- Publish results to the GitHub repository as markdown reports.

---

## Conclusion
This strategy ensures thorough testing on both Linux and macOS environments with automation. By leveraging vCenter 7 for virtualized runners and Pulumi/Terraform for lifecycle management, we aim for a scalable and reliable testing pipeline.