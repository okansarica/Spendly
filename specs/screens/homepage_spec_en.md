// CHANGED_BY_AI: 2026-03-02 - Update homepage spec interactions and empty states
# Homepage Specification

This document describes the functional requirements of the homepage screen. It defines the data sources, components, and behavior expected from the backend service that provides the homepage response.

---

## Data Source

All homepage data will be generated from the following MongoDB collection:

```csharp
public class DailyCategoryAccountSummary : BaseEntity
{
    public ObjectId UserId { get; set; }
    public DateTime Date { get; set; }         // Day of the record
    public ObjectId CategoryId { get; set; }   // Category identifier (e.g., "market")
    public ObjectId AccountId { get; set; }    // Account identifier (e.g., "hsbc_credit")
    public decimal TotalAmount { get; set; }   // Total amount spent on that day, account, and category
}
```

This collection stores pre-aggregated daily totals per user, account, and category.

---

## Application Purpose

This application is a **mobile expense tracking (gider takip) application** focused exclusively on expense management.

> ⚠️ Income is NOT tracked. All analytics and summaries are expense-based only.

---

## Homepage UX Principles

- Data-dense but visually clean.
- Fast-loading (single API).
- Scroll-based vertical layout.
- Swipe interactions inside chart components.
- Pull-to-refresh enabled.
- Friendly and motivational empty states.
- Expense-focused insights.
- Professional financial dashboard aesthetic (clean typography, refined spacing, modern visuals).

---

## Shared Currency Formatting Component (Mandatory)

All monetary values displayed on the homepage **must use a shared formatting utility/component**.

### Requirements

Create a shared function/component:

```
formatCurrency(amount: number, options?: { fontSize?, fontWeight?, color? })
```

### Rules

- Must format values using **United Kingdom locale** (`en-GB`).
- Must use **GBP (£)** currency symbol.
- Example output:
  - `£1,240.50`
  - `£98.00`
- Should internally use:
  `Intl.NumberFormat("en-GB", { style: "currency", currency: "GBP" })`

### Design Rules

- The function is responsible ONLY for:
  - Formatting number
  - Adding currency symbol
- It must NOT hardcode:
  - Font size
  - Font weight
  - Color
  - Layout styling
- Typography and visual styling must be controlled by the consumer component.

All money displayed anywhere on homepage must go through this formatter:
- KPI card
- Pie charts tooltips
- Trend charts
- Weekly snapshot
- Top category
- Highest expense
- Latest transactions
- Daily average

No raw numbers should appear in UI.

---

# Homepage Components

All components are derived from:

- `DailyCategoryAccountSummary`
- Normalized transaction collection

---

# 1. Current Month Total Spending (Primary KPI Card)

This section is displayed as a **single unified card component**.

## KPI Card Structure

The card contains:

### Primary Value (Large & Dominant)

- Current calendar month total spending.
- Visually emphasized (largest font size on screen).
- Must use shared currency formatter.

### Secondary Value (Smaller & Subtle)

- Previous full month's total spending.
- Displayed directly below the primary value.
- Lower visual weight (smaller font, muted color).
- Must use shared currency formatter.

### Comparison Indicator (Inline or Bottom Section)

Mid-month comparison (current month vs same-day previous month).

Includes:
- Percentage change
- Up/Down icon
- Color indicator:
  - 🟢 Decrease
  - 🔴 Increase
  - ⚪ Neutral

## Layout Rules

- Entire section must be contained inside **one single card container**.
- Card should include:
  - Padding
  - Rounded corners
  - Light shadow
- Secondary value must not appear as a separate component.
- Comparison indicator visually belongs to the same card.

This card acts as the financial summary anchor of the homepage.

---

## Mid-Month Smart Comparison

If today is, for example, the 10th:

The system compares:
- Current month (Day 1 → Today)
- Previous month (Day 1 → Same day number)

Indicator logic:
- 🟢 Green → Spending decreased
- 🔴 Red → Spending increased
- ⚪ Neutral → No significant change

Includes:
- Percentage difference
- Small trend icon (↑ ↓)
- Optional micro sparkline (mini 10-day trend)

---

# 2. Account-Based Spending Pie Chart (Swipe Enabled)

- Aggregated by account.
- Default view: Current month.
- Horizontal swipe:
  - Swipe left → Previous month
  - Swipe right → Return to current month

## Visualization Requirements

- Must appear professional and finance-grade.
- Use modern color palette (muted, non-cartoonish).
- Avoid overly bright colors.
- Smooth shadowing and depth.

### 3D Effect (If Supported by Chart Library)

- Subtle 3D depth.
- Soft lighting effect.
- No exaggerated perspective distortion.
- Clean edge rendering.

If true 3D is not supported:
- Simulate depth using:
  - Gradient fills
  - Inner shadow
  - Soft outer shadow

## Data Rules

- Show top 5 accounts + "Other"
- Show percentage + formatted amount
- Tooltip must use shared currency formatter
- Slice taps must not navigate; chart is view-only

---

# 3. Category-Based Spending Pie Chart (Swipe Enabled)

- Aggregated by category.
- Default view: Current month.
- Horizontal swipe supported.

## Visualization Rules

- Same professional 3D/gradient rules as account pie chart.
- Highlight largest category visually:
  - Slight elevation
  - Stronger border
  - Slight outward offset

## Data Rules

- Top 6 categories + "Other"
- Show percentage + formatted amount
- Slice taps must not navigate; chart is view-only

---

# 4. Six-Month Spending Trend (Dual Bar Chart)

Covers last 6 months.

For each month:
- Active bar → That month total
- Faded bar → Previous month total

Purpose:
- Month-over-month clarity
- Immediate growth/decline visibility

Enhancements:
- Show percentage change above bars
- Tap a month → open detailed report
- Tooltip values must use shared currency formatter
- Subtle gradient fill for modern feel

---

# 5. Latest 10 Expenses

- Most recent 10 normalized transactions.
- Sorted by date descending.

Each row contains:
- Merchant name
- Category badge
- Account indicator
- Formatted amount (via shared formatter)
- Date

Enhancements:
- No swipe actions
- Rows are view-only and do not navigate

---

# 6. Additional Data Blocks (Increase Data Density)

## A. Top Spending Category (This Month)

- Category name
- Total spent (formatted)
- % of total spending

## B. Highest Single Expense (This Month)

- Merchant
- Formatted amount
- Date

## C. Most Used Account (This Month)

- Account name
- % share

## D. Weekly Snapshot

- This week total (formatted)
- Previous week total (formatted)
- % difference indicator

## E. Daily Average (Current Month)

- Average daily spending (formatted)
- Compared to previous month daily average

All monetary values must use the shared currency formatter.

---

# 7. Empty State Behavior

If user has no expense data:

Display a friendly empty state:

- Large illustration/icon (wallet, empty piggy bank, etc.)
- Single friendly text line
- No CTA buttons

Empty states apply separately to:
- Entire homepage
- Charts
- Transaction list

Charts must NOT render empty graphs.
Instead show:
- Friendly icon
- “No data for this period”

---

# 8. Pull-to-Refresh

The homepage must support **pull-down refresh gesture**.

Behavior:
- Pulling down triggers full homepage API refresh.
- Shows loading indicator.
- Updates:
  - Totals
  - Charts
  - Latest expenses
- Resets mobile cache.

---

# 9. Service Behavior

## Single Request Architecture

Homepage loads with **one API call** returning:

- Current month total
- Previous month total
- Mid-month comparison metrics
- Account distribution (current + previous month)
- Category distribution (current + previous month)
- Six-month trend dataset
- Weekly snapshot
- Top category
- Highest expense
- Most used account
- Daily average metric
- Latest 10 expenses

---

# 10. Caching Strategy

## Backend Caching

- Homepage response cached.
- Invalidated when:
  - New transaction imported
  - Transaction updated/deleted
- Short TTL (5–10 minutes acceptable).

## Mobile App Caching

- Homepage cached in app state.
- No auto refetch on tab navigation.
- Refetch only:
  - Pull-to-refresh
  - After import
  - After transaction modification
  - Cache expiration

---

# 11. Performance Rules

- All heavy aggregation must use MongoDB aggregation pipelines.
- No per-account or per-category loop queries.
- Top-N logic handled in aggregation.
- Use indexed fields:
  - UserId
  - Date
  - AccountId
  - CategoryId

---

# Summary

Homepage is a data-rich, insight-driven financial dashboard.

It includes:

1. Current month total (primary KPI)
2. Previous month total
3. Mid-month smart comparison
4. Account distribution (swipe-enabled, professional 3D/gradient style)
5. Category distribution (swipe-enabled, professional 3D/gradient style)
6. Six-month dual bar trend
7. Weekly snapshot
8. Top category
9. Highest single expense
10. Most used account
11. Daily average metric
12. Latest 10 expenses
13. Friendly empty states
14. Pull-to-refresh support
15. Shared UK currency formatting utility

All data is expense-only.

Income is never calculated or displayed.


---

# 🚨 STRICT IMPLEMENTATION CONTRACT (NON-NEGOTIABLE)

The AI MUST follow these rules exactly.

## 1. Pie Chart Aggregation Rules (ABSOLUTE)

### Account Pie Chart
- MUST aggregate by `AccountId`
- MUST NOT aggregate by month
- MUST NOT aggregate by date
- MUST NOT aggregate by category
- Data grouping key = AccountId

### Category Pie Chart
- MUST aggregate by `CategoryId`
- MUST NOT aggregate by month
- MUST NOT aggregate by account
- MUST NOT aggregate by date
- Data grouping key = CategoryId

If aggregation key is incorrect, implementation is INVALID.

---

## 2. Swipe Behavior (MANDATORY)

Both pie charts MUST support horizontal swipe interaction.

### Required Swipe Logic

Default:
- Show Current Month

Swipe Left:
- Show Previous Month

Swipe Right:
- Return to Current Month

If swipe interaction is missing, implementation is INVALID.

---

## 3. No Reinterpretation Rule

The AI is NOT allowed to:
- Redesign chart logic
- Change aggregation keys
- Replace swipe with buttons
- Replace swipe with dropdown
- Convert pie chart into bar chart
- Change chart type

Only implement what is specified.

---

## 4. Data Source Constraint

Pie charts MUST use:

- `DailyCategoryAccountSummary`
- Filtered by:
  - UserId
  - Date range (current or previous month)

Then grouped ONLY by:
- AccountId (for account chart)
- CategoryId (for category chart)

---

## 5. Acceptance Criteria

Implementation is considered correct only if:

✔ Account pie chart slices represent accounts  
✔ Category pie chart slices represent categories  
✔ Swipe changes month dataset  
✔ No month-based aggregation inside pie charts  
✔ Shared currency formatter used everywhere  
✔ Pie chart slices do not navigate  
✔ Latest expenses have no swipe actions

If any of the above is not true, implementation is WRONG.
