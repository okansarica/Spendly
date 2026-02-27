
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

## Homepage Components
The homepage contains the following elements, all derived from the DailyCategoryAccountSummary data and recent normalized transactions:

### 1. Total Spending for the Previous Calendar Month
- Aggregates all records for the previous full calendar month.
- Includes spending from all accounts.

### 2. Account-Based Spending Pie Chart
- Groups spending by account for the previous calendar month.
- Returned as a dataset to build a pie chart.

### 3. Category-Based Spending Pie Chart
- Groups spending by category for the previous calendar month.
- Returned as a dataset to build a pie chart.
- UI may use a slider/toggle to switch between account-based and category-based views.

### 4. Six-Month Spending Trend (Bar Chart)
- Aggregates totals per month across the last 6 months.
- Includes spending from all accounts.
- Returned as a dataset suitable for a bar chart.

### 5. Latest 10 Expenses
- Returns the most recent 10 normalized transactions.
- Not aggregated; shown as individual expense entries on the homepage.

---

## Service Behavior
### Single Request
The homepage will be loaded using **one API call**. The backend service will:
- Query and assemble all homepage components into a single response object.
- Minimize database calls by using optimized MongoDB aggregations.

### Caching
- The homepage service method will be cached.
- The existing caching mechanism in the system will be used.
- Cache duration can follow application-level caching policies.

---

## Summary
The homepage service returns a complete dashboard snapshot using a single endpoint. It includes:
1. Previous month's total spending
2. Spending distribution by account (pie chart)
3. Spending distribution by category (pie chart)
4. Spending trend for the last six months (bar chart)
5. Latest 10 expenses

All aggregated data is derived from **DailyCategoryAccountSummary**, and recent transactions come from the normalized transaction collection.

