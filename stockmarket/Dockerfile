#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["stockmarket.csproj", "./"]
RUN dotnet restore "stockmarket.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "stockmarket.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "stockmarket.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV LD_LIBRARY_PATH=/home/site/wwwroot/bin/runtimes/linux-x64/native
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true