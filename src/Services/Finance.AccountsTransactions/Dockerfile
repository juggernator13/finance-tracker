FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY Directory.Build.props ./
COPY src ./src
RUN dotnet src/Services/Finance.AccountsTransactions -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Finance.AccountsTransactions.dll"]