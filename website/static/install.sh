#!/usr/bin/env bash
set -euo pipefail

REPO="dealer426/thresh"
INSTALL_DIR="/usr/local/bin"
BINARY_NAME="thresh"

# Detect OS and architecture
OS="$(uname -s)"
ARCH="$(uname -m)"

case "$OS" in
  Linux*)
    case "$ARCH" in
      x86_64) ASSET="thresh-linux-x64.tar.gz" ;;
      *)
        echo "Unsupported architecture: $ARCH. Only x86_64 is supported on Linux." >&2
        exit 1
        ;;
    esac
    ;;
  Darwin*)
    case "$ARCH" in
      arm64) ASSET="thresh-macos-arm64.tar.gz" ;;
      x86_64)
        echo "macOS Intel is not currently supported. Please use Apple Silicon (M1/M2/M3)." >&2
        exit 1
        ;;
      *)
        echo "Unsupported architecture: $ARCH." >&2
        exit 1
        ;;
    esac
    ;;
  *)
    echo "Unsupported operating system: $OS" >&2
    echo "For Windows, run: irm https://thresh.sh/install.ps1 | iex" >&2
    exit 1
    ;;
esac

# Fetch the latest release version tag from GitHub
echo "Fetching latest thresh release..."
LATEST_TAG="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
  | grep '"tag_name"' \
  | sed 's/.*"tag_name": *"\([^"]*\)".*/\1/')"

if [ -z "$LATEST_TAG" ]; then
  echo "Failed to determine the latest release version." >&2
  exit 1
fi

DOWNLOAD_URL="https://github.com/${REPO}/releases/download/${LATEST_TAG}/${ASSET}"

echo "Downloading thresh ${LATEST_TAG} for ${OS}/${ARCH}..."
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

curl -fsSL "$DOWNLOAD_URL" -o "${TMP_DIR}/${ASSET}"

echo "Extracting..."
tar -xzf "${TMP_DIR}/${ASSET}" -C "$TMP_DIR"

# The binary inside the archive is named 'thresh'
if [ ! -f "${TMP_DIR}/${BINARY_NAME}" ]; then
  echo "Unexpected archive structure: '${BINARY_NAME}' not found after extraction." >&2
  exit 1
fi

chmod +x "${TMP_DIR}/${BINARY_NAME}"

echo "Installing thresh to ${INSTALL_DIR}/${BINARY_NAME}..."
if [ -w "$INSTALL_DIR" ]; then
  mv "${TMP_DIR}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
else
  sudo mv "${TMP_DIR}/${BINARY_NAME}" "${INSTALL_DIR}/${BINARY_NAME}"
fi

echo ""
echo "thresh ${LATEST_TAG} installed successfully!"
echo ""
thresh version 2>/dev/null || true
echo ""
echo "Get started: thresh --help"
echo "Documentation: https://thresh.sh/docs"
