// CHANGED_BY_AI: 2026-03-02 - Add shared GBP currency formatter
const gbpFormatter = new Intl.NumberFormat('en-GB', {style: 'currency', currency: 'GBP'});

export const formatCurrency = (amount: number | undefined) => {
  const value = amount ?? 0;
  return gbpFormatter.format(value);
};

