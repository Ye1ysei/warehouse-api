FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY Warehouse/*.csproj ./Warehouse/
COPY Warehouse.Tests/*.csproj ./Warehouse.Tests/
RUN dotnet restore

COPY . .
WORKDIR /app/Warehouse
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Warehouse.dll"]