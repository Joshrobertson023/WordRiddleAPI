# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything into the build context
COPY . .

# Publish the API project
RUN dotnet publish WordRiddleAPI/WordRiddleAPI.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV PORT=5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "WordRiddleAPI.dll"]
