FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["SprinklerNetCore/SprinklerNetCore.csproj", "SprinklerNetCore/"]
RUN dotnet restore "SprinklerNetCore/SprinklerNetCore.csproj"
COPY . .
WORKDIR "/src/SprinklerNetCore"
RUN dotnet build "SprinklerNetCore.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SprinklerNetCore.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SprinklerNetCore.dll"]