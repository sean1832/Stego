<h1 align="center">
  <img src="/design/source.png" width="200px"/><br/>
  Stego UI
</h1>

> A simple steganography UI tool build with WinUI 3 to hide data in images using LSB (Least Significant Bit) technique.

<p align="center">
<img src="docs/images/app-screenshot-light.png" alt="WinUI 3 Gallery" width="500"/>
</p>


## ⭐ Features
- Hide/Extract data in images using LSB technique
- Adjustable LSB bits per byte (1-8 bits)
- Adjustable LSB spacing between bytes
- Supports lossless image formats (PNG, BMP)
- Encrypt/Decrypt data before hiding it in images
- User-friendly interface
- Encryption/Decryption without steganography should user choose so
- Argon2id password hashing for security
- System theme support (light/dark mode)

## Planned Features
- [ ] Support for JPEG images (using DCT)
- [ ] Support for lossless audio formats (WAV, FLAC)
- [ ] Drag and drop support for files


## Requirements
- [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later
- Windows 10 or later
- x64 only

## Installation
You can download the latest release of StegoUI from the [Releases page](https://github.com/sean1832/stego/releases/latest).

