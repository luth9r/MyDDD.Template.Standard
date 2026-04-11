FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MyDDD.Template.Api/MyDDD.Template.Api.csproj", "src/MyDDD.Template.Api/"]
COPY ["src/MyDDD.Template.Infrastructure/MyDDD.Template.Infrastructure.csproj", "src/MyDDD.Template.Infrastructure/"]
COPY ["src/MyDDD.Template.Application/MyDDD.Template.Application.csproj", "src/MyDDD.Template.Application/"]
COPY ["src/MyDDD.Template.Domain/MyDDD.Template.Domain.csproj", "src/MyDDD.Template.Domain/"]

RUN dotnet restore "src/MyDDD.Template.Api/MyDDD.Template.Api.csproj"

COPY . .
WORKDIR "/src/src/MyDDD.Template.Api"
RUN dotnet build "MyDDD.Template.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyDDD.Template.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyDDD.Template.Api.dll"]
