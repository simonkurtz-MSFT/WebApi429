FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-noble-chiseled AS base
WORKDIR /app
EXPOSE 8080

ENV HTTP_PORTS=
ENV ASPNETCORE_URLS=http://+:8080

USER app
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS publish
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -r linux-x64 -o /app/publish

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["./WebApi429"]
