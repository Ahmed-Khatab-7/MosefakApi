# ---------------------------------------------------------
# 1. Build Stage
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Create a working directory
WORKDIR /src

# Copy all project files into the container
COPY . .

# Restore NuGet packages using your main solution
RUN dotnet restore MosefakApp.sln

# Build the solution in Release mode
RUN dotnet build MosefakApp.sln -c Release -o /app/build

# Publish the solution to /app/publish
RUN dotnet publish MosefakApp.sln -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------
# 2. Runtime Stage
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Create a working directory for the app
WORKDIR /app

# Copy published files from build image
COPY --from=build /app/publish ./

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 80 for container traffic
EXPOSE 80

# Finally, run the published DLL
ENTRYPOINT ["dotnet", "MosefakApp.API.dll"]
