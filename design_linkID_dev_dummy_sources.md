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
If this fails with `does not exist` or `transaction is aborted`, go to Step 4a / 4b first, then re-run this query with the correct `schema.table`.

---

## Step 4 — Insert LinkId mapping (idempotent)

Caller `test1` may see link ids from `wadoh.test`.  
Replace `your.user.id` with your id.  
Do not use `LOWER(data_sharing_level)` — that column is a Postgres enum.

### 4a — If a previous statement failed (aborted transaction)

You will see: `current transaction is aborted, commands ignored until end of transaction block`.

```sql
ROLLBACK;
```

Then continue. Do not run more queries until you `ROLLBACK` (or reconnect).

### 4b — Confirm table schema / name

If you see `relation "coalitionmpi.data_share_mapping" does not exist`:

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_name ILIKE '%share%mapping%'
   OR table_name ILIKE '%data_share%'
ORDER BY table_schema, table_name;
```

```sql
SELECT nspname
FROM pg_namespace
WHERE nspname ILIKE '%mpi%' OR nspname ILIKE '%coalition%'
ORDER BY 1;
```

Use the exact `table_schema.table_name` returned (examples: `coalitionmpi.data_share_mapping` or `public.data_share_mapping`).  
In app code the table name is `data_share_mapping`.

### 4c — Confirm exact enum label (case-sensitive)

Invalid values like `'Linkid'` or `'linkid'` fail if the label is different (for example `'LinkId'`).

```sql
SELECT t.typname, e.enumlabel
FROM pg_type t
JOIN pg_enum e ON e.enumtypid = t.oid
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname IN ('coalitionmpi', 'public')
  AND (
        t.typname ILIKE '%sharing%'
     OR t.typname ILIKE '%share%level%'
     OR t.typname = 'data_sharing_level'
  )
ORDER BY t.typname, e.enumsortorder;
```

If the table exists and has rows:

```sql
SELECT DISTINCT data_sharing_level::text AS level_label
FROM coalitionmpi.data_share_mapping
ORDER BY 1;
```

Copy the label **exactly** into the insert below.  
App code matches with `ToLower() == "linkid"`, so the DB label must be some form of linkid (`linkid` / `LinkId`).

**Important:** `DISTINCT` on the table only shows levels already used in rows. If you only see `existence`, that does **not** prove `linkid` is missing — run Step 4e (`pg_enum`). If `linkid` is absent, ask DB team (Step 4d), then insert (Step 4f).

### 4d — Add `linkid` enum value if missing (DB team)

App logins usually **cannot** alter the enum (`must be owner of type ... data_sharing_level`).  
Open a ticket / email the DB team to add `linkid` on DEV. Type name: `coalitionmpi.data_sharing_level`.

Optional — find type owner (read-only):

```sql
SELECT n.nspname, t.typname, pg_get_userbyid(t.typowner) AS type_owner
FROM pg_type t
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname = 'coalitionmpi'
  AND t.typname = 'data_sharing_level';
```

If you *are* the owner (or a DBA), run outside a failed transaction (`ROLLBACK;` first if needed).  
DEV labels are lowercase (`existence`, …) → add `linkid`:

```sql
ALTER TYPE coalitionmpi.data_sharing_level ADD VALUE IF NOT EXISTS 'linkid';
```

### 4e — Confirm `linkid` exists (after DB team)

```sql
SELECT e.enumlabel
FROM pg_type t
JOIN pg_enum e ON e.enumtypid = t.oid
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname = 'coalitionmpi'
  AND t.typname = 'data_sharing_level'
ORDER BY e.enumsortorder;
```

Need `linkid` in the list before inserting.  
Expected labels include: `existence`, `exceptprotectedpopulation`, `full`, `linkid`.

### 4f — Insert mapping

Replace `your.user.id` with your id.

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
    'linkid',
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
If the insert fails, run `ROLLBACK;` then fix the enum label / table name and retry.

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
  AND LOWER(allowed.source_system_name) = 'wadoh.test';
```

Expected: `wadoh.test` at `linkid`, `is_active = true`.

---

## Step 6 — Bruno: three request bodies (same unique person)

Use the **same** unique demographics in all three calls below.  
Do **not** only rename `sourceSystem` on an old Bruno script that already matches another source — Verato will return that existing person and you will get `"No identity found."` for `wadoh.test` / `test1`.

Also for postIdentity: top-level `sourceSystem` and `sources[0].name` must both be `wadoh.test`.

### 6a — postIdentity (`wadoh.test`)

`POST {{URL}}/api/v1/postIdentity`

```json
{
  "trackingId": "dummy-linkid-post-uniq-001",
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
          "id": "WADOH-LINKID-BANDIR-001"
        }
      ],
      "names": [
        {
          "first": "BandirLink",
          "last": "Testuniq001"
        }
      ],
      "datesOfBirth": ["1988-03-22"],
      "ssns": ["001-88-4321"],
      "genders": ["M"]
    }
  }
}
```

Confirm response includes `wadoh.test` under `identityGroupedBySource` (or equivalent). Note the `linkId`.

### 6b — demographicsQuery (`wadoh.test`)

`POST {{URL}}/api/v1/demographicsQuery`

Demographic query **filters** to sources matching the caller. If the identity only has another source, you get `"No identity found."` even though the person exists.

```json
{
  "trackingId": "dummy-linkid-demo-uniq-001",
  "sourceSystem": "wadoh.test",
  "agency": "",
  "ipAddress": "192.230.0.1",
  "content": {
    "responseIdentityFormatNames": ["GROUP_BY_SOURCE"],
    "identity": {
      "names": [
        {
          "first": "BandirLink",
          "last": "Testuniq001"
        }
      ],
      "datesOfBirth": ["1988-03-22"],
      "ssns": ["001-88-4321"],
      "genders": ["M"]
    }
  }
}
```

Expect a hit with `wadoh.test` in the grouped sources before Step 7.

### 6c — identityLinkId as caller (`test1`)

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
          "first": "BandirLink",
          "last": "Testuniq001"
        }
      ],
      "datesOfBirth": ["1988-03-22"],
      "ssns": ["001-88-4321"],
      "genders": ["M"]
    }
  }
}
```

Use `test1` IP (`10.0.0.0`) — not the `wadoh.test` IP.  
`responseIdentityFormatNames` is not required; the API uses `GROUP_BY_SOURCE` internally.

### Expected identityLinkId response

```json
{
  "success": true,
  "message": "Identity found.",
  "content": {
    "linkId": "..."
  }
}
```

`linkId` should match Step 6a.

### Split test if 6c fails

Call `identityLinkId` as `wadoh.test` / IP `192.230.0.1` with the same demographics as 6c (only change `sourceSystem` and `ipAddress`).

| Call as | IP | Meaning if found / not found |
| --- | --- | --- |
| `wadoh.test` | `192.230.0.1` | Found → person OK; 6c issue is mapping / allowed list. Not found → demographics or `wadoh.test` not on the identity (see 6a / 6b). |
| `test1` | `10.0.0.0` | Found → mapping works. Not found while `wadoh.test` works → check Step 5 mapping / API LinkId share load. |

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

## Troubleshooting

| Error | What to do |
| --- | --- |
| `current transaction is aborted...` | Run `ROLLBACK;` then retry. |
| `relation "...data_share_mapping" does not exist` | Run Step 4b; use the real `schema.table` name. Confirm you are on the DEV MPI database. |
| `invalid input value for enum ... "Linkid"` / `"linkid"` | Enum labels are case-sensitive. Run Step 4c / 4e. If missing, ask DB team (Step 4d), then insert (Step 4f). |
| `must be owner of type ... data_sharing_level` | Ticket / email DB team (Step 4d). Do not expect app logins to alter enums. |
| `invalid input syntax for type inet` | Do not use placeholders like `YOUR.IP.HERE`. Use a real IP such as `10.0.0.0`. |
| `function lower(data_sharing_level) does not exist` or `operator does not exist: data_sharing_level = text` | Do not filter the enum with `LOWER()` or compare to text in SQL. App code filters level in memory (API branch). |
| `"Source system mismatch."` on postIdentity | Top-level `sourceSystem` must match `content.identity.sources[0].name`. |
| `"No identity found."` on demographic query / identityLinkId | Usually: (1) reused demographics so identity only has another source; (2) demographic query filtered to caller source; (3) missing LinkId mapping for `test1` → `wadoh.test`; (4) wrong IP. Re-run Step 6a → 6b → 6c with unique demographics. |

---

## Notes

- API IP order: body `ipAddress` → `X-Forwarded-For` → remote IP. Prefer setting `ipAddress` in Bruno.
- Caller can always see its own source; that does not need a mapping row.
- Same demographics find the **person**; `identityLinkId` / demographic query still require a **source visible to the caller** (own source or LinkId share).
- Same-source smoke test (no mapping): call as `WAHCA.Providerone` for an identity under that source.
- See also: `design_linkID_db_setup.md`, `design_linkID_bruno_test.md`, `design_linkID_lookup.md`.
