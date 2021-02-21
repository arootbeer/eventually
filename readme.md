# Eventually Monolithic Codebase

This repository currently holds all of the domain logic and a very basic portal front-end.

## Running Locally

To run the domain command handler API and portal UI locally, the following prerequisites must be completed:
* The following entries should be added to the local machine's HOSTS file:
  ```
  127.0.0.1     domain.eventual.ly
  127.0.0.1     portal.eventual.ly
  ```
  (yes, these are explicitly invalid domains)
* Run `certlm` and expand `Trusted Root Certification Authorities`
  * Right click on `Certificates`, then select `All Tasks`->`Import...`
  * Click "Next"  
  * Enter the path to `src\Domain\APIHost\domain.pfx`, then click "Next"
  * Enter "eventual.ly" in the `Password` field, then click "Next"
  * Click "Next"
  * Click "Next"
  * Click "Finish"
* Repeat the steps above for `src\Portal\UI\portal.pfx`
  
* At a command prompt, navigate to `src\Domain\APIHost` and run the following commands:
  ```
  dotnet user-secrets set "CertPassword" "eventual.ly"
  dotnet run
  ```
* At another command prompt, navigate to `src\Portal\UI` and run the same command:
  ```
  dotnet user-secrets set "CertPassword" "eventual.ly"
  dotnet run
  ```
 
You should now be able to access the following links:
https://domain.eventual.ly:5001/swagger/index.html
https://portal.eventual.ly/