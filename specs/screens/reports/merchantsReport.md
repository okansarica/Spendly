# Merchant Report – Feature Description

## Overview

The Merchant Report screen summarizes spending by merchant and is similar in structure to the existing `Categories Report`. It lets users quickly see where they spend the most and analyze spending trends.
---


## Merchant Report Screen

### Monthly Summary

At the top of the screen, display the user's **total spending across all merchants for the current month**.

- This value is retrieved from the **user's monthly spend table**.
- No direct database aggregation should be performed here.

Below this amount, display:

- **Total spending during the same period in the previous month**
- **Percentage change compared to the previous month**

Example structure:

```text
Total spent this month: $X
Last month (same period): $Y
Change: +12% / -8%
```

---

### Top Merchants Pie Chart

Below the monthly summary, display a **pie chart showing the top merchants**.

Rules:

- Show the **Top 9 merchants by total spending**
- Group all remaining merchants into **"Other"**
- The chart will therefore contain **10 segments in total (Top 9 + Other)**

Important:

- Merchant grouping must be done **in the Business Layer**
- **No grouping or aggregation should be performed at the database level**

---

### All Merchants List

Below the pie chart, display the **All Merchants** section. This section should behave similarly to the `Categories` list UI.

Rules:

- Each merchant should appear inside its own **box/card**
- Merchants must be **sorted alphabetically**
- The UI should include a **client-side search/filter** under the **"All Merchants"** title
- Filtering should happen **only in the UI**

Interaction:

- When a merchant is tapped, navigate to the **Merchant Detail** screen

---

## Merchant Detail Screen

When a merchant is selected, the user navigates to the **Merchant Detail** page. This screen is structurally similar to the `Category Detail` screen.

### Merchant Spending Summary

At the top of the screen display:

- **Total spending for this merchant (current period)**
- **Spending during the same period last month**
- **Percentage change between the two**

Example:

```text
Total spent at Amazon this month: $450
Last month (same period): $380
Change: +18%
```

---

### Transactions List

Below the summary, display a **list of all transactions made with this merchant**.

Rules:

- Transactions should be **sorted by name by default**
- Each transaction item should display the same information used in the `Categories` transaction list

---

### Filtering and Sorting

This screen must support the same filtering behavior used in other report detail screens.

Filter:

- Select a **custom date range**

Sort:

- Transaction name
- Amount
- Date

Sorting and filtering behavior should be consistent with existing report detail pages.

---

## Notes (Concise)

- Business logic: aggregation and grouping are performed in the Business Layer.
- UI: search/filter and alphabetical sorting operate client-side.
- DB: no direct aggregation should be performed at the database.
