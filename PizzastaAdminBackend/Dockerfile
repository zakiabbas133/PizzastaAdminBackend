# ==========================================
# Build stage
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file first
COPY ["PizzastaAdminBackend.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "PizzastaAdminBackend.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish "PizzastaAdminBackend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# ==========================================
# Runtime stage
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Render uses port 10000
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

# Start ASP.NET Core application
ENTRYPOINT ["dotnet", "PizzastaAdminBackend.dll"]