# AI Prompt: Refactor Registration Flow with Subscription Options

## Existing Flow
- When a user fills out the registration form, a verification code is sent to their email.
- The user is redirected to the verification screen.
- After entering the code, the user can log in.
- For social registration, the user logs in directly without email verification.

## New Flow
1. **Registration Submission**
   - When the user fills out the registration form and clicks **Register**, a popup appears.
   - The popup is similar to the `SubscriptionPlansModal` modal.
     - If possible, reuse the existing modal or extract common parts into shared components.
   - This popup now includes a **Trial option**.
     - If the user selects **Trial**, no payment is collected.

2. **Popup Behavior**
   - A **Close button** is available; closing the popup returns the user to the registration screen.
   - Texts and labels remain the same as the existing modal.

3. **User Selection Flow**
   - **If the user selects Trial:** this will work as current flow, the only extra thing is selection popup
     - Continue with the existing flow.
     - Send verification code to the user's email.
     - Redirect to verification screen.
     - After entering the code, log the user in.

   - **If the user selects Paid Subscription:**
     - Send verification code to the user's email as usual.
     - In the backend:
       - Create a **trial subscription** (existing flow).
       - Additionally, create a **paid subscription**, initially inactive.
         - Use `UserSubscriptionService` to undrstand the existing flow and manage the transition from trial to paid subscription.
       - Generate a **payment URL** associated with the paid subscription. use CreatePaymentUrl function 
       - Save both subscriptions in the database. (trial is already being saved, add paid one in case requested)
     - Categories, merchants, and other necessary entities are created as in the normal registration flow (`AuthService.register`).

4. **Post-Verification Behavior**
   - After the user verifies their email:
     - Open an **in-app browser** and redirect to the backend-provided payment URL. (there is an existing flow in `UserSubscriptionModal`)
     - Once payment is completed, the backend receieves a **webhook** from stripe, which sends a **mobile push notification**.
     - If the user registered and completes verification:
       - They can close the browser and still be logged in.
       - If the paid subscription is not completed, they continue with **trial subscription**.
   
5. **Edge Cases**
   - If the user closes the browser before completing the payment:
     - They remain logged in.
     - Paid subscription remains inactive.
     - User continues on trial subscription until payment is completed.

## Notes for Implementation
- Reuse existing components (`UserSubscriptionModal`) whenever possible.
- Keep trial and paid logic separate but maintain shared flow for verification and login.
- Ensure backend services handle both trial and paid subscriptions correctly:
  - `AuthService.register` should manage category/merchant creation.
  - `UserSubscriptionService` should track subscription transitions.
- Mobile push notifications should follow the same flow as existing in-app purchase notifications.
- Review the code in `UserSubscriptionService` `UserService` `AuthService` in backend to understand the existing similar operations already working when moving logged in user to paid subscription