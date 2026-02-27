# Expense Tracking App -- Reports Page Specification

## Overview

This document defines the functional and API requirements for the
Reports Page of a mobile expense tracking application.

The application tracks expenses only (no income calculations).

All month comparisons must be based on: Month-To-Date (MTD) vs Previous
Month-To-Date (Previous MTD).

If today is April 14: - Compare April 1--14 - With March 1--14 - NOT
full March vs April 1--14

------------------------------------------------------------------------

# Reports Page -- Functional Requirements

## 1. Summary Card

Displays total spending for the current month-to-date and compares it
with the same day range of the previous month.

### Data Fields

-   currentMonthToDateTotal
-   previousMonthSamePeriodTotal
-   differenceAmount
-   percentageChange
-   trend (increase \| decrease \| neutral)
-   isNewSpending (boolean if previous month total is zero)

### Calculation

percentageChange = (currentMTD - previousMTD) / previousMTD \* 100

If previousMTD == 0: - percentageChange = null - isNewSpending = true

Color Rules: - Increase → Red - Decrease → Green - Equal → Neutral

------------------------------------------------------------------------

## 2. Top Changing Categories (Bar Chart)

Displays top 3--5 categories sorted by highest absolute difference
between: - Current MTD - Previous MTD (same date range)

Sorting: abs(currentMTD - previousMTD) DESC

Data per category: - categoryId - categoryName -
currentMonthToDateTotal - previousMonthSamePeriodTotal -
differenceAmount - percentageChange

------------------------------------------------------------------------

## 3. Category Distribution (Pie Chart)

Displays category distribution for current MTD only.

Fields: - categoryId - categoryName - currentMonthToDateTotal -
percentageOfTotal

percentageOfTotal = categoryCurrentMTD / totalCurrentMTD

------------------------------------------------------------------------

## 4. Full Category List

Sorted by currentMonthToDateTotal DESC.

Fields per category: - categoryId - categoryName -
currentMonthToDateTotal - previousMonthSamePeriodTotal -
differenceAmount - percentageChange

Each item should show: - Percentage change - Color indicator - Previous
month same period total

------------------------------------------------------------------------

# Category Detail Page

Opened when a category is selected.

## 1. Category Summary

Displays: - Category name - Current MTD total

------------------------------------------------------------------------

## 2. Transaction List

Shows all transactions for the selected category.

Filters: - Date range - Account (multi-select)

Sorting: - By date (asc/desc) - By amount (asc/desc)

Pagination required.

Transaction fields: - transactionId - date - merchantName -
accountName - amount 

------------------------------------------------------------------------

# API Endpoints

## GET /api/reports/overview

Returns: - Summary card data - Top changing categories - Category
distribution - Full category list

## GET /api/reports/category/{categoryId}

Query parameters: - startDate (optional) - endDate (optional) -
accountIds (optional array) - sortBy (date \| amount) - sortDirection
(asc \| desc) - page - pageSize

Response: - Category summary - Paginated transactions - Pagination
metadata

------------------------------------------------------------------------

# Technical Requirements

1.  All date comparisons must respect user timezone.
2.  All calculations must be performed server-side.
3.  MTD comparison must always align day-of-month ranges.
4.  Percentage calculations must safely handle division-by-zero cases.
