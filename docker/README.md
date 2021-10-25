# Installation

docker-compose --project-name teamcity up -d

# Create more agent

docker-compose --project-name teamcity up -d --scale agent=5

# Update

docker-compose --project-name teamcity up -d --build --force-recreate

# JetBrains Docker images

https://github.com/JetBrains/teamcity-docker-images

# Change Log

## 2021-10-25

- update image versions

## 2021-02-01

- enable build in docker support
- agent image now contains node.js LTS installation
- agent image now contains build tools installation