# MPI identity link id lookup

`POST api/v1/identityLinkId`

The caller sends demographics and receives the MPI link id. Internally this uses `POST api/v1/demographicsQuery` so Verato returns one match with its built-in cutoff. `matchScoreThreshold` and `maxSearchResults` are not part of this request.

## Request

```json
{
  "trackingId": "",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "",
  "ipAddress": "",
  "content": {
    "responseIdentityFormatNames": ["DEFAULT"],
    "identity": {
      "names": [{ "first": "", "last": "" }],
      "datesOfBirth": [""],
      "ssns": [""],
      "genders": [""],
      "addresses": [
        { "line1": "", "city": "", "state": "", "postalCode": "" }
      ]
    }
  }
}
```

## Response

```json
{
  "trackingId": "",
  "auditId": "",
  "success": true,
  "message": "Identity found.",
  "content": {
    "linkId": ""
  }
}
```

When no link id is returned, `content.linkId` is null and `message` is "No identity found."

## Database

Add this row to `coalitionmpi.data_share_mapping`. Look up the `onboarded_system` id for `wadshs.verificationhub` and for `WAHCA.Providerone`, then insert. `LinkId` allows the link id to be returned.


| Caller                   | Can see            | Level    | Active |
| ------------------------ | ------------------ | -------- | ------ |
| `wadshs.verificationhub` | `WAHCA.Providerone` | `LinkId` | true   |


`Wahbe.hpf`, `wadshs.aces`, ESD, and WTB are not included in this change.

If Verification Hub is not onboarded in DEV, use dummy sources for internal testing — see `design_linkID_dev_dummy_sources.md` (`test1` → `wadoh.test`).