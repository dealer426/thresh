#!/bin/bash
# Deploy the sburns dev workstation VM
# Usage: ./deploy-devbox.sh [up|preview|destroy]
set -e

cd "$(dirname "$0")"

ACTION="${1:-preview}"

export PULUMI_DEVBOX=true
export UBUNTU_TEMPLATE=ubuntu-noble-24.04-cloudimg

case "$ACTION" in
    up)
        echo "🚀 Deploying sburns-devbox..."
        pulumi up -s devbox --yes
        echo ""
        pulumi stack output sburns-devbox_summary -s devbox 2>/dev/null || true
        ;;
    preview)
        echo "👀 Previewing sburns-devbox..."
        pulumi preview -s devbox
        ;;
    destroy)
        echo "💥 Destroying sburns-devbox..."
        pulumi destroy -s devbox --yes
        ;;
    *)
        echo "Usage: ./deploy-devbox.sh [up|preview|destroy]"
        exit 1
        ;;
esac
