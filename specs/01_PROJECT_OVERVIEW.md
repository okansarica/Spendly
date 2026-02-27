# Mobile Expense Tracking Application – Project Specification

## 1. Project Overview

This project is a **mobile expense tracking application** focused exclusively on **expense management** (no income tracking).

The system enables users to:

- Track expenses across multiple accounts and cards
- Import transactions via bank statements or Open Banking APIs
- Categorize and manage merchants
- Analyze spending through reports and summaries

> ⚠️ Important: The application does **not** handle income tracking. Only expenses are considered in all calculations and reports.

---

## 2. Authentication & User Management

### 2.1 Authentication Methods

Users can sign up and log in via:

- Email & password (traditional account creation)
- Google OAuth
- Facebook OAuth

### 2.2 User Profile

Each user has:

- Authentication credentials
- Linked accounts/cards
- Assigned categories (auto-generated at onboarding)
- Assigned default merchant list
- App preferences & data permissions

---

## 3. Onboarding Flow

When a new user registers:

1. The system automatically creates predefined default categories for the user.
2. A predefined global merchant list is assigned to the user.
3. Each merchant in this list is pre-linked to a relevant default category.

Users can later:

- Delete merchants
- Edit merchants
- Reassign merchants to different categories
- Remove category linkage

---

## 4. Core Domain Model

### 4.1 Categories

- Predefined system categories exist.
- These are cloned per user during onboarding.
- Users can perform full CRUD operations:
    - Create category
    - View category
    - Update category
    - Delete category

### 4.2 Merchants

- System provides a fixed base merchant list.
- Merchants are assigned to users during onboarding.
- Each merchant:
    - Belongs to **only one category**
    - May exist without a category (uncategorized)

Users can:

- Edit merchant name
- Delete merchant
- Reassign category
- Leave merchant uncategorized

---

### 4.3 Accounts & Cards

Users can create unlimited:

- Bank accounts
- Credit cards
- Debit cards
- Other financial accounts

Users can perform full CRUD operations on accounts/cards.

Each transaction must be linked to one account or card.

---

## 5. Transaction Import System

Transactions can be added in two ways:

### 5.1 Bank Statement Upload

Users can upload bank statements (e.g., PDF, CSV, etc.).

The system will:

1. Parse the statement
2. Extract:
    - Transaction date
    - Amount
    - Merchant name
    - Description
    - Account reference
3. Store transactions in the database
4. Attempt merchant matching
5. Apply category based on merchant linkage

---

### 5.2 Open Banking API Integration

With user consent, the system can:

- Connect to bank/provider APIs
- Fetch transactions periodically (daily or scheduled)
- Automatically import expenses
- Match merchants
- Apply category mapping

---

## 6. Data Flow & Processing

1. Transaction is imported (via statement or API).
2. Merchant name is normalized.
3. System attempts merchant match:
    - If merchant exists → link transaction.
    - If not → create unlinked merchant entry.
4. Category is assigned based on merchant mapping.
5. Transaction becomes visible in:
    - Dashboard
    - Reports
    - Account summaries
    - Category summaries

---

## 7. Application Structure (Main Sections)

The application consists of four main sections:

---

## 7.1 Home (Dashboard)

Purpose: High-level financial overview.

Displays:

- Total balance / total debt
- Recent transactions
- Category-based spending summaries
- Monthly overview snapshot

All values reflect **expenses only**.

---

## 7.2 Reports

Purpose: Analytical spending insights.

Features:

- Date range filtering
- Account-based filtering
- Category-based filtering
- Transaction listing
- Aggregated totals

Reports focus strictly on outgoing transactions.

---

## 7.3 Finance (Management Section)

Administrative and configuration area.

Includes:

- Category management (CRUD)
- Merchant management
- Merchant–Category mapping
- Account/Card management
- Bank statement upload
- API connection management

This is the system configuration layer.

---

## 7.4 User Settings

Includes:

- Profile management
- Email/password management
- OAuth connections
- Data permissions (Open Banking consent)
- Logout / account management

---

## 8. Business Rules

1. The system tracks **expenses only**.
2. Income transactions are ignored or excluded.
3. Each merchant can belong to **only one category**.
4. A merchant may exist without a category.
5. Each transaction:
    - Must belong to one account/card.
    - May belong to one merchant.
6. Categories are user-specific (even if system-defined initially).
7. Users can customize their financial structure entirely after onboarding.

---

## 9. Scalability & Architecture Considerations

- Multi-user isolated data model
- Import pipeline with normalization layer
- Idempotent transaction imports
- Merchant name cleaning & fuzzy matching support
- Category auto-assignment rules engine (future extensibility)
- Scheduled background jobs for API transaction syncing

---

## 10. Non-Goals

The following are explicitly out of scope:

- Income tracking
- Investment tracking
- Budget planning (unless added later)
- Tax calculation features

---

## Summary

This application is a:

- Multi-account
- Merchant-aware
- Category-driven
- Expense-only
- Mobile-first financial tracking system

It combines:

- Manual import (statement parsing)
- Automated import (Open Banking APIs)
- Strong categorization logic
- Customizable user finance structure
