# Investigation Report: ESI Configuration "Edit" & "New ESI Configuration" Redirect to Login

## Summary Answer

**Root cause: The Angular route `master/esi-slab/new-esi-slab` (used by the "New ESI Configuration" button) and the route `master/esi-slab/edit-esi-slab/:id` (used by the "Edit" button) are both MISSING from the routing module.** There is also no `NewEsiSlabComponent` — neither the component class files nor a `new-esi-slab` subdirectory exist. When Angular cannot match a route, it falls through to the wildcard `{ path: '**', redirectTo: 'login' }` in `admin-layout-routing.module.ts`, which redirects the user to `/login`. The login page then detects a stale token in localStorage and displays _"Your session is expired! Please login to continue..."_ — making it appear to be a session/auth problem when it is actually a missing route + missing component problem.

---

## Evidence

### 1. The list component navigates to routes that don't exist

**File:** `src/app/modules/master/esi-slab/esi-slab.component.html`

The "New ESI Configuration" button uses:
```html
[routerLink]="['/master/esi-slab/new-esi-slab']"
```

The Edit icon in the table uses:
```html
[routerLink]="['/master/esi-slab/edit-esi-slab', row.Id]"
```

The TypeScript `onEditClick()` method in `esi-slab.component.ts` (line ~137) also navigates programmatically to:
```ts
this._router.navigate(['/master/esi-slab/new-esi-slab'], { queryParams: { id: data.id } });
```

### 2. Neither route is defined in the routing module

**File:** `src/app/layout/admin-layout/admin-layout-routing.module.ts`

The Indian Statutory Compliance section (lines ~390–407) only has:
```ts
{ path: 'master/esi-slab', component: EsiSlabComponent, canActivate: [AuthGuard] },
```

The routes `master/esi-slab/new-esi-slab` and `master/esi-slab/edit-esi-slab/:id` are completely absent.

For comparison, the PF Slab (the sister component) correctly has both routes:
```ts
{ path: 'master/pf-slab',          component: PfSlabComponent,    canActivate: [AuthGuard] },
{ path: 'master/pf-slab/new-pf-slab', component: NewPfSlabComponent, canActivate: [AuthGuard] },
```

### 3. The NewEsiSlabComponent does not exist

Running a recursive directory listing of `src/app/modules/master/esi-slab/` confirms:

```
esi-slab.component.css
esi-slab.component.html
esi-slab.component.ts
```

No `new-esi-slab/` subdirectory exists at all. Contrast with `pf-slab`, which has a complete `new-pf-slab/` subdirectory containing:
```
new-pf-slab.component.css
new-pf-slab.component.html
new-pf-slab.component.ts
```

### 4. The wildcard route causes the login redirect

**File:** `src/app/layout/admin-layout/admin-layout-routing.module.ts` (last route):
```ts
{ path: '**', redirectTo: 'login' }
```

**File:** `src/app/app-routing.module.ts`:
```ts
{ path: '**', redirectTo: 'login' }
```

Both the child and root routing modules have wildcard fallbacks to `login`. When Angular cannot match `master/esi-slab/new-esi-slab`, it hits this wildcard.

### 5. The login page shows "session expired" even for a route-not-found redirect

**File:** `src/app/modules/user/login/login.component.ts` (`ngOnInit`, lines ~47–52):
```ts
ngOnInit(): void {
  const manualLogout = localStorage.getItem('manualLogout') === 'true';
  this.token = this.authService.getToken() || '';
  if (this.token && !manualLogout) {
    this.authService.clearToken();
    this.warningMessage = 'Your session is expired! Please login to continue...';
  }
}
```

When the user is still logged in (token present in localStorage) and gets redirected to `/login` by the wildcard, the login page fires this check. It sees a token (`this.token` is truthy) and `manualLogout` is false, so it clears the token and shows the misleading "session expired" message. The user is actually logged out as a side effect of hitting the missing route.

### 6. The AuthGuard itself is NOT the problem

**File:** `src/app/service/auth.guard.ts`

The guard is simple:
```ts
canActivate(): boolean {
  if (this.authService.isAuthenticated()) {
    return true;
  } else {
    this.router.navigate(['/login']);
    return false;
  }
}
```

It only checks for a token in localStorage. The route `master/esi-slab/new-esi-slab` never reaches the guard because the route doesn't exist in the routing table — Angular matches the wildcard first.

### 7. The shared module does NOT declare NewEsiSlabComponent

**File:** `src/app/modules/shared.module.ts`

Imports and declarations found:
```ts
import { PfSlabComponent }    from './master/pf-slab/pf-slab.component';
import { NewPfSlabComponent } from './master/pf-slab/new-pf-slab/new-pf-slab.component';
import { EsiSlabComponent }   from './master/esi-slab/esi-slab.component';
// NewEsiSlabComponent — NOT IMPORTED, NOT DECLARED
```

`EsiSlabComponent` appears twice in the declarations array (a minor duplicate declaration bug), confirming this module was partially set up for ESI but the new/edit form component was never added.

### 8. Screen list entry exists but there is no new/edit screen entry

**File:** `OBMSINDIA-API/Scripts/17_Populate_ScreenList.sql`

`'ESI Slab'` exists as a screen entry in the `Master` category:
```sql
IF NOT EXISTS (SELECT 1 FROM ScreenList WHERE ScreenName = 'ESI Slab' AND Category = 'Master')
    INSERT INTO ScreenList (ScreenName, Category, LastUpdate)
    VALUES ('ESI Slab', 'Master', GETDATE())
```

The ESI Slab list page uses screen name `'ESI Slab'` for user-access-rights checking (`getUserAccessRights(userName, 'ESI Slab')`). This part is correct. The new/edit form would reuse the same screen name (as `new-pf-slab` uses `'PF Slab'`), so no new DB screen entry is needed for the fix.

---

## Conclusions

| # | Finding | Impact |
|---|---------|--------|
| 1 | `NewEsiSlabComponent` files do not exist (no `new-esi-slab/` folder) | **Critical** — component must be created |
| 2 | Routes `master/esi-slab/new-esi-slab` and `master/esi-slab/edit-esi-slab/:id` missing from routing module | **Critical** — causes wildcard → login redirect |
| 3 | `NewEsiSlabComponent` not imported/declared in `shared.module.ts` | **Critical** — Angular won't know the component even after it's created |
| 4 | `EsiSlabComponent` declared twice in `shared.module.ts` | Minor — harmless but should be cleaned up |
| 5 | Login `ngOnInit` clears the token on any redirect to `/login` while the user is logged in | Side effect — makes route-not-found look like session expiry |

---

## Recommended Fix

The fix has three parts — all purely frontend (Angular UI). No backend API changes are needed; the API endpoints `GET/POST/PUT api/statutory/esi-configuration` already exist and work.

### Part 1 — Create the NewEsiSlabComponent

Create the folder and files:
```
src/app/modules/master/esi-slab/new-esi-slab/
  new-esi-slab.component.ts
  new-esi-slab.component.html
  new-esi-slab.component.css
```

Model it exactly on `new-pf-slab` (`src/app/modules/master/pf-slab/new-pf-slab/new-pf-slab.component.ts`):
- Use `IndianStatutoryService.getESIConfiguration()` and `saveESIConfiguration()` / `updateESIConfiguration()`
- Read `?id=` query param for edit mode (same pattern as `new-pf-slab`)
- Call `getUserAccessRights(user, 'ESI Slab')` for access control
- Navigate back to `/master/esi-slab` on save/cancel

The `ESIConfiguration` model already exists in `src/app/model/indian-compliance.model.ts`.  
The service methods already exist in `src/app/service/indian-statutory.service.ts` (`getESIConfiguration`, `saveESIConfiguration`, `updateESIConfiguration`).

### Part 2 — Add routes to the routing module

**File:** `src/app/layout/admin-layout/admin-layout-routing.module.ts`

Add the import at the top:
```ts
import { NewEsiSlabComponent } from '../../modules/master/esi-slab/new-esi-slab/new-esi-slab.component';
```

Add routes in the Indian Statutory Compliance section (after the existing `master/esi-slab` route):
```ts
{ path: 'master/esi-slab/new-esi-slab',          component: NewEsiSlabComponent, canActivate: [AuthGuard] },
{ path: 'master/esi-slab/edit-esi-slab/:id',      component: NewEsiSlabComponent, canActivate: [AuthGuard] },
```

Note: The HTML Edit button uses `[routerLink]="['/master/esi-slab/edit-esi-slab', row.Id]"` (route parameter, not query param), while `onEditClick()` in the TS uses a query param. Standardize both to use the route-parameter form (`edit-esi-slab/:id`) and read `this._activatedRoute.snapshot.params['id']` in the new component.

### Part 3 — Declare NewEsiSlabComponent in shared.module.ts

**File:** `src/app/modules/shared.module.ts`

Add import:
```ts
import { NewEsiSlabComponent } from './master/esi-slab/new-esi-slab/new-esi-slab.component';
```

Add to `declarations` and `exports` arrays:
```ts
NewEsiSlabComponent,
```

Also remove the duplicate `EsiSlabComponent` declaration.

### No database/API changes required

The ESI Configuration API endpoints (`GET`, `POST`, `PUT`, `DELETE` at `api/statutory/esi-configuration`) are implemented and functional in `IndianStatutoryController.cs`. The `'ESI Slab'` screen is present in `ScreenList` with the correct category. No SQL scripts need to be run.
