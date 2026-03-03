// CHANGED_BY_AI: 2026-03-02 - Add merchant nickname field
// CHANGED_BY_AI: 2026-03-02 - Remove merchant transaction summary from list
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

---

# 1️⃣ Merchant List Screen

## General Rules

- Displays ALL merchants
- NO pagination
- NO lazy loading

---

## Screen Layout Structure

### Top Section — Filters & Controls

Must include:


2. **Search Input**
   - Filters by merchant name  // this will work in ui
   - Case-insensitive
   - Instant filtering (debounced)


---

## Merchant List Item (Row/Card Design)

Each merchant row must display:

- Merchant Name (primary emphasis)
- Assigned Category (badge style)


Tapping a merchant navigates to the Merchant Detail Screen.

---



## Empty States

Define different UX states for:

1. No merchants exist
2. Merchants exist but none match filter/search

Each state must include:
- Title
- Supporting description
- Optional CTA

---

## Performance Constraints

Even without pagination/lazy loading, app must handle:

- 500–1000 merchants  

---

# Merchant Detail Page — Finance Module (Mobile Expense Tracking App)

## Purpose

This screen opens when the user taps a merchant from:

Finance → Merchants → Merchant List

It provides:

- Category reassignment capability  
- Nickname editing (optional)  
- Administrative merchant actions (optional: rename, delete, merge)  

This screen must be implementation-ready for React Native + .NET backend.

---

# 1️⃣ Navigation & Entry Rules

## Entry Points

- Merchant List Screen → Tap Merchant
- Deep link (optional future support)
