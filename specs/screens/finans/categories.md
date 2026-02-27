# AI Prompt -- Finance Module Category Management

## Your Role

You are a Senior Product Designer + UX Architect + Mobile UI Engineer.
Based on the requirements below, you will comprehensively design
production-ready mobile screen flows, user scenarios, edge cases, and
component behaviors.

Goal: Produce the **Category Management** flow in the Finance module end-to-end.

Platform: Mobile application  
Domain: Expense Tracking (Expenses only, no income)  
Scale: Merchant count can grow to 1000+ over time

---

# 1️⃣ Category List Screen (Entry Point)

## Top Area

- Title: "Categories"
- Top right: ➕ New Category button

## Content List

Each category card should display:
- Category name
- Number of merchants linked to this category
- User-configurable color / icon

Behavior:
- Tapping a card → navigates to the Category Edit/ New Category screen.
- Category count will be around 10 mostly, no paging, lazy loading required. must be sorted alphabetically
- Categories will be 2 leveled parent->child so there should be a button to add a child category in the items of list

Edge Cases:
- Empty state if no categories exist
- Filter categories with an input at the top of the page

---

# 2️⃣ New Category Creation (Create)

Categories may be inserted with merchants

Fields:
- Category name (required)
- Color picker
- Icon picker (optional)
- If this is a child category then parent category name should be displayed as well

## Merchant List

Each merchant row should show:
- Merchant name
- Transaction count
- Last transaction date

Gesture:
- Swipe right to reveal:
  - "Remove link"

Validations:
- Creating a category with a duplicate name
- Empty name

---

# 3️⃣ Category Edit Screen

This is not a new screen, we need to add the edit behaviour to new category screen. Depending on the parameter we pass when opening from categories screen we should open in new or edit behaviour


---

# 4️⃣ Merchant-Category Linking Flow

Category Edit → "+ Add Merchant"

When the button is pressed:

A full-screen selection screen must open. Popups or bottom sheets must NOT be used.

Reason:
- Large amount of data
- Filtering is required
- Bulk selection is required

List Shown:
- Only uncategorized merchants

---

# 5️⃣ Add Merchant Screen Structure

## 1️⃣ Search Bar (Required)

- Search by merchant name
- Real-time filtering
- No paging no lazy load

## 2️⃣ Filters (Top-right filter button)

Filter options:
- Merchants with transactions in the last 30 days
- Merchants with highest spending
- Merchants with highest transaction count


## 3️⃣ List Prioritization

Default sort should be alphabetical.

Recommended sort order:
1. Most transactions
2. Active in last 30 days
3. Alphabeetical

## 4️⃣ Multi-Select UX

Each row contains:  
☐ Checkbox  
Merchant name  
Transaction count  
Total spending

When a selection is made:

A fixed bottom action bar must appear:

**"X Merchants Selected — Assign"**

This bar must be fixed at the bottom of the screen. It must not disappear when the user scrolls.

---


# 7️⃣ Expected Output

The AI should produce:

- Screen hierarchy
- User flow diagram
- Component list
- State management requirements
- Edge case scenarios
- Empty / Error state designs
- Validation rules
- API requirements list
- Data model proposal (Category & Merchant relationship)
- UX risk analysis
- Future expansion plan for hierarchical categories

The design must be production-ready from both a UX and system architecture perspective.
