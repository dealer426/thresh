# Thresh MCP Server - Docker Container  
FROM ubuntu:22.04

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    ca-certificates \
    curl \
    libicu70 \
    && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /app

# Copy all the published files
COPY bin/linux-x64/ ./

# Make thresh executable
RUN chmod +x ./thresh

# Expose MCP server port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
  CMD curl -f http://localhost:8080/mcp/initialize || exit 1

# Run native thresh binary
ENTRYPOINT ["./thresh", "serve", "--port", "8080", "--host", "0.0.0.0"]
