# Identity Link Id — INT test

`POST /api/v1/identityLinkId` on INT.

Caller for the share tests is `wadshs.verificationhub`. The other two systems are where the other people are posted. A caller always sees its own source. It sees another source only when an active `LinkId` row points from the caller to that source.

| Source system | CIDR | `ipAddress` in the payload |
| --- | --- | --- |
| `wadshs.verificationhub` | `192.168.254.38/32` | `192.168.254.38` |
| `wadshs.aces` | `198.239.193.47/32` | `198.239.193.47` |
| `Wahbe.hpf` | `20.187.52.46/32` | `20.187.52.46` |

| Person | Posted on | What this proves |
| --- | --- | --- |
| Reddy Bharath Bandi | `wadshs.verificationhub` | Own source. No share row needed. |
| Thakrim Shaik | `wadshs.aces` | Hidden until the ACES share is added, then returned. |
| Chakradhar Bandi | `Wahbe.hpf` | Stays hidden while HPF has no share. Returned after that share is added. |

Base URL: `https://hhscoalitionmpi-api-int.hca.wa.gov`

Use the same client certificate as the other `api/v1` calls. Put `ipAddress` in every body. The API uses that IP to pick the caller and overwrites `sourceSystem` with the whitelisted system for that IP.

Do these steps in order. Do not insert the share rows before the “before mapping” calls.

---

## 1. Confirm the three systems and IPs

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
WHERE LOWER(source_system_name) IN (
    'wadshs.verificationhub',
    'wadshs.aces',
    'wahbe.hpf'
)
ORDER BY source_system_name;
```

Need three active rows. Each `/32` must match the table above. If a stored name differs, use that exact name in the later steps.

IP check:

```sql
SELECT source_system_name, ip_address_cidr
FROM coalitionmpi.onboarded_system
WHERE (
    ip_address_cidr IS NOT NULL
    AND CAST('192.168.254.38' AS inet) <<= ip_address_cidr::inet
)
OR (
    ip_address_cidr IS NOT NULL
    AND CAST('198.239.193.47' AS inet) <<= ip_address_cidr::inet
)
OR (
    ip_address_cidr IS NOT NULL
    AND CAST('20.187.52.46' AS inet) <<= ip_address_cidr::inet
);
```

Each IP should hit only its own system.

---

## 2. Add the `linkid` enum value

Same sequence as DEV. App logins usually cannot alter the type.

### 2a. See who owns the enum

```sql
SELECT n.nspname, t.typname, pg_get_userbyid(t.typowner) AS type_owner
FROM pg_type t
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname = 'coalitionmpi'
  AND t.typname = 'data_sharing_level';
```

### 2b. See the labels that already exist

```sql
SELECT e.enumlabel
FROM pg_type t
JOIN pg_enum e ON e.enumtypid = t.oid
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname = 'coalitionmpi'
  AND t.typname = 'data_sharing_level'
ORDER BY e.enumsortorder;
```

If `linkid` is already in that list, skip 2c.

### 2c. Add `linkid`

Only the type owner from 2a, or a DBA, can run this. If a previous statement failed, run `ROLLBACK;` first.

```sql
ALTER TYPE coalitionmpi.data_sharing_level ADD VALUE IF NOT EXISTS 'linkid';
```

If you are not the owner, Postgres returns `must be owner of type data_sharing_level`. Send the owner from 2a to the database team and ask them to run this.

### 2d. Confirm it was added

```sql
SELECT e.enumlabel
FROM pg_type t
JOIN pg_enum e ON e.enumtypid = t.oid
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname = 'coalitionmpi'
  AND t.typname = 'data_sharing_level'
ORDER BY e.enumsortorder;
```

`linkid` must be in that list before the share-row inserts. The inserts below use `'linkid'`. If the stored label is different, use that exact text instead.

See what Verification Hub can already see:

```sql
SELECT
    caller.source_system_name AS caller,
    allowed.source_system_name AS allowed,
    dsm.data_sharing_level::text AS level,
    dsm.is_active
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
ORDER BY allowed.source_system_name, level;
```

`wadshs.aces` and `Wahbe.hpf` should not have an active link-id row yet. If they do, skip the matching insert later.

---

## 3. Post the three people

`POST https://hhscoalitionmpi-api-int.hca.wa.gov/api/v1/postIdentity`

Headers: `Accept: application/json`, `Content-Type: application/json`.

Each post must return success before you look up a link id. `sourceSystem`, `sources[0].name`, and `ipAddress` must belong to the same system.

### 3a. Reddy Bharath Bandi on Verification Hub

```json
{
  "trackingId": "int-linkid-post-reddy",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "DSHS",
  "ipAddress": "192.168.254.38",
  "protectedPopulation": [
    { "ProtectedPopulationFlag": "N" }
  ],
  "content": {
    "identity": {
      "sources": [
        {
          "name": "wadshs.verificationhub",
          "id": "VH-2026-RBANDI-1842"
        }
      ],
      "names": [
        {
          "first": "Reddy",
          "middle": "Bharath",
          "last": "Bandi"
        }
      ],
      "datesOfBirth": ["1991-04-16"],
      "ssns": ["534-28-6194"],
      "genders": ["M"],
      "emails": ["reddy.bharath.bandi@example.com"],
      "phoneNumbers": [
        {
          "number": "5550142",
          "areaCode": "360",
          "countryCode": "1"
        }
      ],
      "addresses": [
        {
          "line1": "418 Legion Way SE",
          "city": "Olympia",
          "state": "WA",
          "postalCode": "98501"
        }
      ]
    }
  }
}
```

### 3b. Thakrim Shaik on ACES

```json
{
  "trackingId": "int-linkid-post-thakrim",
  "sourceSystem": "wadshs.aces",
  "agency": "DSHS",
  "ipAddress": "198.239.193.47",
  "protectedPopulation": [
    { "ProtectedPopulationFlag": "N" }
  ],
  "content": {
    "identity": {
      "sources": [
        {
          "name": "wadshs.aces",
          "id": "ACES-2026-TSHAIX-5521"
        }
      ],
      "names": [
        {
          "first": "Thakrim",
          "last": "Shaik"
        }
      ],
      "datesOfBirth": ["1987-11-23"],
      "ssns": ["417-63-2085"],
      "genders": ["F"],
      "emails": ["thakrim.shaik@example.com"],
      "phoneNumbers": [
        {
          "number": "5550198",
          "areaCode": "253",
          "countryCode": "1"
        }
      ],
      "addresses": [
        {
          "line1": "732 St Helens Ave",
          "city": "Tacoma",
          "state": "WA",
          "postalCode": "98402"
        }
      ]
    }
  }
}
```

### 3c. Chakradhar Bandi on HealthPlanFinder

```json
{
  "trackingId": "int-linkid-post-chakradhar",
  "sourceSystem": "Wahbe.hpf",
  "agency": "Washington Health Benefit Exchange",
  "ipAddress": "20.187.52.46",
  "protectedPopulation": [
    { "ProtectedPopulationFlag": "N" }
  ],
  "content": {
    "identity": {
      "sources": [
        {
          "name": "Wahbe.hpf",
          "id": "HPF-2026-CBANDI-7730"
        }
      ],
      "names": [
        {
          "first": "Chakradhar",
          "last": "Bandi"
        }
      ],
      "datesOfBirth": ["1994-07-09"],
      "ssns": ["361-90-7742"],
      "genders": ["M"],
      "emails": ["chakradhar.bandi@example.com"],
      "phoneNumbers": [
        {
          "number": "5550164",
          "areaCode": "509",
          "countryCode": "1"
        }
      ],
      "addresses": [
        {
          "line1": "905 W Riverside Ave",
          "city": "Spokane",
          "state": "WA",
          "postalCode": "99201"
        }
      ]
    }
  }
}
```

---

## 4. Lookups before any new share row

`POST https://hhscoalitionmpi-api-int.hca.wa.gov/api/v1/identityLinkId`

Same headers. Do not send `matchScoreThreshold` or `maxSearchResults`. The API forces `GROUP_BY_SOURCE` internally.

Found:

```json
{
  "success": true,
  "message": "Identity found.",
  "content": { "linkId": "<a link id>" }
}
```

Hidden or no match:

```json
{
  "success": true,
  "message": "No identity found.",
  "content": { "linkId": null }
}
```

### 4a. Reddy, called as Verification Hub — expect Identity found

Own source. No mapping row.

```json
{
  "trackingId": "int-linkid-vh-reddy",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "DSHS",
  "ipAddress": "192.168.254.38",
  "content": {
    "identity": {
      "names": [
        { "first": "Reddy", "middle": "Bharath", "last": "Bandi" }
      ],
      "datesOfBirth": ["1991-04-16"],
      "ssns": ["534-28-6194"],
      "genders": ["M"],
      "addresses": [
        { "line1": "418 Legion Way SE", "city": "Olympia", "state": "WA", "postalCode": "98501" }
      ]
    }
  }
}
```

### 4b. Thakrim, called as Verification Hub — expect No identity found

She is on ACES. Hub cannot see ACES yet.

```json
{
  "trackingId": "int-linkid-vh-thakrim-before",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "DSHS",
  "ipAddress": "192.168.254.38",
  "content": {
    "identity": {
      "names": [
        { "first": "Thakrim", "last": "Shaik" }
      ],
      "datesOfBirth": ["1987-11-23"],
      "ssns": ["417-63-2085"],
      "genders": ["F"],
      "addresses": [
        { "line1": "732 St Helens Ave", "city": "Tacoma", "state": "WA", "postalCode": "98402" }
      ]
    }
  }
}
```

### 4c. Chakradhar, called as Verification Hub — expect No identity found

He is on HPF. Hub cannot see HPF.

```json
{
  "trackingId": "int-linkid-vh-chakradhar-before",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "DSHS",
  "ipAddress": "192.168.254.38",
  "content": {
    "identity": {
      "names": [
        { "first": "Chakradhar", "last": "Bandi" }
      ],
      "datesOfBirth": ["1994-07-09"],
      "ssns": ["361-90-7742"],
      "genders": ["M"],
      "addresses": [
        { "line1": "905 W Riverside Ave", "city": "Spokane", "state": "WA", "postalCode": "99201" }
      ]
    }
  }
}
```

### 4d. Empty identity — expect “Identity is required.”

```json
{
  "trackingId": "int-linkid-missing-identity",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "DSHS",
  "ipAddress": "192.168.254.38",
  "content": {
    "identity": null
  }
}
```

---

## 5. Allow Verification Hub to see ACES

Replace `int-linkid-test` with your user id. Use the enum label from step 2.

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
    'int-linkid-test',
    NOW(),
    TRUE
FROM coalitionmpi.onboarded_system caller
JOIN coalitionmpi.onboarded_system allowed
    ON LOWER(allowed.source_system_name) = 'wadshs.aces'
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
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

Expected insert count: `1`. If it errors, run `ROLLBACK;` and fix the enum label.

Confirm only ACES was added:

```sql
SELECT
    allowed.source_system_name AS allowed,
    dsm.data_sharing_level::text AS level,
    dsm.is_active
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
  AND dsm.data_sharing_level::text ILIKE 'linkid'
  AND dsm.is_active = TRUE
ORDER BY allowed.source_system_name;
```

`wadshs.aces` should be listed. `Wahbe.hpf` should not.

---

## 6. Lookups after the ACES share

### 6a. Thakrim again, same Verification Hub body as 4b

Change `trackingId` to `int-linkid-vh-thakrim-after`.

Expect **Identity found** and a link id.

### 6b. Chakradhar again, same Verification Hub body as 4c

Change `trackingId` to `int-linkid-vh-chakradhar-still-hidden`.

Expect **No identity found**. HPF is still not shared.

### 6c. Reddy again

Still **Identity found**. The new row must not change the own-source result.

---

## 7. Allow Verification Hub to see HealthPlanFinder

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
    'int-linkid-test',
    NOW(),
    TRUE
FROM coalitionmpi.onboarded_system caller
JOIN coalitionmpi.onboarded_system allowed
    ON LOWER(allowed.source_system_name) = 'wahbe.hpf'
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
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

Expected insert count: `1`.

Verify both:

```sql
SELECT allowed.source_system_name
FROM coalitionmpi.data_share_mapping dsm
JOIN coalitionmpi.onboarded_system caller ON caller.id = dsm.source_system_id
JOIN coalitionmpi.onboarded_system allowed ON allowed.id = dsm.allowed_system_id
WHERE LOWER(caller.source_system_name) = 'wadshs.verificationhub'
  AND dsm.data_sharing_level::text ILIKE 'linkid'
  AND dsm.is_active = TRUE
ORDER BY 1;
```

Expect `wadshs.aces` and `Wahbe.hpf`.

---

## 8. Chakradhar after the HPF share

Same body as 4c. `trackingId`: `int-linkid-vh-chakradhar-after`.

Expect **Identity found** and a link id.

---

## 9. Each system can see its own person

No extra mapping. The IP is what selects the caller.

### 9a. ACES looks up Thakrim — expect Identity found

```json
{
  "trackingId": "int-linkid-aces-thakrim",
  "sourceSystem": "wadshs.aces",
  "agency": "DSHS",
  "ipAddress": "198.239.193.47",
  "content": {
    "identity": {
      "names": [
        { "first": "Thakrim", "last": "Shaik" }
      ],
      "datesOfBirth": ["1987-11-23"],
      "ssns": ["417-63-2085"],
      "genders": ["F"],
      "addresses": [
        { "line1": "732 St Helens Ave", "city": "Tacoma", "state": "WA", "postalCode": "98402" }
      ]
    }
  }
}
```

### 9b. HPF looks up Chakradhar — expect Identity found

```json
{
  "trackingId": "int-linkid-hpf-chakradhar",
  "sourceSystem": "Wahbe.hpf",
  "agency": "Washington Health Benefit Exchange",
  "ipAddress": "20.187.52.46",
  "content": {
    "identity": {
      "names": [
        { "first": "Chakradhar", "last": "Bandi" }
      ],
      "datesOfBirth": ["1994-07-09"],
      "ssns": ["361-90-7742"],
      "genders": ["M"],
      "addresses": [
        { "line1": "905 W Riverside Ave", "city": "Spokane", "state": "WA", "postalCode": "99201" }
      ]
    }
  }
}
```

### 9c. ACES looks up Reddy — expect No identity found

Reddy is only on Verification Hub. The share rows point hub → ACES, not ACES → hub.

```json
{
  "trackingId": "int-linkid-aces-reddy",
  "sourceSystem": "wadshs.aces",
  "agency": "DSHS",
  "ipAddress": "198.239.193.47",
  "content": {
    "identity": {
      "names": [
        { "first": "Reddy", "middle": "Bharath", "last": "Bandi" }
      ],
      "datesOfBirth": ["1991-04-16"],
      "ssns": ["534-28-6194"],
      "genders": ["M"],
      "addresses": [
        { "line1": "418 Legion Way SE", "city": "Olympia", "state": "WA", "postalCode": "98501" }
      ]
    }
  }
}
```

---

## Pass checklist

| Step | Caller IP | Person | Expected |
| --- | --- | --- | --- |
| 4a | `192.168.254.38` | Reddy Bharath Bandi | Identity found |
| 4b | `192.168.254.38` | Thakrim Shaik | No identity found |
| 4c | `192.168.254.38` | Chakradhar Bandi | No identity found |
| 4d | `192.168.254.38` | none | Identity is required |
| 6a | `192.168.254.38` | Thakrim Shaik | Identity found |
| 6b | `192.168.254.38` | Chakradhar Bandi | No identity found |
| 6c | `192.168.254.38` | Reddy Bharath Bandi | Identity found |
| 8 | `192.168.254.38` | Chakradhar Bandi | Identity found |
| 9a | `198.239.193.47` | Thakrim Shaik | Identity found |
| 9b | `20.187.52.46` | Chakradhar Bandi | Identity found |
| 9c | `198.239.193.47` | Reddy Bharath Bandi | No identity found |
