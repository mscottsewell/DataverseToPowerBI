# Screenshot Placeholder Guide

This guide maps each screenshot placeholder to its location and what to capture.

## How to find placeholders quickly

Search for `SCREENSHOT PLACEHOLDER` across the repo, or search specific IDs such as `SS-01`.

## Placeholder map

| ID | File | Section Anchor | What the screenshot should include |
|---|---|---|---|
| SS-01 | `README.md` | Step 5: Choose your Dimension Tables | Relationship selector with **Source Table** column visible, grouped relationship headers, and checked/unchecked examples. |
| SS-02 | `README.md` | Step 5: Choose your Dimension Tables | Cross-chain ambiguity state: amber/orange warning highlight and finish-time warning prompt. |
| SS-03 | `README.md` | Step 6: Customize the Queries | Expanded lookup group showing sub-columns (**ID/Name/Type/Yomi**) with Include/Hidden toggle states. |
| SS-04 | `README.md` | Storage Modes → Per-Table Storage Mode | Storage mode UI showing **Dual (All)** and **Dual (Select)** behavior (model-level mode and per-table differences). |
| SS-05 | `README.md` | Change Preview & Impact Analysis → Preview Features | Change Preview dialog showing grouped TreeView, impact badges, and right detail pane for selected item. |
| SS-06 | `docs/troubleshooting.md` | "Missing relationships in the model" | Troubleshooting-oriented relationship selector view showing Active vs Inactive indicators and a conflict example if available. |

## Suggested capture tips

- Use the same zoom level and theme across screenshots for consistency.
- Prefer realistic sample data names over placeholder lorem text.
- Avoid including sensitive tenant/environment identifiers.
- Capture enough context to show the feature, but crop to reduce clutter.

## After adding images

- Replace each placeholder block in the source markdown with the final image markdown.
- Keep the `SS-0X` IDs in captions or comments if you want future maintainers to keep traceability.
