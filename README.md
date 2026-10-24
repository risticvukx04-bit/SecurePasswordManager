# Web-Based Secure Password Manager

## Project Description

A web-based password manager designed to securely store and manage user credentials and files.

The application provides user registration and authentication, secure session management, an encrypted password vault, and encrypted file storage. Sensitive data is encrypted on the server before being stored.

## Scope

The application includes:

- User registration and authentication
- Secure password hashing
- Secure session management
- Password vault with add, edit, and delete operations
- AES-256-GCM encryption of stored vault passwords
- File upload, server-side encryption, and download
- Per-user access control for vault entries and files
- Secure cookie configuration
- HTTPS in the development environment

## Architecture

The application consists of:

- Web browser and HTML/CSS/JavaScript interface
- ASP.NET Core web application
- SQLite database managed with Entity Framework Core
- .NET User Secrets for storing the encryption key
- Encrypted file storage in `App_Data/EncryptedFiles`

Communication between the browser and the application is protected using HTTPS in the development environment.

## Security Features

### Password Hashing

User account passwords are hashed using ASP.NET Core Identity's `PasswordHasher<User>`. Plaintext account passwords are not stored in the database.

### Vault Encryption

Passwords stored in the vault are encrypted using AES-256-GCM with a 256-bit encryption key.

A new random nonce is generated for each encryption operation. The authentication tag provides integrity protection for encrypted data.

### File Encryption

Files uploaded by authenticated users are encrypted server-side before being saved to disk.

- Files are encrypted using the application's encryption service.
- Encrypted file contents are stored in `App_Data/EncryptedFiles`.
- File metadata is stored in the SQLite database.
- Files are decrypted on the server when an authorized user downloads them.
- File access is restricted to the authenticated user who owns the file.
- The maximum upload size is 10 MB.

### Key Management

The AES encryption key is stored using .NET User Secrets and is not included in the source code or Git repository.

The encryption key must be configured before running the application.

### Session Security

Sessions use cryptographically random 32-byte tokens. Only a SHA-256 hash of the session token is stored in the database.

The session cookie uses:

- `HttpOnly`
- `Secure`
- `SameSite=Strict`
- One-hour expiration

Sessions are invalidated when the user logs out.

### Access Control

Vault entries and uploaded files are associated with individual users. Authenticated requests verify the user's session and ownership before allowing access to protected data.

## API Routes

### Authentication

- `POST /register` — Create a new user account
- `POST /login` — Authenticate a user
- `POST /logout` — Log out the current user

### Password Vault

- `GET /vault` — Retrieve the authenticated user's stored credentials
- `POST /vault` — Add a new credential
- `PUT /vault/{id}` — Update an existing credential
- `DELETE /vault/{id}` — Delete an existing credential

### File Storage

- `POST /files/upload` — Upload and encrypt a file
- `GET /files` — List the authenticated user's uploaded files
- `GET /files/{id}/download` — Decrypt and download an authorized file

## Technologies

- C#
- ASP.NET Core
- .NET 9
- Entity Framework Core
- SQLite
- ASP.NET Core Identity PasswordHasher
- AES-256-GCM
- SHA-256
- HTTPS
- .NET User Secrets
- HTML, CSS, and JavaScript

## How to Run Locally

### Requirements

- .NET 9 SDK
- Git
- A web browser

### 1. Clone the Repository

```bash
git clone https://github.com/risticvukx04-bit/SecurePasswordManager.git
cd SecurePasswordManager
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure the Encryption Key

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Generate and configure a random 32-byte Base64 encryption key in PowerShell:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "EncryptionKey" $key
```

Do not commit the encryption key to the repository.

### 4. Configure the Development HTTPS Certificate

On a machine where the development HTTPS certificate is not yet trusted, run:

```bash
dotnet dev-certs https --trust
```

Follow any confirmation prompts.

### 5. Run the Application

```bash
dotnet run --launch-profile https
```

Open the HTTPS address displayed in the terminal. In the current development configuration, the application uses:

`https://localhost:7027`

Keep the terminal open while using the application.

## Database and File Storage

The application uses SQLite with Entity Framework Core.

The database stores:

- `Users` — user accounts and password hashes
- `Sessions` — active session token hashes
- `VaultEntries` — encrypted vault credentials
- `EncryptedFiles` — uploaded file metadata and ownership information

Encrypted file contents are stored separately in `App_Data/EncryptedFiles`. The SQLite database and other local data files should not be committed to Git.

## Current Limitations

- The vault encryption key is not derived directly from the user's master password. Account password hashing and vault encryption use separate mechanisms.
- Multi-factor authentication (MFA) is not currently implemented.
- HTTPS is configured for local development; production deployment would require additional configuration and security testing.
