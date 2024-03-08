# SPDX-License-Identifier: MIT
# Copyright: 2023 Econolite Systems, Inc.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Api.SystemModeler/Api.SystemModeler.csproj", "Api.SystemModeler/"]
RUN dotnet restore "Api.SystemModeler/Api.SystemModeler.csproj"
COPY . .
WORKDIR "/src/Api.SystemModeler"
RUN dotnet build "Api.SystemModeler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.SystemModeler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.SystemModeler.dll"]
