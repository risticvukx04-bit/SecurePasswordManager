# Web-Based Secure Password Manager

## Project Description

A web-based password manager designed to securely store and manage user credentials.

The application provides user registration and authentication, secure session management, and a password vault for storing website credentials. Sensitive vault passwords are encrypted before being stored in the database.

## Scope

The application includes:

* User registration and authentication
* Secure password hashing
* Secure session management
* Password vault
* Adding, editing and deleting stored credentials
* AES-256-GCM encryption of stored vault passwords
* Secure cookie configuration
* Per-user access control
* Protection against common web application attacks

## Architecture

The application consists of:

* Web browser
* ASP.NET Core web application
* SQLite database
* .NET User Secrets for storing the encryption key

Communication between the browser and the application is protected using HTTPS.

## Security Features

### Password Hashing

User account passwords are hashed using ASP.NET Core Identity's `PasswordHasher<User>`. Plaintext account passwords are never stored in the database.

### Vault Encryption

Passwords stored in the vault are encrypted using AES-256-GCM with a 256-bit encryption key.

A new random nonce is generated for each encryption operation. The authentication tag provides integrity protection for the encrypted data.

### Key Management

The AES encryption key is stored using .NET User Secrets and is not included in the source code or GitHub repository.

### Session Security

Sessions use cryptographically random 32-byte tokens. Only a SHA-256 hash of the session token is stored in the database.

The session cookie uses:

* `HttpOnly`
* `Secure`
* `SameSite=Strict`
* One-hour expiration

Sessions are invalidated when the user logs out.

### Access Control

Vault entries are associated with a specific user. Vault operations verify the authenticated user's session and restrict access to entries belonging to that user.

## API Routes

### Authentication

* `POST /register` — Create a new user account
* `POST /login` — Authenticate a user
* `POST /logout` — Log out the current user

### Password Vault

* `GET /vault` — View the authenticated user's stored credentials
* `POST /vault` — Add a new credential
* `PUT /vault/{id}` — Update an existing credential
* `DELETE /vault/{id}` — Delete an existing credential

## Technologies

* C#
* ASP.NET Core
* .NET 9
* Entity Framework Core
* SQLite
* ASP.NET Core Identity PasswordHasher
* AES-256-GCM
* HTTPS
* .NET User Secrets

## How to Run Locally

### Requirements

* .NET 9 SDK
* A web browser

### Clone the Repository

Clone the repository and navigate to the project directory:

```bash
git clone https://github.com/risticvukx04-bit/SecurePasswordManager.git
cd SecurePasswordManager
```

### Restore Dependencies

```bash
dotnet restore
```

### Configure the Encryption Key

The application requires a 32-byte encryption key stored using .NET User Secrets.

Initialize User Secrets if necessary:

```bash
dotnet user-secrets init
```

Generate a 32-byte Base64 key and configure it as:

```bash
dotnet user-secrets set "EncryptionKey" "<BASE64_KEY>"
```

The encryption key must not be committed to the repository.

### Run the Application

```bash
dotnet run
```

The application will be available at the HTTPS URL displayed in the terminal.

## Database

The application uses SQLite with Entity Framework Core.

The database contains:

* `Users` — user accounts and password hashes
* `Sessions` — active user sessions
* `VaultEntries` — encrypted vault credentials

The local SQLite database is excluded from Git using `.gitignore`.

## Current Limitations

The current implementation does not derive the vault encryption key directly from the user's master password. The authentication password hash and the vault encryption key are separate.

Multi-factor authentication (MFA) is not currently implemented.
