# MPI identity link id lookup

`POST api/v1/identityLinkId`

The caller sends demographics and receives the MPI link id. The request body is the same as `POST api/v1/demographicsSearch`.

## Request

```json
{
  "trackingId": "",
  "sourceSystem": "wadshs.verificationhub",
  "agency": "",
  "ipAddress": "",
  "content": {
    "responseIdentityFormatNames": ["DEFAULT"],
    "matchScoreThreshold": 0.0,
    "maxSearchResults": 10,
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

No schema change. Add these rows to `coalitionmpi.data_share_mapping`. `Full` allows the link id to be returned. `existence` does not. 


| Caller                   | Can see          | Level  | Active |
| ------------------------ | ---------------- | ------ | ------ |
| `wadshs.verificationhub` | ProviderOne      | `Full` | true   |
| `wadshs.verificationhub` | HealthPlanFinder | `Full` | true   |
| `wadshs.verificationhub` | ACES             | `Full` | true   |


ESD and WTB are not included.