#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/runtime:latest AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:latest AS build
COPY . .
RUN ls
RUN dotnet restore "Portal/UI/Eventually.Portal.UI.csproj"
COPY . .
RUN dotnet build "Portal/UI/Eventually.Portal.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Portal/UI/Eventually.Portal.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eventually.Portal.UI.dll"]