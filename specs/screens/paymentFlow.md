# Subscription Renewal & Stripe Payment Flow – Implementation Prompt

## Goal

Redesign and improve the subscription renewal flow in the mobile application and implement a Stripe-based payment process with backend coordination.

---

## 1. Trigger Condition (Existing functionality)

- When the user's `SubscriptionEndDate` has expired:
    - Showing a **subscription blocker modal**.
    - This modal must prevent access to premium features.
---

## 2. Modal UX Improvements (Mobile App)

Redesign the modal to:

- Be visually appealing and conversion-focused.
- Clearly explain the benefits of renewing.
- Contain two selectable payment options:
    - **Monthly Plan** – £3.99
    - **Yearly Plan** – £39.99
- Highlight the yearly plan as “Best Value” (optional but recommended).
- Include:
    - Plan selection UI (radio or selectable cards)
    - A `Renew` button

Prices are fetched from backend and can be cached in the app for **1 hour**.

---

## 3. Backend – Subscription Prices

- Prices should be **hardcoded in backend**:
    - Monthly: `3.99`
    - Yearly: `39.99`

- Expose endpoint to fetch available subscription plans.
- App caches response for 1 hour.

---

## 4. Renew Flow – Create Payment URL

When user selects a plan and presses `Renew`:

### 4.1 API Call

Mobile app sends request to: POST /user/create-payment-url


Request body includes:
- SelectedPlanType (Monthly / Yearly)

---

## 5. Backend Flow – Create Payment Session

When request is received:

### Step 1 – Create UserSubscription Record

Create new `UserSubscription` record, use the exiting class

- UserId
- PlanType
- PaymentStatus = `Waiting`
- StartDate = null
- EndDate = null

### Step 2 – Reserve ObjectId

- Generate and reserve a new `ObjectId`
- This ID will be used as a **Stripe reference ID**
- Dont store it anywhere until getting a response from stripe

---

## 6. Stripe Integration

### Step 1 – Create Payment URL

- Send request to Stripe to create a payment session.
- Pass reserved ObjectId as:
    - `client_reference_id` (or equivalent Stripe reference field)

### Step 2 – Redirect URLs

Redirect URLs must:

- Be configurable
- Read from `shared.local.json`
- Add new configuration section:

    "StripeSettings": {
      "SuccessUrl": "...",
      "CancelUrl": "...",
      "WebhookSecret": "...",
      "ApiKey": "..."
    }

Create corresponding StripeSettingsViewModel

Register in shared AddSettings configuration section

### 7. Stripe Response Handling

When Stripe returns response:

- Validate that returned reference ID matches reserved ObjectId.

If valid:

- Create new `UserSubscriptionPaymentUrl` record.

You may modify `UserSubscriptionPaymentUrl` entity to store:

- StripeSessionId
- ClientReferenceId
- PaymentUrl
- Token (if needed)
- RawResponse (optional)
- CreatedDate

### 8. Return Payment URL to Mobile

Backend returns:

```json
{
  "paymentUrl": "https://stripe-session-url"
}
```

### 9. Mobile – Open Stripe in App

Mobile app:

- Opens the returned URL using an in-app browser library.
- Must not be fullscreen should be like bottom sheet style (slides from bottom).
- Covers almost entire screen.
- User can close the browser.

### 10. If User Closes Payment

If user closes browser without completing payment:

- Show subscription blocker again. (Never hide this when opening the browser. Browser will be popup sliginde from bottom, so expected to be on top of everything.)
- User can reselect plan and retry.

### 11. Webhook Handling

- Create an endpoint to receive Stripe webhooks for payment status updates.
- Save the webhook payload and headers in the communication logs entity for auditing and debugging purposes. All these logs should contain the same reference id
- Validate webhook signature using `WebhookSecret` from configuration.

Webhook handling should update:

- `UserSubscription.PaymentStatus`
- Set `StartDate`
- Set `ExpectedEndDate` (based on plan type)
- `UserSubscriptionPaymentUrl` record with Stripe response details.
- 

## Rules
- Create an entity to save the stripe communication logs, including:
    - Request payload
    - Response payload
    - Headers 

- Stripe communication should be wrapper with try catch and return FunctionResponse.Failure if an error occurs. log the error details using the existing serilog logger.
