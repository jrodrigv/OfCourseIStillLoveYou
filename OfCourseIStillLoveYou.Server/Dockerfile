FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY OfCourseIStillLoveYou.Server.csproj .
RUN dotnet restore OfCourseIStillLoveYou.Server.csproj
COPY . .
RUN dotnet build "OfCourseIStillLoveYou.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OfCourseIStillLoveYou.Server.csproj" -c Release --self-contained false --runtime linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OfCourseIStillLoveYou.Server.dll"]