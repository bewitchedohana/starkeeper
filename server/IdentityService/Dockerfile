FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
EXPOSE 80
EXPOSE 443

FROM base AS build
WORKDIR /app
COPY . ./
RUN ["dotnet", "restore"]

FROM build AS development
WORKDIR /app
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
RUN dotnet tool install --global dotnet-ef
COPY --from=build /app .
ENTRYPOINT ["dotnet", "watch", "--project", "src/IdentityService.Presentation"]