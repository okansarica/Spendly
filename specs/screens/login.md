# SCREEN-BASED BUSINESS FLOW

## 1. Login Screen

### Purpose
This screen allows existing users to authenticate using their email and password, or through social login providers (Google, Facebook). It also provides navigation links to the Register and Forgot Password screens.

### UI Behavior
- Input fields:
  - Email
  - Password
- Buttons / Links:
  - "Login" button
  - "Login with Google" button
  - "Login with Facebook" button
  - "Register" link → navigates to Register screen
  - "Forgot Password" link → navigates to Forgot Password screen
- Upon clicking any login button:
  - Entered credentials or social login data are sent to the backend.
  - The UI displays error messages or proceeds based on the backend response.

### Backend Interaction
- Endpoints:
  - `POST /auth/login` (email/password)
  - `POST /auth/social-login` (Google, Facebook)
- Email/Password Login Request Body:
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- Social Login Request Body:
  ```json
  {
    "provider": "google" | "facebook",
    "token": "string"
  }
  ```
- Response Scenarios:
  1. **Success:** 
     - Access and Refresh tokens are returned.
  2. **Email not verified (only for email/password):** 
     - Response includes a verification code.
     - UI navigates to Verification screen (same flow as registration verification).
     - Upon successful verification, backend returns access and refresh tokens.
  3. **Invalid credentials / token:** 
     - Error message displayed on UI.

### Data & Entities
- The backend verifies against the `User` entity:
  - Email
  - PasswordHash (for email/password login)
  - EmailVerification (IsVerified, VerificationCode)
  - LoginProviders (e.g., Local, Google, Facebook)
- For social login, if the user already exists:
  - Update LoginProviders to include the new provider.
  - Do not overwrite existing password unless explicitly changed.

### Business Rules
- Access and Refresh tokens are only issued after successful authentication and verification.
- If email is not verified (email/password login), login is blocked until verification is completed.
- Social login accounts are automatically considered verified.
- Duplicate social logins for the same email update the existing user account instead of creating a new one.

---

## 2. Forgot Password Screen

### Purpose
This screen allows users to initiate password reset in case they forgot their credentials.

### UI Behavior
- Input fields:
  - Email
- Buttons / Links:
  - "Send Reset Link" button
  - "Login" link → navigates to Login screen
  - "Register" link → navigates to Register screen
- Upon clicking "Send Reset Link":
  - Email is submitted to the backend.
  - UI shows success or error messages based on response.

### Backend Interaction
- Endpoint: `POST /auth/forgot-password`
- Request Body:
  ```json
  {
    "email": "string"
  }
  ```
- Response:
  - If email exists, backend sends a password reset link to the user via email.
  - If email does not exist, a generic message is returned to prevent information leakage.
  - Further steps of password reset are out of scope for this document.
  - When the verification code is incorrect, VerificationAttemptCount is incremented.
    - If VerificationAttemptCount reaches MAX_VERIFICATION_ATTEMPTS, IsVerificationLocked is set to true and the user receives a “Please contact support” error.
    - If IsVerificationLocked is true, all verification attempts are rejected.
    - If the user is not verified and the existing verification code is expired, a new verification code is generated automatically and LastCodeIssuedAt is updated.
    - If the user is not verified and the account is locked, the verification screen displays a locked account message.


### Business Rules
- Email must be associated with an existing account.
- Backend triggers email delivery but does not handle reset flow within this spec.

---

## Notes
- All token storage and verification flows reuse the same logic as the Registration flow.
- Navigation links between screens ensure smooth UX.
- Error messages should be consistent across Login, Register, Forgot Password, and Social Login flows.

