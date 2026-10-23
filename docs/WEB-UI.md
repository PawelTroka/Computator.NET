# Web workspace refresh

This change keeps the original logo bytes, Ace editor implementations/themes, mathematical expression transformation, route structure and execution components. It updates only the surrounding layout/navigation/CSS and adds appearance/accessibility checks to the existing focused browser suite.

The shell uses neutral charcoal surfaces, warm restrained controls alongside the Ambiance editor, a compact sidebar and a normal-flow mobile menu. Expression entry retains its existing light GitHub editor theme rather than modifying tested Ace internals. Focus indicators, a skip link, Escape-to-close navigation, wrapping result text and reduced-motion handling are included. There are no new fonts, dependencies, animations, external assets or numerical behavior changes.

`Web-Validate` now saves desktop/mobile screenshots alongside its browser reports, while all existing execution and real-API suites remain applicable. Review screenshots and the live candidate before merging; passing layout assertions is not full visual acceptance. The WebClient IDE-start fix is a separate preceding PR.

## Drafts while switching tools

Calculate, Chart and Numerical calculations each keep their own expression and input values while navigating within the loaded page, including browser Back/Forward. You can visit Custom functions to add or edit a definition and return to the work you were preparing. Numerical calculations also retains the selected operation and method; the available methods are refreshed on return, and a removed choice falls back to the first currently available method.

Leaving a tool still cancels its pending request and clears its result, error and rendered chart. Returning does not execute anything automatically. The next explicit calculation or chart uses the retained inputs and current custom-function definitions. Clearing an expression or resetting a parameter remains effective across navigation. These three drafts are kept in memory independently in each tab and start from their defaults after a full page reload; script-document storage and import/export retain their separate behavior.
