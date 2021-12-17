FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FluentBlazorRouter.Test/FluentBlazorRouter.Test.csproj", "FluentBlazorRouter.Test/"]
RUN dotnet restore "FluentBlazorRouter.Test/FluentBlazorRouter.Test.csproj"
COPY . .
WORKDIR "/src/FluentBlazorRouter.Test"
RUN dotnet build "FluentBlazorRouter.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluentBlazorRouter.Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluentBlazorRouter.Test.dll"]
