# eknova CLI - Windows Development Quick Reference

## 🔄 When Starting a New Windows Development Session

Copy and paste this template when working with an AI assistant on Windows:

---

### 🖥️ **Environment Information:**
```
OS: Windows [10/11]
Java Version: [paste output of: java -version]
GraalVM/Native Image: [paste output of: native-image --version]
Git Status: [paste output of: git status --porcelain]
Current Branch: [paste output of: git branch --show-current] 
Working Directory: [paste output of: pwd or cd]
```

### 📁 **Project Context:**
```
Project: eknova CLI - Windows Native Development
Repository: https://github.com/dealer426/eknova
Main Branch: dev
CLI Location: eknova\eknova-cli\
Target Output: build\eknova-cli-1.0.0-SNAPSHOT-runner.exe
```

### 🎯 **Current Objective:**
```
[ ] Build Windows native executable without Java dependencies
[ ] Test CLI functionality on Windows
[ ] Ensure WSL integration works from Windows
[ ] Other: [specify current task]
```

### 🏗️ **Standard Build Commands:**
```cmd
# Navigate to CLI directory
cd eknova\eknova-cli

# Clean build
gradlew.bat clean

# Build native Windows executable
gradlew.bat build -Dquarkus.native.enabled=true -Dquarkus.package.jar.enabled=false

# Test executable
build\eknova-cli-1.0.0-SNAPSHOT-runner.exe --help
build\eknova-cli-1.0.0-SNAPSHOT-runner.exe version
build\eknova-cli-1.0.0-SNAPSHOT-runner.exe list
```

### ❓ **Common Issues to Check:**
```
[ ] Visual Studio Build Tools installed?
[ ] Native Image component available?
[ ] Running from Developer Command Prompt?
[ ] Sufficient memory for build (8GB+ recommended)?
[ ] Windows Defender blocking executable?
```

### 📊 **Success Indicators:**
```
[ ] Build completes without errors
[ ] Executable file ~300MB in size
[ ] --help shows CLI usage
[ ] version shows "🚀 eknova 1.0.0-SNAPSHOT"
[ ] File type shows "PE32+ executable" (not "ELF")
```

---

## 🔧 **Quick Diagnostics:**

```cmd
# System Check
java -version
native-image --version
where gradle
dir build\*.exe

# Build Environment
echo %JAVA_HOME%
echo %PATH%
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
```

## 📋 **Copy This Template:**

```
🖥️ Environment: Windows [version], Java [version], GraalVM [yes/no]
📁 Working Directory: [current path]  
🎯 Current Task: [what you're trying to accomplish]
❗ Issue: [if any - paste error messages]
🔍 Need Help With: [specific question or problem]
```

---

**📄 Full Setup Guide:** See `WINDOWS_NATIVE_SETUP.md` for complete installation instructions.