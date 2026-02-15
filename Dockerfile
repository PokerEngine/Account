FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /usr/local/account

CMD ["dotnet", "watch", "--project", "src/Infrastructure", "run"]
