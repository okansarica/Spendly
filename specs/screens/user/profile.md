# User profile navigation

Top-level language selection dropdown. When the user changes the language the choice is sent to the backend immediately and saved as the user's preference. The app updates its UI to the selected language.

Navigation includes links to the Profile screen and the Change Password screen.

## Profile screen

Shows first name, last name, email, and newsletter subscription status. On open the screen loads the user's current information. The user can update all fields except email. When the user taps Save the updated information is persisted and a confirmation message is shown.

At the bottom of the screen there are Logout and Delete My Account buttons.

## Change Password screen

Fields: current password, new password, confirm new password. The user enters the current password and the new password twice. The confirm field must match the new password. On Save the app verifies the current password, updates it if correct, and shows a confirmation message.
