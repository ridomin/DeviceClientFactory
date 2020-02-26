#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TestDevice/TestDevice.csproj", "TestDevice/"]
COPY ["Rido.DeviceClientFactory/Rido.DeviceClientFactory.csproj", "Rido.DeviceClientFactory/"]
RUN dotnet restore "TestDevice/TestDevice.csproj"
COPY . .
WORKDIR "/src/TestDevice"
RUN dotnet build "TestDevice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestDevice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestDevice.dll"]