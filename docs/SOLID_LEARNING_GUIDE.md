# SOLID Learning Guide (RodneyPortfolio + ChurchWebsite)

This document explains **why** the recent refactors were made, **how** they map to SOLID, and gives interview-ready definitions and examples.

---

## 1) SOLID Acronym: Technical and Plain-English Meanings

## S — Single Responsibility Principle
- **Technical:** A class/module should have only one reason to change (one cohesive responsibility).
- **Plain English:** One thing should do one job well.

## O — Open/Closed Principle
- **Technical:** Software entities should be open for extension but closed for modification.
- **Plain English:** Add new behavior by plugging in new code, not by rewriting old working code.

## L — Liskov Substitution Principle
- **Technical:** Subtypes must be replaceable for their base types without breaking correctness.
- **Plain English:** If code expects a type/interface, any implementation should work the same way.

## I — Interface Segregation Principle
- **Technical:** Clients should not be forced to depend on methods they do not use; prefer focused interfaces.
- **Plain English:** Keep interfaces small and specific, not huge “do-everything” contracts.

## D — Dependency Inversion Principle
- **Technical:** High-level modules should depend on abstractions, not concrete implementations.
- **Plain English:** Depend on interfaces/contracts, not hardcoded classes.

---

## 2) Why These Changes Were Made

The refactor focused on replacing rigid coupling and duplication with abstractions and single sources of truth.

## A) ChurchWebsite service abstractions (DIP + OCP)
- Introduced:
  - `IEventService`, `IGroupService`, `ISermonService`, `ILocationService`
- Updated page models to inject interfaces instead of concrete services.
- Updated DI in `ChurchWebsite/Program.cs` to bind interface -> implementation.

**Why this matters**
- Before: page models were tightly coupled to specific classes.
- After: page models depend on contracts, so you can swap in-memory services for DB/API implementations with minimal changes.
- SOLID impact:
  - **DIP:** high-level pages now depend on abstractions.
  - **OCP:** easier extension with new implementations.

## B) Destination configuration re-centralization (SRP + OCP + DIP)
- Removed duplicated hardcoded church destination logic in view/JS.
- Re-used destination values from `ChurchSettings` + page `data-*` attributes.
- `location-route.js` consumes destination from page-provided data.

**Why this matters**
- Before: destination data existed in multiple places; one update required many edits.
- After: one source of truth in settings/model flow.
- SOLID impact:
  - **SRP:** configuration lives in configuration models, not scattered constants.
  - **OCP:** destination can change by config without rewriting routing logic.
  - **DIP:** route logic depends on data contract (`data-*`) instead of hardcoded constants.

## C) Guardrails refactor in API (SRP + DIP + ISP)
- Converted static guardrails to injectable services:
  - `IInputValidator` + `InputValidator`
  - `IContentFilter` + `ContentFilter`
- `ChatController` now consumes interfaces via DI.

**Why this matters**
- Before: controller was directly tied to static utilities.
- After: policies are injectable and replaceable.
- SOLID impact:
  - **SRP:** guardrail logic is encapsulated in dedicated services.
  - **DIP:** controller depends on interfaces.
  - **ISP:** focused interfaces expose only relevant methods.

## D) Shared OpenAI adapter (SRP + DIP + OCP)
- Added `IOpenAIClient` / `OpenAIClient`.
- `OpenAIChatService` and `JobMatchService` now share one HTTP/auth adapter.

**Why this matters**
- Before: duplicated HTTP/auth request plumbing in multiple services.
- After: domain services focus on prompts/parsing; transport handled once.
- SOLID impact:
  - **SRP:** domain services no longer own transport concerns.
  - **DIP:** services depend on `IOpenAIClient`, not `HttpClientFactory` details.
  - **OCP:** changing provider transport behavior is centralized.

## E) Frontend SRP split
- Moved location fallback workflow from `ChurchWebsite/wwwroot/js/site.js` to `ChurchWebsite/wwwroot/js/location-fallback.js`.

**Why this matters**
- Before: one file had navigation, scrolling, and location logic mixed together.
- After: concerns are separated by responsibility.
- SOLID impact:
  - **SRP:** each module has a clearer purpose.

## F) Test hardening (supports LSP/DIP confidence)
- Updated tests for new interfaces and new adapter boundaries.
- Added `OpenAIClient` tests.
- Added `ChurchWebsite.Tests` for settings destination behavior.

**Why this matters**
- When implementations are swapped behind interfaces, tests ensure substitutes still behave correctly (**LSP confidence**) and abstraction boundaries stay stable (**DIP confidence**).

---

## 3) One “Should Not Have Been Done” Example (Before) vs SOLID Version (After)

## Example: Controller calling static utilities directly

### Before (tight coupling; lower testability)
```csharp
// ChatController (old style)
var validationError = InputValidator.GetValidationError(request.Message);
if (ContentFilter.IsBlocked(request.Message)) { ... }
```

### Why this is weaker
- Controller is tied to concrete static classes.
- Harder to swap validation/filter policies.
- Harder to mock in tests.

### After (DIP + SRP + ISP)
```csharp
// ChatController (refactored style)
var validationError = _inputValidator.GetValidationError(request.Message);
if (_contentFilter.IsBlocked(request.Message)) { ... }
```

**Improvement**
- Controller depends on contracts (`IInputValidator`, `IContentFilter`).
- Validation/filter logic can evolve independently.
- Easy to mock in unit tests.

## Example 2: Hardcoded destination in multiple places (should not be done)

### Before (duplication + brittle changes)
```csharp
// Location.cshtml (old style)
var googleMapsUrl = "https://www.google.com/maps/dir//New+Bethel+Missionary,...";
var appleMapsUrl = "https://maps.apple.com/?daddr=28.0497068,-81.7260765...";
```

```javascript
// location-route.js (old style)
const destination = {
  name: 'New Bethel Missionary Baptist Church',
  address: '123 Ave Y NE, Winter Haven, FL 33881',
  lat: 28.0497068,
  lon: -81.7260765
};
```

### Why this is weaker
- Same business data lives in multiple files.
- If the address changes, devs must update every hardcoded location.
- Higher risk of inconsistent behavior between UI links and routing logic.

### After (single source of truth from settings)
```csharp
// Location.cshtml (refactored style)
var destinationLabel = string.IsNullOrWhiteSpace(Model.Destination.AddressLabel)
    ? Model.Destination.Name
    : Model.Destination.AddressLabel;
var destinationCoords = $"{Model.Destination.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Model.Destination.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
var googleMapsUrl = $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(destinationCoords)}&travelmode=driving";
```

```javascript
// location-route.js (refactored style)
const destination = {
  name: card.dataset.destinationName || 'Church',
  address: card.dataset.destinationAddress || '',
  lat: parseOptionalNumber(card.dataset.destinationLat),
  lon: parseOptionalNumber(card.dataset.destinationLon)
};
```

### Why this change was made
- Destination now comes from one configuration-driven source (`ChurchSettings.Routing.ChurchDestination`).
- The view and JS consume data passed through the page model/`data-*` contract.
- This improves:
  - **SRP:** config responsibility stays in config/domain model.
  - **OCP:** destination can change via config with no route algorithm rewrite.
  - **DIP:** frontend route logic depends on a contract, not embedded constants.

---

## 4) Interview / Teaching Script You Can Recite

Use this concise version:

1. **S**: One class, one job, one reason to change.  
2. **O**: Add new behavior by extending, not by editing stable code.  
3. **L**: Any implementation of an interface should be safely substitutable.  
4. **I**: Small, purpose-built interfaces; no forced unused methods.  
5. **D**: Depend on abstractions, not concrete classes.

Then give your concrete project example:
- “We changed page models and controllers from concrete classes/static calls to interfaces (`IEventService`, `IInputValidator`, `IOpenAIClient`), which improved testability and made behavior easier to extend without breaking existing modules.”

---

## 5) Practical Memory Trick

- **S** = **Single job**  
- **O** = **Open to add, closed to rewrite**  
- **L** = **Like-for-like replacement**  
- **I** = **Interface fit-for-purpose**  
- **D** = **Depend on contracts**

If you can explain each with one real project example, recruiters will see you understand both theory and engineering practice.

