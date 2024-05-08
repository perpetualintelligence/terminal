# Cryptographic Limitations in Blazor WebAssembly Impacting OneImlx.Terminal
Blazor WebAssembly currently lacks comprehensive support for cryptographic operations using `System.Security.Cryptography.X509Certificates`. This limitation impacts how developers can use the `OneImlx.Terminal` framework to validate JWT tokens within a client-side environment.

## Overview
Blazor WebAssembly only supports a limited set of the .NET cryptography libraries, which are not sufficient for handling X509 certificates. 

## Current Workarounds
- **Implement Server-Side Processing**: Developers should handle critical cryptographic operations on the server to circumvent these limitations.
- **Adopt Hybrid Deployment**: Utilize Blazor Server components to enable specific client interactions while securing cryptographic operations on the server.

## GitHub Discussions
For the latest updates and community discussions regarding Blazor WebAssembly's cryptography capabilities, refer to these GitHub issues:
- [Blazor WebAssembly: Cryptography Support](https://github.com/dotnet/aspnetcore/issues)
- [Enhancing Blazor WebAssembly Crypto Capabilities](https://github.com/dotnet/runtime/issues)
