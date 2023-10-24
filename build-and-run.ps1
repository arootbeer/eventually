param(
    [switch] $BuildOnly = $false
)

docker compose -f $PSScriptRoot\build\docker-compose-deps.yml pull
dotnet build $PSScriptRoot\src\Domain\APIHost\Eventually.Domain.APIHost.csproj
dotnet build $PSScriptRoot\src\Portal\UI\Eventually.Portal.UI.csproj

if ($BuildOnly -eq $true) {
    return
}

Start-Process pwsh -ArgumentList "-noprofile -command docker compose -f build\docker-compose-deps.yml up eventstore"
Start-Process pwsh -ArgumentList "-noprofile -command docker compose -f build\docker-compose-deps.yml up domain-data"
Start-Process pwsh -ArgumentList "-noprofile -command docker compose -f build\docker-compose-deps.yml up portal-viewmodel"
Start-Process pwsh -ArgumentList "-noprofile -command docker compose -f build\docker-compose-deps.yml up pgadmin"

Write-Information "Sleeping 15 seconds to allow containers to initialize"
Start-Sleep -Seconds 15
Start-Process pwsh -ArgumentList "-noprofile -command dotnet run --project $PSScriptRoot\src\Domain\APIHost\Eventually.Domain.APIHost.csproj"
Write-Information "Sleeping 10 seconds to allow Domain API Host to initialize"
Start-Sleep -Seconds 10
Start-Process pwsh -ArgumentList "-noprofile -command dotnet run --project $PSScriptRoot\src\Portal\UI\Eventually.Portal.UI.csproj"