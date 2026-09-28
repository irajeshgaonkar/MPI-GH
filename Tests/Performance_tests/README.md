# JMeter Performance Test Plan

One Apache JMeter plan for the Identity APIs. The host comes from a properties file. Request bodies live in the plan and can be edited there.

## Environment

| Environment | Properties file | Host |
|---|---|---|
| Dev | `config/dev.properties` | `hhscoalitionmpi-api-dev.hca.wa.gov` |
| Int | `config/int.properties` | `hhscoalitionmpi-api-int.hca.wa.gov` |
| Test | `config/test.properties` | `hhscoalitionmpi-api-test.hca.wa.gov` |

The `Server` user-defined variable is `${__P(Server,hhscoalitionmpi-api-dev.hca.wa.gov)}`. With no properties file, the plan calls Dev.


Load defaults in the Thread Group: 10 threads, 10 loops, all samplers enabled. Change those in the plan when you want a heavier run.

## APIs

- Post Identity
- Demographic Query
- Demographic Search
- Link Identity
- Unlink Identity
- Merge Identity
- Unmerge Identity
- Identity Exists
- Enrich

## Prerequisites

- Apache JMeter 5.4 or higher: https://jmeter.apache.org/
- Java 8 or higher

## Run from the command line

From the repository root:

```text
jmeter -n -t performance/ExternalPerformance.jmx -q performance/config/dev.properties
jmeter -n -t performance/ExternalPerformance.jmx -q performance/config/int.properties
jmeter -n -t performance/ExternalPerformance.jmx -q performance/config/test.properties
```

## Run from the JMeter GUI

1. Start JMeter with the properties file so `Server` is set before the plan runs:

```text
jmeter -q performance/config/int.properties
```

2. File → Open → `performance/ExternalPerformance.jmx`
3. Options → SSL Manager, and upload the certificate.
4. Start the test and enter the certificate passphrase when prompted.
5. Read results in View Results Tree, Summary Report, and Aggregate Report.

To point at another host without a properties file, change the `Server` value under User Defined Variables.

## Results

- Average response time
- Throughput
- Error rate
- Summary Report, Aggregate Report (including 90th, 95th, and 99th percentiles), and View Results Tree

## Plan layout

```text
Test Plan
├── User Defined Variables
├── Thread Group
│   ├── Loop Controller
│   │   ├── Post Identity
│   │   ├── Demographic Query
│   │   ├── Demographic Search
│   │   ├── Link Identity
│   │   ├── Unlink Identity
│   │   ├── Merge Identity
│   │   ├── Unmerge Identity
│   │   ├── Identity Exists
│   │   ├── Enrich
│   │   └── Synchronizing Timer
├── View Results Tree
├── Aggregate Report
└── Summary Report
```

## Notes

- Use unique IDs and emails so runs do not collide.
- Edit a sampler body in this plan when a payload needs to change. Do not copy the plan per environment.
- Start from the default thread count before raising it.
