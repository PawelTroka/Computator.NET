# Web workspace refresh

This change keeps the original logo bytes, Ace editor implementations/themes, mathematical expression transformation, route structure and execution components. It updates only the surrounding layout/navigation/CSS and adds appearance/accessibility checks to the existing focused browser suite.

The shell uses neutral charcoal surfaces, warm restrained controls alongside the Ambiance editor, a compact sidebar and a normal-flow mobile menu. Expression entry retains its existing light GitHub editor theme rather than modifying tested Ace internals. Focus indicators, a skip link, Escape-to-close navigation, wrapping result text and reduced-motion handling are included. There are no new fonts, dependencies, animations, external assets or numerical behavior changes.

`Web-Validate` now saves desktop/mobile screenshots alongside its browser reports, while all existing execution and real-API suites remain applicable. Review screenshots and the live candidate before merging; passing layout assertions is not full visual acceptance. The WebClient IDE-start fix is a separate preceding PR.
