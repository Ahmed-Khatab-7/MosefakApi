# Use the official .NET 9.0 SDK image as the build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory in the container
WORKDIR /src

# Copy the project files to the container
COPY ["MosefakApi/MosefakApi.csproj", "MosefakApi/"]
COPY ["MosefakApi.Business/MosefakApi.Business.csproj", "MosefakApi.Business/"]
COPY ["MosefakApi.DependencyInjection/MosefakApi.DependencyInjection.csproj", "MosefakApi.DependencyInjection/"]
COPY ["MosefakApp.Core/MosefakApp.Core.csproj", "MosefakApp.Core/"]

# Restore NuGet packages
RUN dotnet restore "MosefakApi/MosefakApi.csproj"

# Copy the remaining source code
COPY . .

# Build the application
WORKDIR "/src/MosefakApi"
RUN dotnet build "MosefakApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MosefakApi.csproj" -c Release -o /app/publish

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