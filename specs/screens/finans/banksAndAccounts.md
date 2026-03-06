# Banks / Accounts Listing Screen – Implementation Guide

## 1. Navigation

The **Banks / Accounts Listing screen** must be added to the **Finance navigation screen**, positioned at the **top of the list**.

---

# 2. Listing Screen

This screen should display **banks and their accounts in a single list**, similar to the **Categories screen**.

### Structure
- A **bank can contain multiple accounts**.
- Accounts must appear **indented under their bank**, similar to how categories/sub categories are displayed.
- The design and layout should **match the Categories screen**.
- Each **bank and account item** must have a **3-dot overflow menu**, like the categories list.

### Visual Distinction

To visually differentiate **banks and accounts**, each list item should have a **thick colored left border**.

Example idea:
- **Bank items** → one color
- **Account items** → another color

Requirements:
- Colors must **match the existing design language**
- Must support **both Light Mode and Dark Mode**

---

# 3. Bank Menu Options

Each bank item must have the following menu options:

- **Update** → navigates to the **Bank Create/Update page**
- **Disconnect** → visible **only if `bank.isConnected == true`** (no interaction needed for now, just add a placeholder)
- **Connect** → visible **only if `bank.isConnected == false`** (no interaction needed for now, just add a placeholder)
- **Add Account** → navigates to the **Create Account page**
- **Delete**

---

# 4. Account Menu Options

Each account item must have the following menu options:

- **Delete**
    - Enabled **only if `account.isConnected == false`**

- **Upload**
    - Used to **upload statements for this account**
    - Enabled **only if `account.isConnected == false`**

- **Connect**
    - Visible **only if `bank.isConnected == true` AND `account.isConnected == false`**

- **Disconnect**
    - Visible **only if `bank.isConnected == true` AND `account.isConnected == true`**

- **Update**
    - Visible **only if `bank.isConnected == false`**

---

# 5. Backend Requirements

To support the listing screen:

- The **`BanksController`** must be implemented/extended in the backend.
- Required **service layer logic** must be written.
- Necessary **ViewModels** must be created.

The endpoint must return **banks and their accounts in a structure suitable for the UI list**.

---

# 6. Add Bank Button

A **"Add Bank" button** must be placed in the **top-right corner** of the screen.

Important rules:
- There must **NOT** be an **Add Account button** here.
- Accounts are created **only through a bank menu**.

---

# 7. Add Bank Flow

When the **Add Bank button** is pressed:

A **popup/modal** should appear.

The popup should:
- Ask the user whether they want to continue with:
    - **Open Banking**
    - **Manual Setup**

The popup should also include:
- A **short explanation of what Open Banking is**
- A **visually appealing design** 
- A layout that **encourages the user to choose Open Banking**

### Behaviour

- If **Open Banking** is selected:
    - Do **NOT implement the flow yet**
    - Add a **TODO placeholder**

- If **Manual Setup** is selected:
    - Navigate to the **Create Bank page**

---

# 8. Create / Update Bank Page

The **Create Bank** and **Update Bank** functionality should use the **same page**.

The behavior will depend on **whether the page is opened for creation or update**.

### Fields

**Bank Name (ComboBox)**

- Data must be loaded from the database
- Source table: **`BankDefinition`**
- This table contains **predefined bank names**

User options:
- There should be radio button on the left of both combo and input to let user choose between:
- Select a bank from the **dropdown** (Disable the input field when a bank is selected from the dropdown)
- Or **enter a custom name manually**

**Description**
- Optional text field

### Update Behaviour

When updating a bank:

- The **ComboBox must NOT be visible**
- If the bank was selected from `BankDefinition`
  (`Bank.BankDefinitionId` is not null):
    - The **bank name cannot be changed**
    - Only the **description** can be updated

### Save Button

- Must use the **Button component**
- Default behavior after save → **redirect back to the listing screen**

---

# 9. Create / Update Account Page

The **Create Account** and **Update Account** functionality should also use the **same page**.

### Fields

**Name**
- Text input
- **Required**

### Save Button

- Must use the **Button component**
- Default behavior after save → **redirect back to the listing screen**
