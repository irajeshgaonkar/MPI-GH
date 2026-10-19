# Identity Link Id — database setup

`POST api/v1/identityLinkId` uses active `data_share_mapping` rows at level `LinkId` to decide which source systems the caller may see a link id from.

| Item | Value |
| --- | --- |
| Schema | `coalitionmpi` |
| Caller | `wadshs.verificationhub` |
| Allowed systems (this change) | `WAHCA.Providerone` |
| Not included yet | `Wahbe.hpf`, `wadshs.aces`, ESD, WTB |
| Sharing level | `LinkId` |

Run these in order. Confirm names in step 1 before inserting.

---

## Step 1 — Confirm onboarded systems

Exact short names (`providerone`) do not match what is stored. Prefer a broader search first:

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
WHERE LOWER(source_system_name) LIKE '%provider%'
   OR LOWER(source_system_name) LIKE '%verification%'
   OR LOWER(source_system_name) LIKE '%wadshs%'
ORDER BY source_system_name;
```

Confirmed stored names:

| Role | `source_system_name` | In this mapping? |
| --- | --- | --- |
| Caller | `wadshs.verificationhub` | Caller (no mapping row needed for itself) |
| ProviderOne | `WAHCA.Providerone` | Yes |
| HealthPlanFinder | `Wahbe.hpf` | No (later) |
| ACES | `wadshs.aces` | No (later) |

Exact-name check for this change:

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
WHERE LOWER(source_system_name) IN (
    'wadshs.verificationhub',
    'wahca.providerone'
)
ORDER BY source_system_name;
```

You should see 2 active rows. If a name differs in another environment, use the exact `source_system_name` from that result in later steps. Do not run the insert until those names are confirmed.

If the exact check returns no rows, list everything onboarded:

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
ORDER BY source_system_name;
```

---

## Step 2 — Check existing LinkId mappings

```sql
SELECT
    dsm.mapping_id,
    caller.source_system_name AS calling_source_system,
    allowed.source_system_name AS allowed_system,
    dsm.data_sharing_level,
    dsm.is_active,
    dsm.created_by,
    dsm.created_date
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller
    ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed
    ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
  AND LOWER(dsm.data_sharing_level) = 'linkid'
ORDER BY allowed.source_system_name;
```

If `WAHCA.Providerone` is already active at `LinkId`, skip step 3.

---

## Step 3 — Insert LinkId mapping (idempotent)

Replace `your.user.id` with the person applying the change.

```sql
BEGIN;

INSERT INTO coalitionmpi.data_share_mapping (
    source_system_id,
    allowed_system_id,
    data_sharing_level,
    created_by,
    created_date,
    is_active
)
SELECT
    caller.id,
    allowed.id,
    'LinkId',
    'your.user.id',
    NOW(),
    TRUE
FROM coalitionmpi.onboarded_system caller
JOIN coalitionmpi.onboarded_system allowed
    ON LOWER(allowed.source_system_name) = 'wahca.providerone'
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
  AND COALESCE(caller.is_active, TRUE) = TRUE
  AND COALESCE(allowed.is_active, TRUE) = TRUE
  AND NOT EXISTS (
      SELECT 1
      FROM coalitionmpi.data_share_mapping existing
      WHERE existing.source_system_id = caller.id
        AND existing.allowed_system_id = allowed.id
        AND LOWER(existing.data_sharing_level) = 'linkid'
  );

COMMIT;
```

If anything looks wrong before commit, use `ROLLBACK;` instead of `COMMIT;`.

Expected insert count: `1` if none existed, `0` if already present.

---

## Step 4 — Verify what the endpoint will use

```sql
SELECT allowed.source_system_name AS allowed_link_id_system
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller
    ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed
    ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
  AND LOWER(dsm.data_sharing_level) = 'linkid'
  AND dsm.is_active = TRUE
ORDER BY allowed.source_system_name;
```

Expected:

- `WAHCA.Providerone`

`Wahbe.hpf`, `wadshs.aces`, ESD, and WTB must not appear for this change.

---

## Notes

- The calling system can always see its own source data; that does not need a mapping row.
- After this change, `wadshs.verificationhub` can get a link id when the matched identity includes `wadshs.verificationhub` or `WAHCA.Providerone`.
- Identities that only have `Wahbe.hpf` or `wadshs.aces` will still return no link id until those mappings are added later.
- `data_sharing_level` must be `LinkId` (matches `DataSharingLevel.LinkId`).
- Only `is_active = TRUE` rows are used by the API.
- Apply in the correct environment (dev / int / test / prod) with the right approvals.
