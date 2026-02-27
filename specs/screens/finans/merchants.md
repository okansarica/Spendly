# AI Prompt — Finance Module: Merchant Management (Mobile Expense Tracking App)

## Role

You are a **Senior Product Designer + UX Architect + Senior Mobile Engineer (React Native)** working with a **.NET backend**.

Design a **production-ready Merchant Management module** for the **Finance section** of a mobile expense tracking application.

Your output must include:

- Screen architecture  
- User flows  
- UI hierarchy  
- Component behavior definitions  
- State management strategy (React Native)  
- Backend expectations (.NET)  
- Edge cases  
- Empty & error states  
- Sorting & filtering logic  
- Performance considerations (NO pagination, NO lazy loading)  

The result must be implementation-ready and suitable for engineering handoff.

---

# Product Context

This is a **mobile expense tracking application** focused strictly on **expenses (no income tracking)**.

## Core Capabilities

- Email login  
- Google & Facebook OAuth login  
- Unlimited accounts/cards per user  
- Bank statement upload (parsed into transactions)  
- Open banking integration (periodic transaction sync with user consent)  
- Category CRUD  
- Merchant CRUD  
- Merchant–Category binding (each merchant can belong to exactly ONE category)  
- Transactions are matched to merchants during import  
- Users can reassign merchants to different categories. Every merchant can have only one category

---

# Onboarding Logic

When a new user registers:

- Predefined system categories are created for the user  
- Each category comes with predefined merchants  
- These merchants are editable and removable by the user  

---

# Scope of This Prompt

Design the **Merchant Management section** under:

Finance → Merchants

This section manages:

- All merchants  
- Merchant-category assignments  
- Monthly expense summaries per merchant  
- Merchant-level transaction visibility  

---

# 1️⃣ Merchant List Screen

## General Rules

- Displays ALL merchants  
- NO pagination  
- NO lazy loading  
- Default filter: Current calendar month  

---

## Screen Layout Structure

### Top Section — Filters & Controls

Must include:

1. **Month Selector**
   - Default: current month  
   - Can set date range
   - Date filter will make a query to api

2. **Search Input**
   - Filters by merchant name  // this will work in ui
   - Case-insensitive  
   - Instant filtering (debounced)  

3. **Sort Selector**
   Options:
   - Alphabetical (A–Z)  
   - Alphabetical (Z–A)  
   - Highest Spending (selected month)  
   - Lowest Spending (selected month)  
   - Most Transactions (selected month)  
   - Least Transactions (selected month)  

Sorting applies after filtering.

---

## Merchant List Item (Row/Card Design)

Each merchant row must display:

- Merchant Name (primary emphasis)  
- Assigned Category (badge style)  
- Total Spending (selected month)  
- Transaction Count (selected month)  

If merchant has:
- 0 transactions in selected month → show 0 amount and 0 count  
- Merchant must NOT be hidden  

Tapping a merchant navigates to the Merchant Detail Screen.

---

## Sorting Logic

Sorting must use:

- Aggregated totals for selected month  
- OR merchant name (case-insensitive)  

Tie-breaker rule:
- Alphabetical ascending  

---

## Empty States

Define different UX states for:

1. No merchants exist  
2. Merchants exist but none match filter/search  
3. Merchants exist but no transactions in selected month  

Each state must include:
- Title  
- Supporting description  
- Optional CTA  

---

## Performance Constraints

Even without pagination/lazy loading, app must handle:

- 500–1000 merchants  
- Monthly aggregation calculations  

### Backend Strategy (.NET)

- Server-side aggregation (GroupBy MerchantId + Date range)  
- Indexed queries (UserId, MerchantId, Date)  
- Avoid N+1 queries  
- Optional: precomputed monthly summaries  


# Merchant Detail Page — Finance Module (Mobile Expense Tracking App)

## Purpose

This screen opens when the user taps a merchant from:

Finance → Merchants → Merchant List

It provides:

- Monthly summary of spending for the selected merchant  
- Full list of transactions for the selected date range  
- Filtering and sorting controls  
- Category reassignment capability  
- Administrative merchant actions (optional: rename, delete, merge)  

This screen must be implementation-ready for React Native + .NET backend.

---

# 1️⃣ Navigation & Entry Rules

## Entry Points

- Merchant List Screen → Tap Merchant
- Deep link (optional future support)

