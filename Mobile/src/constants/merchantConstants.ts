// CHANGED_BY_AI: 2026-03-02 - Add merchant list constants
export const MerchantDefaults = {
  SearchDebounceMs: 300,
};

export const MerchantSortOptions = [
  {id: 'name.asc', sortBy: 'name', sortDirection: 'asc', labelKey: 'SortAlphabeticalAsc'},
  {id: 'name.desc', sortBy: 'name', sortDirection: 'desc', labelKey: 'SortAlphabeticalDesc'},
  {id: 'amount.desc', sortBy: 'amount', sortDirection: 'desc', labelKey: 'SortHighestSpending'},
  {id: 'amount.asc', sortBy: 'amount', sortDirection: 'asc', labelKey: 'SortLowestSpending'},
  {id: 'transactions.desc', sortBy: 'transactions', sortDirection: 'desc', labelKey: 'SortMostTransactions'},
  {id: 'transactions.asc', sortBy: 'transactions', sortDirection: 'asc', labelKey: 'SortLeastTransactions'},
];

