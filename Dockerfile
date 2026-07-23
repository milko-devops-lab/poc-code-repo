# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore distinct layers for caching
COPY ["Claims.Api/Claims.Api.csproj", "Claims.Api/"]
RUN dotnet restore "Claims.Api/Claims.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Claims.Api"
RUN dotnet publish "Claims.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS final
WORKDIR /app

# Create non-root user for K8s / OpenShift security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Claims.Api.dll"]