FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
EXPOSE 80
EXPOSE 443
USER identity

FROM base AS build
WORKDIR /src
COPY . ./
RUN ["dotnet", "restore", "IdentityService.Presentation"]

FROM build AS development
WORKDIR /src
COPY --from=build /src .
ENTRYPOINT ["dotnet", "watch", "--project", "IdentityService.Presentation"]