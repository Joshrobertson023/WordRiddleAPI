# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish WordRiddleAPI/WordRiddleAPI.csproj -c Release -o /app/publish

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "WordRiddleAPI.dll"]
