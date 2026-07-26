# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore distinct layers
COPY ["Claims.Api/Claims.Api.csproj", "Claims.Api/"]
RUN dotnet restore "Claims.Api/Claims.Api.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/Claims.Api"
RUN dotnet publish "Claims.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Built-in non-root user 'app' for Kubernetes/OpenShift SCC policy compliance
USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Claims.Api.dll"]