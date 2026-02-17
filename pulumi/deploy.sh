#!/bin/bash
# Deploy Pulumi infrastructure

cd "$(dirname "$0")"

echo "Building project..."
dotnet build
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo "Deploying with Pulumi..."
./pulumi-cli/pulumi/bin/pulumi.exe up --yes
