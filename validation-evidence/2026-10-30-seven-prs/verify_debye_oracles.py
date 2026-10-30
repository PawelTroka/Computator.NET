#!/usr/bin/env python3
"""Independently verify the six x=1 Debye binary64 constants in the fixtures."""
from decimal import Decimal, localcontext
from fractions import Fraction
from math import comb, factorial
import json

expected = [
    0.7775046341122482, 0.7078784756278292, 0.6744155640778147,
    0.6548740688867369, 0.6421002580217791, 0.6331114258349511,
]
bernoulli = [Fraction(1)]
for order in range(1, 80):
    bernoulli.append(-sum(comb(order + 1, k) * bernoulli[k] for k in range(order)) / (order + 1))


def debye_at_one(n, terms):
    # Integrate t/(exp(t)-1) = sum B_k*t^k/k! term by term:
    # D_n(1) = n * integral_0^1 t^n/(exp(t)-1) dt.
    return n * sum(bernoulli[k] / (factorial(k) * (n + k)) for k in range(terms))


results = []
with localcontext() as context:
    context.prec = 80
    for n, reference in enumerate(expected, start=1):
        lower, higher = debye_at_one(n, 60), debye_at_one(n, 80)
        delta = abs(higher - lower)
        assert delta < Fraction(1, 10**48), (n, delta)
        assert float(lower) == float(higher) == reference, (n, reference, float(higher))
        results.append({
            "order": n,
            "storedBinary64": reference,
            "decimal80": str(Decimal(higher.numerator) / Decimal(higher.denominator)),
            "differenceBetween60And80Terms": str(Decimal(delta.numerator) / Decimal(delta.denominator)),
        })
print(json.dumps({"passed": 6, "failed": 0, "results": results}, indent=2))
