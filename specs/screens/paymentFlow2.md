# Payment Flow Implementation

## Context

The application uses **Stripe for subscription payments** and **Firebase Cloud Messaging (FCM)** for push notifications.

Currently, the payment process mostly works, but we need to extend it with **push notifications to inform the app when a payment succeeds or fails**.

---

# Current Flow

1. When a user's **subscription expires**, the app opens a screen called **`SubscriptionBlocker`**.
2. This screen **blocks further usage** of the application.
3. The screen contains:
    - Plan selection
    - A **Renew** button.

4. When the user presses **Renew**:
    - The backend creates a **Stripe payment URL**.
    - This URL is returned to the UI.

5. The UI opens the **Stripe payment page inside an in-app browser**.

6. After the user completes the payment:
    - The backend waits for a **Stripe webhook** on the endpoint:

      ```
      stripe-webhook
      ```

7. When the webhook arrives:
    - The backend updates database records such as:
        - Subscription status
        - Subscription payment URL
        - Other related payment data.

This part of the system **is already working correctly**.

---

# Required Extension

When Stripe sends the webhook (payment success or failure), the backend must **send a Firebase push notification to the user’s device**.

This notification will inform the app whether the payment **succeeded or failed**.

---

# Firebase Notification Service

Firebase logic must be implemented in a **separate service** with the following responsibilities:

- Sending Firebase push notifications
- Encapsulating all Firebase-related logic
- Being reusable for **future notifications**

Example concept:
FirebaseNotificationService


This service should handle:
- Authentication with Firebase
- Sending push notifications
- Structuring notification payloads

---

# Notification Payload

The push notification should contain an enum **flag indicating payment result**.

Example payload:

```json
{
  "type": "subscription_payment_result",
  "status": "success"
}
```

## Mobile App Behavior

When the application receives this notification:

1. Close In-App Browser

   If the payment page is still open in the in-app browser, it must be closed immediately.

2. Handle Failed Payment

   If:

   ```text
   status = fail
   ```

   The app should:

   - Show an error toast
   - Keep the `SubscriptionBlocker` screen active

3. Handle Successful Payment

   If:

   ```text
   status = success
   ```

   The app should:

   - Close the `SubscriptionBlocker` screen

   The logic likely depends on the subscription end date. The existing system already manages this through the `subscription-end` endpoint—investigate this endpoint to understand how the subscription state is determined.

   Most likely `subscriptionEndDate` should no longer be undefined in the app  and the user will be treated as subscribed. Once this state updates, the user should be able to continue using the application normally.

## Summary

### Backend

- (EXISTING BEHAVIOUR DONT CHANGE) Listen to Stripe webhook
- (EXISTING BEHAVIOUR DONT CHANGE) Update database
- Send Firebase push notification via `FirebaseNotificationService`

### Firebase Service

- Dedicated service
- Responsible for all FCM logic
- Reusable for other notifications

### Mobile App

On receiving the notification:

- Close in-app browser
- If payment failed → show error toast
- If payment succeeded → remove `SubscriptionBlocker`
- App continues normally

### Goal

Ensure that the mobile app immediately reacts to Stripe payment results via Firebase push notifications, without requiring polling or manual refresh.
