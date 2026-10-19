# Identity Link Id — Bruno test

`POST /api/v1/identityLinkId`

This endpoint uses demographics query under the hood. Use the query-style payload, not search. Do not send `matchScoreThreshold` or `maxSearchResults`. `responseIdentityFormatNames` (RIFN) is not required; the API always uses `GROUP_BY_SOURCE` internally.

---

## Bruno setup

1. Open **P0_TestsCases**.
2. New Folder → `11_Identity Link Id`.
3. Inside that folder → New Request:
   - Name: `Successful Identity Link Id`
   - Method: `POST`
   - URL: `{{URL}}/api/v1/identityLinkId`
4. Headers:
   - `Accept` = `application/json`
   - `Content-Type` = `application/json`
5. Body → JSON → paste the request body below.
6. Save.

Optional env var (DEV):

| Name | Value |
| --- | --- |
| `IdentityLinkId` | `/api/v1/identityLinkId` |

Then URL can be `{{URL}}{{IdentityLinkId}}`.

DEV base URL is already `https://hhscoalitionmpi-api-dev.hca.wa.gov`.

---

## Request body

Same demographic vars as demographics query. No RIFN.

```json
{
  "trackingId": "{{trackingId}}",
  "sourceSystem": "{{Source_name}}",
  "agency": "",
  "ipAddress": "",
  "content": {
    "identity": {
      "emails": ["{{Email}}"],
      "addresses": [
        {
          "line1": "{{AddressLine1}}",
          "line2": "{{AddressLine2}}",
          "city": "{{City}}",
          "state": "{{State}}",
          "postalCode": "{{ZipCode}}"
        }
      ],
      "names": [
        {
          "first": "{{FirstName}}",
          "middle": "{{MiddleName}}",
          "last": "{{LastName}}",
          "suffix": "{{Suffix}}"
        }
      ],
      "ssns": ["{{SSN}}"],
      "genders": ["{{Gender}}"],
      "datesOfBirth": ["{{DateOfBirth}}"],
      "phoneNumbers": [
        {
          "number": "{{Number}}",
          "areaCode": "{{AreaCode}}",
          "extension": "{{Extension}}",
          "countryCode": "{{CountryCode}}"
        }
      ]
    }
  }
}
```

In DEV, `Source_name` is `WAHCA.Providerone`.

---

## How to get demographic values

Those vars are usually set by the demographics query before-request script, not hard-coded in the DEV env.

Option A:

1. Run **Successful Demographic query** first (it posts an identity and sets the vars).
2. Run **Successful Identity Link Id** with the body above.

Option B:

Copy the before-request script from **Successful Demographic query - Default** onto this request, and set `SourceName` / `sourceSystem` to `WAHCA.Providerone` (or `{{Source_name}}`).

---

## Expected response

Found:

```json
{
  "trackingId": "...",
  "auditId": "...",
  "success": true,
  "message": "Identity found.",
  "content": {
    "linkId": "..."
  }
}
```

Not found / not visible to caller:

```json
{
  "success": true,
  "message": "No identity found.",
  "content": {
    "linkId": null
  }
}
```

---

## Notes

- Without LinkId data share mappings, a link id is returned only when the matched identity already includes the calling `sourceSystem` (for example `WAHCA.Providerone`).
- Verification Hub mapping (`wadshs.verificationhub` → `WAHCA.Providerone`) is a later DB step. See `design_linkID_db_setup.md`.
- Query vs search: identity block is the same; search adds `matchScoreThreshold` and `maxSearchResults`. Do not use those on this endpoint.
