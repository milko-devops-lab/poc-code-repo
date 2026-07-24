# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore distinct layers for caching
COPY ["Claims.Api/Claims.Api.csproj", "Claims.Api/"]
RUN dotnet restore "Claims.Api/Claims.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Claims.Api"
RUN dotnet publish "Claims.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Използваме вградения в .NET 8/9/10 non-root потребител (app) за K8s / OpenShift сигурност
USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Claims.Api.dll"]