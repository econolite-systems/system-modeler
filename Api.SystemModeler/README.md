# SPDX-License-Identifier: MIT
# Copyright: 2023 Econolite Systems, Inc.

Add secrets for dev in a secrets.json file

secrets.json
```javascript
{
  "Kafka": {
    "GroupId": "system-modeler.prod",
    "bootstrap": {
      "servers": "localhost:9092"
    }
  },
  "ConnectionStrings": {
    "Mongo": "mongodb://localhost:27020"
  }
}
```

To add all of the secrets.json to the user-secrets do the following:

Windows

```
type .\secrets.json | dotnet user-secrets set
```

Mac/Linux

```
cat ./secrets.json | dotnet user-secrets set
```