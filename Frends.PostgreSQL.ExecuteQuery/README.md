# Frends.PostgreSQL.ExecuteQuery
Frends Task for executing queries or non-queries to PostgreSQL database.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions/workflows/ExecuteQuery_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.PostgreSQL/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.PostgreSQL.ExecuteQuery)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.PostgreSQL/Frends.PostgreSQL.ExecuteQuery|main)

# Installing

You can install the Task via Frends UI Task View or you can find the NuGet package from the following NuGet feed https://www.myget.org/F/frends-tasks/api/v2.

## Building


Build the project

 `dotnet build`

Run tests

 To run tests you need to test database. You can start a test database using Docker with
 `docker run -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d postgres`
 
 When container is running you can start test with
 `dotnet test`


Create a NuGet package

 `dotnet pack --configuration Release`