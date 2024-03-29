# Installation

## Prerequisites

```
mkdir files
chown -R 1000:1000
```

## Note

In docker-compose.yaml file it is expected that teamcity is running behind load balancer.
However if at the begining you are planning to access it directly (for example for setup or even run always without load balancer)
then set TEAMCITY_HTTPS_PROXY_ENABLED to false in docker-compose.yaml

## Install

```
docker-compose --project-name teamcity up -d
```

# Create more agent

```
docker-compose --project-name teamcity up -d --scale agent=5
```

# Update

```
docker-compose --project-name teamcity up -d --build --force-recreate
```

# JetBrains Docker images

https://github.com/JetBrains/teamcity-docker-images

# Use of PostgreSql

You can use included PostgreSQL container to store TeamCity configuration (or for integration test).
However keep in mind - it does not have any volumes configured so any update
might lose the data stored in it. Consult PostgreSQL to configure volumes properly.

When configuring TeamCity, you can enter these data:
HostName: postgres
User: admin
Password: admin

In order to access PgAdmin, open http://localhost:5480 and use credentials:
User: admin@postgres.com
Password: postgres

Attention: please do not expose PostgreSQL and PgAdmin endpoints to public unless you really know what are you doing (and of course use more safe passwords).

## Drop test_ databases

```
CREATE EXTENSION IF NOT EXISTS dblink;

DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT datname FROM pg_database where datname like 'test%') LOOP
        PERFORM dblink_exec('port=5432 dbname=postgres', 'DROP DATABASE ' || r.datname);
    END LOOP;
END $$;
```

# Troubleshooting

In case you are getting errors:
The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached

You might need to execute this on host machine:

```
echo fs.inotify.max_user_instances=524288 | sudo tee -a /etc/sysctl.conf && sudo sysctl -p
```

Connect to server container:

```
docker exec -it teamcity-server bash
```

To find out configured database properties:

```
cat /data/teamcity_server/datadir/config/database.properties
```

If teamcity asks for super user token:

```
cat /opt/teamcity/logs/teamcity-server.log | grep Super
```


# Change Log

## 2022-09-06

- upgrade version

## 2021-12-31

- add PostgreSql and PgAdmin containers

## 2021-11-10

- update to .NET6

## 2021-10-25

- update image versions

## 2021-02-01

- enable build in docker support
- agent image now contains node.js LTS installation
- agent image now contains build tools installation