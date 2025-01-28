FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TasksService/TasksService.csproj", "TasksService/"]
RUN dotnet restore "TasksService/TasksService.csproj"
COPY . .
WORKDIR "/src/TasksService"
RUN dotnet build "TasksService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TasksService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TasksService.dll"]