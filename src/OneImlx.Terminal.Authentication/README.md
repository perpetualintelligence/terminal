# Terminal Authentication

## Introduction

Welcome to the `pi-cli` authentication, a part of the `pi-cli` cross-platform developer framework designed for building modern command-line interface (CLI) terminals. This module is specifically tailored to handle authentication, providing a seamless and secure experience for users and developers alike.

### **Note:** 
This is a ***preview*** release. Features and implementations are subject to change, and the module may undergo design changes without advance notice.

## MSAL Support

### Overview
As of now, our primary focus is to integrate Microsoft Identity Client (MSAL) as the authentication provider. MSAL offers robust, secure, and easy-to-implement authentication for various Microsoft identities.

### Key Features
- **Cross-Platform Token Caching**: Leveraging MSAL's capabilities, our module includes an implementation of a cross-platform token cache. This feature enhances efficiency and user experience by reducing the frequency of token acquisitions.
- **Secure and Efficient Authentication**: Ensures secure handling of tokens while optimizing the authentication process across different platforms.
- **Easy Integration**: Designed to be easily integrated into the `pi-cli` framework, facilitating smooth authentication processes in your CLI applications.

## Future Directions

While MSAL is our current choice for authentication, we are open to extending support to other Identity Providers (IdPs) in the future. This would broaden the scope of our module, allowing for a more diverse and flexible authentication ecosystem.

## Premium Positioning

Details to be determined (TBD).

## Getting Started

1. **Installation**: Instructions on how to integrate the authentication module into your `pi-cli` framework will be provided.
2. **Configuration**: Guidance on configuring the module with MSAL and setting up the token cache.
3. **Examples and Use Cases**: Sample implementations and scenarios to help you understand how to use the module in real-world applications.

## Contributing

Contributions are welcome! Whether it's feature suggestions, code contributions, or bug reports, we value your input and encourage collaboration. Please refer to our contributing guidelines for more information.

## License and Terms

This project is licensed under [specify license]. For more details on the license, terms, and data policies, visit our [terms and conditions](https://terms.perpetualintelligence.com/articles/intro.html).
