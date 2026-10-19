# Identity Link Id — database setup

`POST api/v1/identityLinkId` uses active `data_share_mapping` rows at level `LinkId` to decide which source systems the caller may see a link id from.

| Item | Value |
| --- | --- |
| Schema | `coalitionmpi` |
| Caller | `wadshs.verificationhub` |
| Allowed systems | ProviderOne, HealthPlanFinder, ACES |
| Not included | ESD, WTB |
| Sharing level | `LinkId` |

Run these in order. Confirm names in step 1 before inserting.

---

## Step 1 — Confirm onboarded systems

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
WHERE LOWER(source_system_name) IN (
    'wadshs.verificationhub',
    'providerone',
    'healthplanfinder',
    'aces'
)
ORDER BY source_system_name;
```

You should see 4 rows. If a name differs, use the exact `source_system_name` from this result in later steps.

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

If ProviderOne, HealthPlanFinder, and ACES are already active, skip step 3.

---

## Step 3 — Insert LinkId mappings (idempotent)

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
    ON LOWER(allowed.source_system_name) IN (
        'providerone',
        'healthplanfinder',
        'aces'
    )
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

Expected insert count: `3` if none existed, `0` if already present.

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

- ProviderOne (or your exact stored name)
- HealthPlanFinder
- ACES

ESD and WTB must not appear.

---

## Notes

- The calling system can always see its own source data; that does not need a mapping row.
- `data_sharing_level` must be `LinkId` (matches `DataSharingLevel.LinkId`).
- Only `is_active = TRUE` rows are used by the API.
- Apply in the correct environment (dev / int / test / prod) with the right approvals.
