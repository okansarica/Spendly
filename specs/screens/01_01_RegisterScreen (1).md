
# PROJECT OVERVIEW 
## Purpose 
This document defines the high-level purpose and functional scope of the authentication module of the Expense Tracker system. 
The goal of this module is to provide a secure, scalable, and extensible user registration and verification flow that supports both traditional email/password registration and third-party social authentication. 
This document describes system behavior from a product and business logic perspective. Technical implementation details are defined in dedicated specification documents. 
--- 
## Product Vision 
The system provides a secure user onboarding experience that includes: 
- Email/password-based registration 
- Email verification with code confirmation 
- Social login (Google, Facebook) 
- Secure token-based authentication 
- Automated email notifications 
The authentication system must be scalable, secure, and designed to support future expansion without architectural changes. 
--- 
## High-Level Registration Flow 
### 1. Standard Email Registration 
#### User Interface Behavior 
- The Register screen displays input fields: 
 - Name 
 - Surname 
 - Email 
 - Password 
- A "Register" button submits the form. 
- A Login button/link returns to login screen 
- When the button is pressed: 
 - The entered data is sent to the backend via an anonymous endpoint. 
#### Backend Behavior 
- The backend receives the registration request. 
- The system validates that the email is not already registered. 
- If valid: 
 - A new `User` entity is created. 
 - The password is securely hashed before storage. 
 - Email verification metadata is generated. 
 - A verification code is created. 
 - The user is stored in the database. 
- A verification email containing the code is sent using amazon ses integration. Create a shared service for email sending 
- The API responds with: 
 - Success (verification required), or 
 - An appropriate error (e.g., email already exists). 
#### User Entity Structure 
The `User` entity includes: 
- Name 
- Surname 
- Email 
- PasswordHash 
- EmailVerification: 
 - IsVerified 
 - VerificationDateTime 
 - VerificationCode 
 - VerificationAttemptCount
 - IsVerificationLocked
 - VerificationLockedAt?
- LoginProviders (e.g., Local, Google, Facebook) 
--- 
### 2. Email Verification Flow 
#### UI Behavior 
- After successful registration, the Verification screen is displayed. 
- The user enters the verification code received via email. Verification code compoent should be designed as a reusable component 
- The code is submitted to the backend. 
#### Backend Behavior 
- The backend validates the verification code. 
- If valid: 
 - The user’s `EmailVerification.IsVerified` flag is updated. 
 - `VerificationDateTime` is set. 
 - A welcome email is sent. 
 - Access and Refresh tokens are generated. 
- Tokens are returned in the response. 
#### Post-Verification UI Behavior 
- Tokens are securely stored on the client. 
- The user is considered authenticated. 
- The user is redirected to the main application screen. 
--- 
### 3. Social Registration (Google / Facebook) 
#### UI Behavior 
- The Register screen includes: 
 - "Continue with Google" 
 - "Continue with Facebook" 
- When selected: 
 - The user completes authentication with the external provider. 
 - The required user information (email, name) is retrieved. 
 - The data is sent to the backend. 
#### Backend Behavior 
- If the email does not exist: 
 - A new user is created. 
 - Email is automatically marked as verified. 
 - Password is stored as empty/null. 
 - The login provider is recorded. 
- If the email already exists: 
 - The existing user record is updated to include the new login provider. 
 - The user can log in using both password and social login (if password exists). 
- If the user was previously created using social login only: 
 - Standard registration with the same email is not allowed. 
- Tokens are generated and returned upon successful authentication. 
--- 
## Business Rules 
The following rules must always be enforced: 
1. A single email address cannot be registered more than once. 
2. If an existing email registers through a new social provider, the existing account must be updated instead of creating a new account. 
3. A user registered via social login must not be allowed to register again using email/password. 
4. Email verification is mandatory for standard registration. 
5. Social login accounts are considered verified by default. 
6. Access and Refresh tokens must be generated only after successful authentication or verification. 
--- 
## Environments 
The system operates in the following environments: 
- Development 
- Staging 
- Production 
Each environment must use isolated configuration, secrets, and email credentials. 
--- 
## Scope Limitation 
This document currently describes only the Registration and Verification flow. 
Other modules (Login, Password Reset, Expense Management, Reporting, etc.) will be defined in separate specifications.

---

## Additions (Appended Without Modifying Existing Requirements)

### Constants
- `VERIFICATION_CODE_TIMEOUT`: **1 day (24 hours)**. The timeout duration **must be defined in constants/config** and not hard-coded. 
- `ACCESS_TOKEN_LIFETIME`: **15 minutes**. Keep in constants. 
- `REFRESH_TOKEN_LIFETIME`: **30 days**. Keep in constants. 
- `MAX_VERIFICATION_ATTEMPTS` : **5** Keep in constants.

### Verification Code Policy
- **Code Length**: 4 digits (numeric). 
- **Expiration**: If the user attempts to **log in** and `EmailVerification.IsVerified == false` **and** the existing code is **expired** (based on `VERIFICATION_CODE_TIMEOUT`), the system **must automatically generate and send** a new verification code. 
- **Attempt Limits**: After **5 incorrect code entries**, the verification flow must be **locked** for the account and a **“Please contact support”** message must be shown. Unlock flow to be handled by support/admin. 

### Password Policy
- **Minimum length**: 6 characters. 
- **Composition**: Must include at least **one lowercase letter [a-z]**, **one uppercase letter [A-Z]**, and **one digit [0-9]**. 
- **Symbols**: **Not required**. 

### Password Hashing Standard
- Use a **strong, modern, memory-hard** password hashing algorithm. **Preferred**: **Argon2id** with a unique per-user salt and calibrated parameters appropriate for the environment. 
- If Argon2id is unavailable in the target runtime, **fallback** to **bcrypt** with an appropriate cost factor (e.g., ≥ 12). 

### Token Management
- **Access Token Lifetime**: **15 minutes** (see `ACCESS_TOKEN_LIFETIME` constant). 
- **Refresh Token Lifetime**: **30 days** (see `REFRESH_TOKEN_LIFETIME` constant). 
- **Refresh Token Storage**: Refresh tokens **must be persisted in the database** and associated with the user/session. 

### Registration & Login Constraints
- A user who has **registered but not yet verified** **cannot register again** with the same email address. 
- Such a user **can log in**; upon login, the **verification screen must be shown again** until email verification is successfully completed. 

### UX Requirements
- **Buttons must show a spinner** during **every API request** they trigger. Either design a **dedicated loading button** component or reuse an existing one across the app. 
