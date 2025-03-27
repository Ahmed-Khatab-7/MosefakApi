# Use the official .NET 9.0 SDK image as the build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory in the container
WORKDIR /src

# Copy the entire repository content
COPY . .

# Find all solution files and restore NuGet packages for them
RUN find . -name "*.sln" -exec dotnet restore {} \;

# Build the main project
WORKDIR "/src"
RUN dotnet build "MosefakApi.sln" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MosefakApi.sln" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9.0 runtime image as the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set the working directory
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Set the environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 80
EXPOSE 80

# Define the entry point for the application
ENTRYPOINT ["dotnet", "MosefakApi.dll"]
