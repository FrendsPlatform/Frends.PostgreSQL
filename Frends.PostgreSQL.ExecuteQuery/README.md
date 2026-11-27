# Frends.PostgreSQL.ExecuteQuery

Frends Task for executing queries or non-queries against a PostgreSQL database.

[![Build status](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions/workflows/Execute_main_release.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions/workflows/Execute_main_release.yml)
![Code Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.PostgreSQL/Frends.PostgreSQL.ExecuteQuery|main)
[![Repository License](https://img.shields.io/github/license/FrendsPlatform/Frends.PostgreSQL?label=License)](https://github.com/FrendsPlatform/Frends.PostgreSQL/blob/main/LICENSE)

## Installing

You can install the Task via Frends UI Task View or you can find the NuGet package from the following NuGet feed https://www.myget.org/F/frends-tasks/api/v2.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.PostgreSQL.git`

### Build the project

`dotnet build`

### Run tests

The tests require a local PostgreSQL database. Start one with Docker:

`docker run -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d postgres`

Then run:

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`