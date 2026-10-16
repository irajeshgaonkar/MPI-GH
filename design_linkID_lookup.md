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

Add these rows to `coalitionmpi.data_share_mapping`. Look up the `onboarded_system` id for `wadshs.verificationhub` and the id for each target by `source_system_name`, then insert. `LinkId` allows the link id to be returned. 


| Caller                   | Can see          | Level  | Active |
| ------------------------ | ---------------- | ------ | ------ |
| `wadshs.verificationhub` | ProviderOne      | `LinkId` | true   |
| `wadshs.verificationhub` | HealthPlanFinder | `LinkId` | true   |
| `wadshs.verificationhub` | ACES             | `LinkId` | true   |


ESD and WTB are not included.