# Eventual.ly Monolithic Codebase

This repository currently holds all of the domain logic and a very basic portal front-end. Only the codebase
is monolithic, for ease of development - the application server is designed to be highly distributed. 

## Running Locally

### TL;DR
* First time only: run `build\dev-secrets.ps1`, then 
* Run `build-and-run.ps1` from a Powershell window 

### The long version
Out of the box, Eventual.ly relies on PostgresQL for event storage and MongoDB for projection storage. In the
`/build` folder, you can run `docker compose -f .\docker-compose-deps.yml up --build` to spin up a local
stack with 2 PGSQL servers (one for event storage and one to act as a 3NF projection repository), a MongoDB
server that's used for the portal UI, as well as both PGAdmin and Mongo-Express management tools. The default
configurations in `src/Domain/ApiHost/appSettings.json` and `src/Portal/UI/appsettings.json` are configured to
connect to these databases. Once this stack is up and running, follow the directions below to build and run the code.

To run the domain command handler API and portal UI locally, the following prerequisites must be completed:
* As an administrator, run `build\dev-secrets.ps1` to install the self-signed certificates for each of the HTTP 
applications. You should see output similar to the following 
  ```
  Thumbprint                                Subject              EnhancedKeyUsageList
  ----------                                -------              --------------------
  586E7FC590A6A9AF1388C9F9EF448D308E1038E4  CN=domain.eventual.… {Client Authentication, Server Authentication}
  FEEE4443849BE3CA4C305CEF54D02D7C9211442C  CN=portal.eventual.… {Client Authentication, Server Authentication}
  Successfully saved CertPassword = eventual.ly to the secret store.
  Successfully saved CertPassword = eventual.ly to the secret store.
  ```
* Add the following entries to the local machine's HOSTS file:
  ```
  127.0.0.1     domain.eventual.ly
  127.0.0.1     portal.eventual.ly
  ```
* In two separate terminal windows, run the following commands:
  ```
  dotnet run --project src\Domain\APIHost\Eventually.Domain.APIHost.csproj
  Start-Sleep -Seconds 10
  dotnet run --project src\Portal\UI\Eventually.Portal.UI.csproj
  ```

You should now be able to access the following links:

https://domain.eventual.ly:5001/swagger/index.html

https://portal.eventual.ly/