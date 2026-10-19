# Identity Link Id — DEV dummy sources (run in order)

Verification Hub (vhub) is not in `onboarded_system` in DEV. For internal testing use:

| Role | `source_system_name` |
| --- | --- |
| Caller | `test1` |
| Allowed (LinkId share) | `wadoh.test` |

DEV-only. Real vhub → ProviderOne still needs Verification Hub onboarded later.

Run the steps below **in sequence**.

---

## Step 0 — Known IPs for these sources (DEV)

These ranges do **not** overlap. Use a **different** `ipAddress` per source.

| Source | CIDR | Example `ipAddress` to send |
| --- | --- | --- |
| `test1` | `10.0.0.0/32` | `10.0.0.0` (only this host; `/32` is a single IP) |
| `wadoh.test` | `192.230.0.0/20` | `192.230.0.1` (any host in `192.230.0.0`–`192.230.15.255`) |

Do **not** use text like `YOUR.IP.HERE` in SQL (`inet` cast will fail).

Confirm what is stored:

```sql
SELECT
    source_system_name,
    is_active,
    start_ip_address,
    end_ip_address,
    ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE LOWER(source_system_name) IN ('test1', 'wadoh.test')
ORDER BY source_system_name;
```

If a source has **many** rows (multiple IP ranges), any matching active row is enough. Pick one IP that falls in that source’s CIDR/range for Bruno.

---

## Step 1 — Confirm both sources are onboarded and active

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

Need: 2 rows, `is_active = true`, and IP columns not empty.

If names differ, list all:

```sql
SELECT id, source_system_name, agency_name, is_active
FROM coalitionmpi.onboarded_system
ORDER BY source_system_name;
```

---

## Step 2 — Confirm each source IP is allowed

### 2a — `test1` with `10.0.0.0`

```sql
SELECT
    source_system_name,
    is_active,
    start_ip_address,
    end_ip_address,
    ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE is_active = TRUE
  AND LOWER(source_system_name) = 'test1'
  AND (
        (
          start_ip_address IS NOT NULL
          AND end_ip_address IS NOT NULL
          AND CAST('10.0.0.0' AS inet)
              BETWEEN start_ip_address::inet AND end_ip_address::inet
        )
        OR (
          ip_address_cidr IS NOT NULL
          AND CAST('10.0.0.0' AS inet) <<= ip_address_cidr::inet
        )
      );
```

Need at least one row.

### 2b — `wadoh.test` with `192.230.0.1`

```sql
SELECT
    source_system_name,
    is_active,
    start_ip_address,
    end_ip_address,
    ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE is_active = TRUE
  AND LOWER(source_system_name) = 'wadoh.test'
  AND (
        (
          start_ip_address IS NOT NULL
          AND end_ip_address IS NOT NULL
          AND CAST('192.230.0.1' AS inet)
              BETWEEN start_ip_address::inet AND end_ip_address::inet
        )
        OR (
          ip_address_cidr IS NOT NULL
          AND CAST('192.230.0.1' AS inet) <<= ip_address_cidr::inet
        )
      );
```

Need at least one row.

---

## Step 3 — Check if LinkId mapping already exists

```sql
SELECT
    dsm.mapping_id,
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
  AND LOWER(allowed.source_system_name) = 'wadoh.test'
ORDER BY dsm.mapping_id;
```

If you already see an active `LinkId` row, skip Step 4.

---

## Step 4 — Insert LinkId mapping (idempotent)

Caller `test1` may see link ids from `wadoh.test`.  
Replace `your.user.id` with your id.  
Do not use `LOWER(data_sharing_level)` — that column is a Postgres enum.

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

Expected insert count: `1` if none existed, `0` if already present.  
If something looks wrong before commit, run `ROLLBACK;` instead of `COMMIT;`.

---

## Step 5 — Verify mapping

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

Expected: `wadoh.test` at `LinkId`, `is_active = true`.

---

## Step 6 — Bruno: create identity under allowed source

`POST {{URL}}/api/v1/postIdentity`

```json
{
  "trackingId": "dummy-linkid-post-001",
  "sourceSystem": "wadoh.test",
  "agency": "",
  "ipAddress": "192.230.0.1",
  "protectedPopulation": [
    { "ProtectedPopulationFlag": "N" }
  ],
  "content": {
    "responseIdentityFormatNames": ["DEFAULT"],
    "identity": {
      "sources": [
        {
          "name": "wadoh.test",
          "id": "DUMMYLINKID001"
        }
      ],
      "names": [
        {
          "first": "DummyFirst",
          "last": "DummyLast"
        }
      ],
      "datesOfBirth": ["1990-01-15"],
      "ssns": ["000-11-2233"],
      "genders": ["F"]
    }
  }
}
```

Use `wadoh.test` IP (`192.230.0.1`). Confirm post succeeds.

---

## Step 7 — Bruno: call identityLinkId as caller

`POST {{URL}}/api/v1/identityLinkId`

```json
{
  "trackingId": "dummy-linkid-get-001",
  "sourceSystem": "test1",
  "agency": "",
  "ipAddress": "10.0.0.0",
  "content": {
    "identity": {
      "names": [
        {
          "first": "DummyFirst",
          "last": "DummyLast"
        }
      ],
      "datesOfBirth": ["1990-01-15"],
      "ssns": ["000-11-2233"],
      "genders": ["F"]
    }
  }
}
```

Same demographics as Step 6. Use `test1` IP (`10.0.0.0`) — not the `wadoh.test` IP.

### Expected response

```json
{
  "success": true,
  "message": "Identity found.",
  "content": {
    "linkId": "..."
  }
}
```

If mapping is missing or IP not allowed for `test1`, you may get `"No identity found."` or an IP validation error.

---

## Step 8 — Optional Bruno IP smoke check (before Steps 6–7)

```json
"sourceSystem": "test1",
"ipAddress": "10.0.0.0"
```

Then:

```json
"sourceSystem": "wadoh.test",
"ipAddress": "192.230.0.1"
```

- Allowed → request proceeds past IP filter  
- Blocked → IP / sourceSystem mismatch  

Do not send `10.0.0.0` for `wadoh.test`, or `192.230.0.1` for `test1` — those CIDRs do not overlap.

---

## Notes

- API IP order: body `ipAddress` → `X-Forwarded-For` → remote IP. Prefer setting `ipAddress` in Bruno.
- Caller can always see its own source; that does not need a mapping row.
- Same-source smoke test (no mapping): call as `WAHCA.Providerone` for an identity under that source.
- See also: `design_linkID_db_setup.md`, `design_linkID_bruno_test.md`, `design_linkID_lookup.md`.
