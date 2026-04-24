FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/genVApi/WebAPI/WebAPI.csproj", "src/genVApi/WebAPI/"]
COPY ["src/genVApi/Application/Application.csproj", "src/genVApi/Application/"]
COPY ["src/genVApi/Domain/Domain.csproj", "src/genVApi/Domain/"]
COPY ["src/genVApi/Infrastructure/Infrastructure.csproj", "src/genVApi/Infrastructure/"]
COPY ["src/genVApi/Persistence/Persistence.csproj", "src/genVApi/Persistence/"]
COPY ["src/corePackages/", "src/corePackages/"]

RUN dotnet restore "src/genVApi/WebAPI/WebAPI.csproj"

COPY . .
WORKDIR /src/src/genVApi/WebAPI
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebAPI.dll"]
