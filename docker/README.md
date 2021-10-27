# Installation

docker-compose --project-name teamcity up -d

# Create more agent

docker-compose --project-name teamcity up -d --scale agent=5

# Update

docker-compose --project-name teamcity up -d --build --force-recreate

# JetBrains Docker images

https://github.com/JetBrains/teamcity-docker-images

# Troubleshooting

In case you are getting errors:
The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached

You might need to execute this on host machine:

```
echo fs.inotify.max_user_instances=524288 | sudo tee -a /etc/sysctl.conf && sudo sysctl -p
```

# Change Log

## 2021-10-25

- update image versions

## 2021-02-01

- enable build in docker support
- agent image now contains node.js LTS installation
- agent image now contains build tools installation