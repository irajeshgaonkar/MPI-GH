# Identity Link Id — DEV dummy sources

Verification Hub (vhub) is not in `onboarded_system` in DEV. For internal testing, use two existing dummy sources:

| Role | Example `source_system_name` |
| --- | --- |
| Caller | `test1` |
| Allowed (LinkId share) | `wadoh.test` |

You can reverse caller/allowed if needed. This is DEV-only. Real vhub → ProviderOne still needs Verification Hub onboarded later.

---

## 1. Confirm both sources are onboarded and active

```sql
SELECT
    id,
    source_system_name,
    agency_name,
    is_active,
    start_ip_address,
    end_ip_address,
    ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE LOWER(source_system_name) IN ('test1', 'wadoh.test')
ORDER BY source_system_name;
```

Need:

- both rows present
- `is_active = true`
- IP range and/or CIDR filled in (see next section)

List everything if names differ:

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
ORDER BY source_system_name;
```

---

## 2. Check if your IP is allowed (auth)

External `/api/v1` calls use IP whitelist on `onboarded_system`. Replace `YOUR.IP.HERE` with the IP you send in the body (`ipAddress`) or your VPN/client IP.

```sql
SELECT
    source_system_name,
    is_active,
    start_ip_address,
    end_ip_address,
    ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE is_active = TRUE
  AND (
        (
          start_ip_address IS NOT NULL
          AND end_ip_address IS NOT NULL
          AND CAST('YOUR.IP.HERE' AS inet)
              BETWEEN start_ip_address::inet AND end_ip_address::inet
        )
        OR (
          ip_address_cidr IS NOT NULL
          AND CAST('YOUR.IP.HERE' AS inet) <<= ip_address_cidr::inet
        )
      )
ORDER BY source_system_name;
```

If `test1` and/or `wadoh.test` appear, that IP is allowed for those sources.

### Bruno quick check

Call any `/api/v1/...` endpoint with:

```json
"sourceSystem": "test1",
"ipAddress": "YOUR.IP.HERE"
```

- Allowed → request proceeds  
- Blocked → IP / sourceSystem mismatch  

Repeat with `"sourceSystem": "wadoh.test"`.

If IP columns are empty for those rows, whitelist must be set before you can call as that source.

---

## 3. Add LinkId mapping (DEV dummy)

Caller `test1` may see link ids from `wadoh.test`.

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
    ON LOWER(allowed.source_system_name) = 'wadoh.test'
WHERE LOWER(caller.source_system_name) = 'test1'
  AND COALESCE(caller.is_active, TRUE) = TRUE
  AND COALESCE(allowed.is_active, TRUE) = TRUE
  AND NOT EXISTS (
      SELECT 1
      FROM coalitionmpi.data_share_mapping existing
      WHERE existing.source_system_id = caller.id
        AND existing.allowed_system_id = allowed.id
        AND existing.data_sharing_level::text ILIKE 'linkid'
  );

COMMIT;
```

If `data_sharing_level` is a Postgres enum, do not use `LOWER(data_sharing_level)` in filters. Prefer `::text` / exact enum label as above.

Verify:

```sql
SELECT
    caller.source_system_name AS calling_source_system,
    allowed.source_system_name AS allowed_system,
    dsm.data_sharing_level,
    dsm.is_active
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller
    ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed
    ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'test1'
  AND dsm.is_active = TRUE
ORDER BY allowed_system;
```

Expected: `wadoh.test` at `LinkId`.

---

## 4. Test the share path

1. `POST /api/v1/postIdentity` with source `wadoh.test` and known demographics (IP allowed for `wadoh.test`).
2. `POST /api/v1/identityLinkId` as `test1` with the **same** demographics (IP allowed for `test1`).

Expected when mapping is active and Verato matches:

- `success`: true  
- `message`: Identity found.  
- `content.linkId`: non-null  

Without the mapping, `test1` only gets a link id if the matched identity already includes `test1` as a source.

---

## Notes

- Caller can always see its own source; that does not need a mapping row.
- Same-source smoke test (no DB mapping): call `identityLinkId` as `WAHCA.Providerone` (or another onboarded source) for an identity under that same source — already validated in DEV.
- When Verification Hub is onboarded, replace these dummies with the real caller name and `WAHCA.Providerone` for the production LinkId mapping.
- See also: `design_linkID_db_setup.md`, `design_linkID_bruno_test.md`, `design_linkID_lookup.md`.
