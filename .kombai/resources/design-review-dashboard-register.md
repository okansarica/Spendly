# Design Review Results: DashboardScreen & RegisterScreen

**Review Date**: 2026-03-17
**Screens**: `/dashboard` · `/auth/register`
**Focus Areas**: Visual Design · UX/Usability · Accessibility · Micro-interactions/Motion

> **Note**: This review was conducted through static code analysis only (React Native app — no browser preview available). Visual inspection on a live device would surface additional layout rendering and interactive behavior issues.

---

## Summary

Both screens follow a solid theming foundation with well-structured design tokens and consistent card-based layouts. However, a **critical production bug** exists in RegisterScreen (dev flag left enabled), and there are widespread accessibility gaps across interactive elements, inline style performance issues, and several UX affordances missing from the Dashboard. Addressing the critical and high-severity items will meaningfully improve quality, safety, and inclusivity.

---

## Issues

| # | Issue | Criticality | Category | Location |
|---|-------|-------------|----------|----------|
| 1 | `DEV_AUTO_FILL = true` is enabled in production — auto-fills form with test user data and a weak password `'123'`, exposing test credentials to real users | 🔴 Critical | UX/Usability | `Mobile/src/screens/auth/RegisterScreen.tsx:32,48` |
| 2 | `buttonPrimaryDisabled` in dark mode is `#1E40AF` — identical to `buttonPrimary`. Disabled buttons are visually indistinguishable from enabled ones in dark mode, violating WCAG 1.4.3 | 🟠 High | Accessibility | `Mobile/src/theme/colors.ts:54` |
| 3 | `StyleSheet.create(...)` is called directly inside component body on every render across all components. Styles depending on theme should be wrapped in `useMemo` or hoisted to module-level for static values — causes unnecessary object recreation on every render | 🟠 High | Visual Design | `Mobile/src/screens/dashboard/DashboardScreen.tsx:80`, `Mobile/src/screens/auth/RegisterScreen.tsx:91`, `Mobile/src/components/Header.tsx:45`, `Mobile/src/components/Button.tsx:90` |
| 4 | Header icon buttons (back arrow, theme toggle) have no `accessibilityLabel` or `accessibilityRole` — screen readers cannot announce their purpose | 🟠 High | Accessibility | `Mobile/src/components/Header.tsx:113-128` |
| 5 | RegisterScreen submit button (`TouchableOpacity`) has no `accessibilityRole="button"` or `accessibilityLabel` — screen readers will not identify it as a button | 🟠 High | Accessibility | `Mobile/src/screens/auth/RegisterScreen.tsx:175` |
| 6 | No inline field-level validation feedback — the only signal of invalid input is a disabled button with no explanation. Fields accept any value silently (e.g., malformed email, empty name) | 🟠 High | UX/Usability | `Mobile/src/screens/auth/RegisterScreen.tsx:51,140-173` |
| 7 | Password field has no show/hide toggle (`secureTextEntry` only) — standard mobile UX expectation missing, frustrating on registration where users can't verify their password | 🟠 High | UX/Usability | `Mobile/src/screens/auth/RegisterScreen.tsx:167-173` |
| 8 | RegisterScreen submit button reimplements button styles inline (`TouchableOpacity` + inline `StyleSheet`) instead of using the existing reusable `Button` component — inconsistent with the rest of the app | 🟡 Medium | Visual Design | `Mobile/src/screens/auth/RegisterScreen.tsx:117-125,175-177` |
| 9 | Hero card spending value uses hardcoded `fontSize: 42` instead of a design token — breaks type scale consistency and won't respect future theme changes | 🟡 Medium | Visual Design | `Mobile/src/screens/dashboard/DashboardScreen.tsx:126` |
| 10 | `SubscriptionPlansModal` title uses `fontSizes.xxl + 2` arithmetic on a design token — should be a named token (e.g., `fontSizes.xxxl`) added to the theme | 🟡 Medium | Visual Design | `Mobile/src/components/SubscriptionPlansModal.tsx:185` |
| 11 | Header subscription warning banner uses hardcoded `#FFA500` orange — not a theme token, will not adapt to dark/light mode and breaks design system consistency | 🟡 Medium | Visual Design | `Mobile/src/components/Header.tsx:96` |
| 12 | `SocialLoginButtons` uses hardcoded `#5F6368` for button text color — bypasses theme tokens, won't adapt to theme changes | 🟡 Medium | Visual Design | `Mobile/src/components/SocialLoginButtons.tsx:67` |
| 13 | No "See all" or navigation affordance on the **Latest Expenses** card — users can only see the truncated list with no path to browse more transactions | 🟡 Medium | UX/Usability | `Mobile/src/screens/dashboard/DashboardScreen.tsx:594-621` |
| 14 | Horizontal chart scroll (Spending by Account/Category) has no swipe affordance — pagination dots exist but there is no peeking next chart, no arrow hint, no label. Users may not discover it is scrollable | 🟡 Medium | UX/Usability | `Mobile/src/screens/dashboard/DashboardScreen.tsx:466-500` |
| 15 | Six-month trend percentage change row renders raw percentages above the bar chart with no label or sign indicator explaining what they represent (MoM change? YoY?) | 🟡 Medium | UX/Usability | `Mobile/src/screens/dashboard/DashboardScreen.tsx:562-566` |
| 16 | No confirm password field — users cannot detect registration typos. Standard registration UX requirement | 🟡 Medium | UX/Usability | `Mobile/src/screens/auth/RegisterScreen.tsx:140-173` |
| 17 | Social login `TouchableOpacity` elements have no `accessibilityLabel` — screen readers will not announce "Continue with Google" / "Continue with Facebook" | 🟡 Medium | Accessibility | `Mobile/src/components/SocialLoginButtons.tsx:78-87` |
| 18 | No skeleton/shimmer loading state — a full-page `ActivityIndicator` is shown while all data loads, which provides poor perceived performance | ⚪ Low | Micro-interactions | `Mobile/src/screens/dashboard/DashboardScreen.tsx:323-331` |
| 19 | Latest Expenses list items are plain `View`s with no press handler — if tapping should navigate to a transaction detail screen, they need `TouchableOpacity` / `Pressable` | ⚪ Low | UX/Usability | `Mobile/src/screens/dashboard/DashboardScreen.tsx:599-616` |
| 20 | Header subscription warning banner has no visual tap affordance (no chevron `›`, no underline, no icon) — users may not realize it is tappable | ⚪ Low | Micro-interactions | `Mobile/src/components/Header.tsx:132-139` |
| 21 | No animated transition between loading → error → content states on Dashboard — state changes are abrupt. A simple `FadeIn` or `Animated.timing` would significantly improve feel | ⚪ Low | Micro-interactions | `Mobile/src/screens/dashboard/DashboardScreen.tsx:323-348` |
| 22 | Comparison badge and info card trend indicators use ASCII `^` / `v` characters instead of Unicode arrows `↑` / `↓` or `react-native-vector-icons` arrow icons — looks unpolished | ⚪ Low | Visual Design | `Mobile/src/screens/dashboard/DashboardScreen.tsx:387-390,400-402` |
| 23 | Empty state `Icon` has no `accessibilityLabel` — screen readers will announce nothing for the illustration | ⚪ Low | Accessibility | `Mobile/src/screens/dashboard/DashboardScreen.tsx:352-355` |
| 24 | `ErrorDisplay` error icon has no `accessibilityLabel` | ⚪ Low | Accessibility | `Mobile/src/components/ErrorDisplay.tsx:28-30` |

---

## Criticality Legend

- 🔴 **Critical**: Breaks functionality, ships broken/insecure data to users, or causes regressions
- 🟠 **High**: Significantly impacts user experience, performance, or violates accessibility standards
- 🟡 **Medium**: Noticeable issue that degrades quality or consistency and should be addressed
- ⚪ **Low**: Nice-to-have improvement or polish item

---

## Next Steps

### Immediate (Critical + High — address before next release)

1. **Fix `DEV_AUTO_FILL`** — Set `DEV_AUTO_FILL = false` in `RegisterScreen.tsx:32`. Consider guarding behind `__DEV__` flag so it is safely usable in development only.
2. **Fix dark mode disabled button** — Change `buttonPrimaryDisabled` in `darkColors` to a visually distinct value such as `#374BA0` or `#4B5563`.
3. **Add `accessibilityLabel` + `accessibilityRole`** to all `TouchableOpacity` / icon buttons across `Header`, `RegisterScreen`, and `SocialLoginButtons`.
4. **Add inline validation** to RegisterScreen — show field-level error messages for empty fields and invalid email format.
5. **Add password visibility toggle** to the password field in RegisterScreen.
6. **Wrap `StyleSheet.create` in `useMemo`** across components where theme-dependent styles are created inside the component body.

### Short-term (Medium)

7. Replace inline `TouchableOpacity` in RegisterScreen with the existing `Button` component.
8. Add `fontSizes.xxxl` token to the theme and use it in `SubscriptionPlansModal`.
9. Add `warningOrange` or `warning` color token to the theme and replace `#FFA500` in `Header`.
10. Replace hardcoded `#5F6368` in `SocialLoginButtons` with `colors.textSecondary`.
11. Add a "See all transactions →" link or chevron to the Latest Expenses card.
12. Peek the next chart or show a subtle arrow indicator alongside pagination dots.
13. Add a label row or tooltip above the trend percentage row explaining "MoM change".
14. Add confirm password field to RegisterScreen.

### Polish (Low)

15. Introduce skeleton loaders for Dashboard cards.
16. Make Latest Expense items tappable if a detail screen exists.
17. Add a `›` or `arrow-forward` icon to the subscription warning banner.
18. Add fade/slide transitions between loading/error/content states.
19. Replace `^`/`v` ASCII arrows with `↑`/`↓` or `arrow-upward`/`arrow-downward` icons.
20. Add `accessibilityLabel` to empty state icons and `ErrorDisplay` icon.
