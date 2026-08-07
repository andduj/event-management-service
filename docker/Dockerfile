FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY EventManagement.sln ./
COPY src/ ./src/

ARG PROJECT_PATH
RUN dotnet restore "$PROJECT_PATH"
RUN dotnet publish "$PROJECT_PATH" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ARG ENTRY_DLL
ENV ENTRY_DLL=$ENTRY_DLL
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT dotnet ${ENTRY_DLL}
