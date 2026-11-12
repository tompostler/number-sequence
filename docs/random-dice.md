# Random dice

`GET /random/dice/{expression}` rolls standard tabletop dice notation and returns a `DiceRoll`: the normalized expression, the total, and every term with its individual rolls.
With no expression it rolls `1d20`.
It is exposed in the Client (`RandomOperations.RollDiceAsync`), the Tool (`random dice 4d6+2d4`) and a card on `/ui/random`.

## What is accepted

Terms of `NdS` (`N` defaults to 1, so `d20` is `1d20`) or plain integers, joined by `+` or `-`.
A leading sign is allowed.
Whitespace between tokens is ignored, but terms must be joined by an operator — `4d6 2d4` is an error rather than an implied `+`, since a space is most often a mangled `+` (below) and guessing would hide that.

Deliberately left out: keep/drop (`4d6kh3`, advantage as `2d20kh1`), exploding dice, multiplication, parentheses.
Simple sums cover what the page is for.
Keep/drop is the one most likely to be wanted next; it would be a suffix on a dice term, and `DiceRollTerm` would need to mark which rolls were dropped rather than just leave them out.

## Why the expression is a path segment

A query string decodes `+` as a space, so `random/dice?roll=4d6+2` would arrive as `4d6 2`.
A path segment keeps a literal `+`, so `curl .../random/dice/4d6+2` works unescaped.
The Client and the page still escape the expression (`Uri.EscapeDataString` / `encodeURIComponent`); `%2B` in a path decodes back to `+`.

## Limits

All in `DiceNotation`: 200 characters, 20 terms, 1,000 dice across the whole expression, 1,000,000 sides, constants up to 1,000,000.
They exist only so an anonymous endpoint can't be asked to roll a billion dice or overflow a total; the total is a `long` and well inside range at these limits.
Numbers over seven digits are rejected before parsing so an `int` never overflows.
A bad expression is a 400 with a user-facing message naming the position.

## UI

Fast roll only — the total, then the per-term breakdown.
A slow rolling animation was considered and skipped: nothing else on the random page animates.
The page script reads the response by property name (`total`, `terms`, `notation`, `subtract`, `sides`, `rolls`), so renaming a `DiceRoll`/`DiceRollTerm` property breaks the card at runtime, not at build.
