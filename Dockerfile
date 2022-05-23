FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
WORKDIR /src
COPY . .
RUN dotnet restore "EthernaCredit.sln"
RUN dotnet build "EthernaCredit.sln" -c Release -o /app/build
RUN dotnet test "EthernaCredit.sln" -c Release

FROM build AS publish
RUN dotnet publish "EthernaCredit.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EthernaCredit.dll"]