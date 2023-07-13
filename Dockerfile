FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/ShounenGaming.API/ShounenGaming.API.csproj", "src/ShounenGaming.API/"]
COPY ["src/ShounenGaming.Common/ShounenGaming.Common.csproj", "src/ShounenGaming.Common/"]
COPY ["src/ShounenGaming.Business/ShounenGaming.Business.csproj", "src/ShounenGaming.Business/"]
COPY ["src/ShounenGaming.DTOs/ShounenGaming.DTOs.csproj", "src/ShounenGaming.DTOs/"]
COPY ["src/ShounenGaming.DataAccess/ShounenGaming.DataAccess.csproj", "src/ShounenGaming.DataAccess/"]
COPY ["src/ShounenGaming.Core/ShounenGaming.Core.csproj", "src/ShounenGaming.Core/"]
RUN dotnet restore "src/ShounenGaming.API/ShounenGaming.API.csproj"
COPY . .
WORKDIR "/src/src/ShounenGaming.API"
RUN dotnet build "ShounenGaming.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShounenGaming.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir mangas
RUN mkdir logs

ENTRYPOINT ["dotnet", "ShounenGaming.API.dll"]