FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "todo_app/todo_app.csproj"
WORKDIR "/src/todo_app"
RUN dotnet build "todo_app.csproj" -c Release -o /app/build
RUN dotnet publish "todo_app.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "todo_app.dll"] 
