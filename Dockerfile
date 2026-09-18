# Etapa de construção
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["crudcomdb.csproj", "./"]
RUN dotnet restore "crudcomdb.csproj"
COPY . .
RUN dotnet publish "crudcomdb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "crudcomdb.dll"]