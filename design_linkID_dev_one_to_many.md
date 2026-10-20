# Identity Link Id — DEV one-to-many (add ProviderOne)

`test1` already has a `linkid` mapping to `wadoh.test` (see `design_linkID_dev_dummy_sources.md`).  
This adds a second allowed system so one caller can see link ids from two sources.

| Role | `source_system_name` |
| --- | --- |
| Caller | `test1` |
| Already allowed | `wadoh.test` |
| Add now | `WAHCA.Providerone` |

Do not remove the `wadoh.test` row.

DEV-only. Run the steps below **in sequence**.

---

## Step 0 — IPs

| Source | CIDR | Example `ipAddress` |
| --- | --- | --- |
| `test1` | `10.0.0.0/32` | `10.0.0.0` |
| `WAHCA.Providerone` | `147.55.192.3/32` | `147.55.192.3` |

`/32` is a single host. Do not use a placeholder in SQL (`inet` cast will fail).

---

## Step 1 — Confirm ProviderOne is onboarded

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
WHERE LOWER(source_system_name) = 'wahca.providerone';
```

Need one active row. Use the exact `source_system_name` from this result if it differs.

---

## Step 2 — See mappings `test1` already has

```sql
SELECT
    caller.source_system_name AS caller,
    allowed.source_system_name AS allowed,
    dsm.data_sharing_level::text,
    dsm.is_active
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'test1'
ORDER BY allowed.source_system_name;
```

Expect the existing `wadoh.test` / `linkid` row. `WAHCA.Providerone` should not be there yet.

---

## Step 3 — Insert `test1` → `WAHCA.Providerone` at `linkid`

Enum label in DEV is lowercase `linkid`. Do not use `LOWER(data_sharing_level)` in SQL.

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
    'bandir107',
    NOW(),
    TRUE
FROM coalitionmpi.onboarded_system caller
JOIN coalitionmpi.onboarded_system allowed
    ON LOWER(allowed.source_system_name) = 'wahca.providerone'
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

Expected insert count: `1`. If it fails, run `ROLLBACK;`.

---

## Step 4 — Verify both allowed systems

```sql
SELECT
    caller.source_system_name AS caller,
    allowed.source_system_name AS allowed,
    dsm.data_sharing_level::text,
    dsm.is_active
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'test1'
  AND dsm.data_sharing_level::text ILIKE 'linkid'
ORDER BY allowed.source_system_name;
```

Expect two active rows: `wadoh.test` and `WAHCA.Providerone`.

---

## Step 5 — Bruno: post a person under ProviderOne

`POST {{URL}}/api/v1/postIdentity`

Use a new source id and unique demographics. Both `sourceSystem` and `sources[0].name` must be `WAHCA.Providerone`.

```json
{
  "trackingId": "dummy-linkid-p1-post-001",
  "sourceSystem": "WAHCA.Providerone",
  "agency": "",
  "ipAddress": "147.55.192.3",
  "protectedPopulation": [
    { "ProtectedPopulationFlag": "N" }
  ],
  "content": {
    "responseIdentityFormatNames": ["DEFAULT"],
    "identity": {
      "sources": [
        {
          "name": "WAHCA.Providerone",
          "id": "P1-LINKID-BANDIR-001"
        }
      ],
      "names": [
        {
          "first": "PoneLink",
          "last": "Qtest88001"
        }
      ],
      "datesOfBirth": ["1982-06-14"],
      "ssns": ["002-77-6611"],
      "genders": ["F"]
    }
  }
}
```

If the local row is inserted with `is_active = false` and `is_delete = false`, that is the known first-insert sync quirk. UI only:

```sql
UPDATE coalitionmpi.client_identity
SET is_active = TRUE,
    updated_date = NOW()
WHERE source_system_id = 'P1-LINKID-BANDIR-001'
  AND LOWER(source_system_name) = 'wahca.providerone';
```

Posting the **same** source id a second time also sets `is_active = true`. A new source id starts over as `false`.

---

## Step 6 — Bruno: demographicsQuery as ProviderOne

`POST {{URL}}/api/v1/demographicsQuery`

Turn off any before-request script that generates a new person. Same demographics as Step 5.

```json
{
  "trackingId": "dummy-linkid-p1-demo-001",
  "sourceSystem": "WAHCA.Providerone",
  "agency": "",
  "ipAddress": "147.55.192.3",
  "content": {
    "responseIdentityFormatNames": ["GROUP_BY_SOURCE"],
    "identity": {
      "sources": [
        {
          "name": "WAHCA.Providerone",
          "id": "P1-LINKID-BANDIR-001"
        }
      ],
      "names": [
        {
          "first": "PoneLink",
          "last": "Qtest88001"
        }
      ],
      "datesOfBirth": ["1982-06-14"],
      "ssns": ["002-77-6611"],
      "genders": ["F"]
    }
  }
}
```

Need a hit before Step 7. Note the `linkId`.

---

## Step 7 — Bruno: identityLinkId as `test1`

`POST {{URL}}/api/v1/identityLinkId`

```json
{
  "trackingId": "dummy-linkid-p1-get-001",
  "sourceSystem": "test1",
  "agency": "",
  "ipAddress": "10.0.0.0",
  "content": {
    "identity": {
      "names": [
        {
          "first": "PoneLink",
          "last": "Qtest88001"
        }
      ],
      "datesOfBirth": ["1982-06-14"],
      "ssns": ["002-77-6611"],
      "genders": ["F"]
    }
  }
}
```

### Expected

```json
{
  "success": true,
  "message": "Identity found.",
  "content": {
    "linkId": "..."
  }
}
```

`linkId` should match Step 6.

One-to-many check: call `identityLinkId` again as `test1` with the earlier `wadoh.test` demographics. That should still return the `wadoh.test` link id. `test1` can see both allowed systems.

---

## Troubleshooting

| Error | What to do |
| --- | --- |
| `current transaction is aborted...` | Run `ROLLBACK;` then retry. |
| `invalid input value for enum ...` | Use exact label `linkid`. |
| `"Source system mismatch."` | Top-level `sourceSystem` must match `sources[0].name`. |
| `"No identity found."` as ProviderOne | Demographics or source id do not match the post. Do not continue to Step 7. |
| `"No identity found."` as `test1` while Step 6 worked | Mapping row missing or not active. Re-run Step 4. |
| IP / sourceSystem mismatch | `147.55.192.3` only for `WAHCA.Providerone`. `10.0.0.0` only for `test1`. |
