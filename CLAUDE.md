# OpenWallet — Code Rules

## C# Style

- **Never use `var`** — always declare the explicit type.
- **No inline comments** — do not add comments inside method bodies. XML doc comments (`///`) on public methods are allowed when the behaviour is non-obvious.
- Prefer expression-bodied members for single-line methods and properties.

## Blazor

- **Always prefer reusable components** — if the same UI pattern appears more than once, extract it into a component under `OpenWallet.Client/Components/`.
- Components accept strongly-typed parameters; never pass raw strings where a typed DTO or enum fits.
- Use `EventCallback<T>` for child-to-parent communication.

## Database / EF Core

- Use PostgreSQL-native types where appropriate (e.g. `NpgsqlPoint` for geo-coordinates instead of separate `double` columns).

## General

- No `var` anywhere in the codebase — this applies to all projects (server, client, shared).
- Strong types all the way: avoid `object`, `dynamic`, and untyped collections.
