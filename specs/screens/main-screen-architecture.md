# AI Prompt --- Mobile Expense Tracking App UI/UX Architecture & Design System

## Role

You are a **Senior Product Designer + UX Architect + Mobile UI
Engineer**.

Your task is to design a **production-ready mobile application UI/UX
architecture** based on the requirements below.

You must:

-   Define screen flows
-   Define navigation structure
-   Define component behaviors
-   Define design system structure
-   Define color system (with tokens)
-   Make the system ready for future Light Mode / Dark Mode
-   Think in scalable, production-level terms (React Native friendly)

------------------------------------------------------------------------

# 1. Global Design System

## 1.1 Design Theme

The general design theme must be:

-   White / light gray based
-   Clean
-   Modern
-   Minimal
-   High readability
-   Soft shadows
-   Subtle dividers

### Primary UX Rules

-   Confirm buttons → **Blue**
-   Cancel / secondary buttons → **Gray / muted tone**
-   Destructive actions → Soft red
-   Backgrounds → White / very light gray tones

------------------------------------------------------------------------

## 1.2 Centralized Design Tokens (Single Source of Truth)

All design values must be stored in **one single theme file**.

Example structure:

-   `colors.ts`
-   `theme.ts`
-   or `design-tokens.ts`

This file must contain:

### Color Tokens

Use semantic naming instead of hard-coded names.

``` ts
colors = {
  backgroundPrimary: "#FFFFFF",
  backgroundSecondary: "#F5F6F8",
  borderSubtle: "#E5E7EB",

  textPrimary: "#111827",
  textSecondary: "#6B7280",

  buttonPrimary: "#2563EB",
  buttonPrimaryText: "#FFFFFF",

  buttonSecondary: "#E5E7EB",
  buttonSecondaryText: "#374151",

  danger: "#DC2626",
  success: "#16A34A",

  spinner: "#2563EB"
}
```

### Typography Tokens

-   fontSizes
-   fontWeights
-   lineHeights

### Spacing Tokens

-   spacingXs
-   spacingSm
-   spacingMd
-   spacingLg
-   spacingXl

### Radius Tokens

-   radiusSm
-   radiusMd
-   radiusLg

------------------------------------------------------------------------

## 1.3 Dark Mode Readiness

The theme system must support:

-   `lightTheme`
-   `darkTheme`

Use semantic tokens so dark mode only requires switching theme objects.

No hardcoded hex values inside components.

------------------------------------------------------------------------

# 2. App Launch Flow

## 2.1 Splash Screen

### Layout

-   Full-screen white background
-   Centered app logo
-   Spinner below logo

### Behavior

-   Shows while app initializes
-   Checks authentication state
-   If authenticated → go to main app
-   If not authenticated → go to Login screen

------------------------------------------------------------------------

# 3. Authentication Flow

## 3.1 Login Screen

### Layout

-   App logo at top
-   Email input
-   Password input
-   Login button (Primary - Blue)
-   Below:
    -   "Forgot Password?" (link style)
    -   "Don't have an account? Register" (link style)

### Behavior

-   Login button disabled until valid input
-   Show loading spinner inside button while submitting
-   Show inline validation errors

------------------------------------------------------------------------

## 3.2 Register Screen

-   Name
-   Email
-   Password
-   Confirm password
-   Register button (Primary Blue)
-   Cancel → muted gray

------------------------------------------------------------------------

## 3.3 Forgot Password Screen

-   Email input
-   Send reset link button
-   Confirmation state screen

------------------------------------------------------------------------

# 4. Post-Login App Structure

After successful login:

Main layout includes:

-   Bottom Navigation Bar
-   4 Main Tabs:
    -   Dashboard
    -   Reports
    -   Finance
    -   User

------------------------------------------------------------------------

# 5. Bottom Navigation Structure

## Tabs

### 1. Dashboard

Icon + Label

### 2. Reports

Icon + Label

### 3. Finance

Icon + Label

### 4. User

Icon + Label

Active tab → Primary Blue\
Inactive tab → Muted Gray

------------------------------------------------------------------------

# 6. Dashboard Screen

Main landing page after login.

Must include:

-   Summary cards
-   Quick overview data
-   Card-based layout
-   Subtle shadows
-   Rounded corners

Cards must use:

-   backgroundSecondary
-   radiusLg
-   paddingMd

------------------------------------------------------------------------

# 7. Reports Menu

When user taps **Reports**:

A screen opens showing 2 report options:

-   Monthly Summary Report
-   Category Distribution Report

Each item:

-   Icon
-   Title
-   Right arrow indicator
-   Subtle divider

Selecting navigates to specific report screen.

------------------------------------------------------------------------

# 8. Finance Section

When user taps **Finance**:

A list appears:

-   Categories
-   Merchants
-   Accounts (future)

Each leads to:

-   List screen
-   Add/Edit screen
-   Delete confirmation modal

------------------------------------------------------------------------

# 9. User Section

When user taps **User**:

A structured list appears.

### Special Requirement

Language selection appears at the top.

It must visually look like a normal list item.

Structure:

1.  Language (selected language shown on right)
2.  Profile Update
3.  Change Password
4.  Delete Account (destructive style)
5.  Logout

### Behavior

-   Delete Account → confirmation modal (danger style)
-   Logout → confirmation modal
-   Language selection → modal or bottom sheet

------------------------------------------------------------------------

# 10. Component Behavior Standards

## Buttons

### Primary Button

-   Blue background
-   White text
-   Disabled → faded blue

### Secondary Button

-   Gray background
-   Dark text

### Danger Button

-   Red background
-   White text

------------------------------------------------------------------------

## Lists

-   Subtle separators
-   Consistent padding
-   Icon alignment consistent
-   Touchable feedback

------------------------------------------------------------------------

## Modals

-   Rounded top corners (if bottom sheet)
-   Clear primary / cancel separation
-   Danger confirmation uses red confirm button

------------------------------------------------------------------------

# 11. UX Consistency Rules

-   All destructive actions require confirmation
-   All forms validate inline
-   All loading states must be visible
-   No hardcoded styling inside components
-   Entire app must be theme-driven
