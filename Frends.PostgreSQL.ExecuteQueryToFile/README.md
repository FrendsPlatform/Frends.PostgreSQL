# Frends.PostgreSQL.ExecuteQueryToFile

Executes a PostgreSQL query and writes the results directly into a CSV or JSON file.

[![Build status](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions/workflows/ExecuteQueryToFile_main_release.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions/workflows/ExecuteQueryToFile_main_release.yml)
![Code Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.PostgreSQL/Frends.PostgreSQL.ExecuteQueryToFile|main)
[![Repository License](https://img.shields.io/github/license/FrendsPlatform/Frends.PostgreSQL?label=License)](https://github.com/FrendsPlatform/Frends.PostgreSQL/blob/main/LICENSE)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.PostgreSQL.git`

### Build the project

`dotnet build`

### Run tests

The tests start a PostgreSQL container with Testcontainers, so Docker must be running.

```
dotnet test
```

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version
This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and improvements not available in the last stable release.
