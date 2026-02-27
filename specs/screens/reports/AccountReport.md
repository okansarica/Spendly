# Expense Tracking App -- Account Reports Page Specification

## Overview

This document defines the functional and API requirements for the
Account Reports Page of the mobile expense tracking application.

The application tracks expenses only (no income calculations).

All month comparisons must be based on: Month-To-Date (MTD) vs Previous
Month-To-Date (Previous MTD).

If today is April 14: - Compare April 1--14 - With March 1--14 - NOT
full March vs April 1--14

All calculations must respect the user's timezone.

------------------------------------------------------------------------

# Account Reports -- Main Page

## 1. Top Summary Card

Displays total spending across ALL accounts for the current
month-to-date.

### Data Fields

-   currentMonthToDateTotal
-   previousMonthSamePeriodTotal
-   differenceAmount
-   percentageChange
-   trend (increase \| decrease \| neutral)
-   isNewSpending (boolean if previous period total is zero)

### Calculation

percentageChange = (currentMTD - previousMTD) / previousMTD \* 100

If previousMTD == 0: - percentageChange = null - isNewSpending = true

### UI Behavior

-   Increase → Red indicator
-   Decrease → Green indicator
-   Equal → Neutral

------------------------------------------------------------------------

## 2. Account Distribution (Pie Chart)

Displays spending distribution per account for the current MTD.

### Data Fields per Account

-   accountId
-   accountName
-   currentMonthToDateTotal
-   percentageOfTotal

percentageOfTotal = accountCurrentMTD / totalCurrentMTD

------------------------------------------------------------------------

## 3. Account List

Displays all accounts sorted by:

currentMonthToDateTotal DESC

### Data per Account

-   accountId
-   accountName
-   currentMonthToDateTotal
-   previousMonthSamePeriodTotal
-   differenceAmount
-   percentageChange

Each row must show: - Total current MTD spending - Previous month same
period total - Percentage change - Color indicator (increase red,
decrease green)

Selecting an account navigates to the Account Detail Page.

------------------------------------------------------------------------

# Account Detail Page

Opened when a user selects an account from the list.

Default date range: Current calendar month.

------------------------------------------------------------------------

## 1. Account Summary Section

Displays:

-   Account name
-   Selected date range
-   Total spending for selected range

If the selected range is current month: - Also show previous month same
period comparison - Show difference amount - Show percentage change -
Show colored trend indicator

------------------------------------------------------------------------

## 2. Filters

Required filters:

-   Date range selector
-   (Optional future extension: Category filter)

------------------------------------------------------------------------

## 3. Categories List (Within Selected Account)

Displays categories belonging to the selected account within the
selected date range.

Sorted by:

categoryTotal DESC

### Data per Category

-   categoryId
-   categoryName
-   totalAmount

------------------------------------------------------------------------

## 4. Category Navigation

When a category is selected:

Navigate to the existing Category Detail Page.

The following filters must be pre-applied:

-   accountId = selected account
-   date range = currently selected range

All sorting and filtering behavior must remain consistent with the main
Category Detail implementation.

------------------------------------------------------------------------

# API Endpoints

## GET /api/reports/accounts/overview

Returns:

-   Summary card data
-   Account distribution
-   Account list

------------------------------------------------------------------------

## GET /api/reports/accounts/{accountId}

Query parameters:

-   startDate (optional)
-   endDate (optional)

Returns:

-   Account summary
-   Category list (descending)
-   Optional previous month comparison (if date range is current month)

------------------------------------------------------------------------

# Technical Requirements

1.  All comparisons must align day-of-month ranges for MTD.
2.  All calculations must be performed server-side.
3.  Timezone must be respected per user.
4.  Percentage calculations must safely handle division-by-zero cases.
5.  All list endpoints must support scalability and pagination
    readiness.
