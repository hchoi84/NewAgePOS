#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NewAgePOS/NewAgePOS.csproj", "NewAgePOS/"]
COPY ["SkuVaultLibrary/SkuVaultLibrary.csproj", "SkuVaultLibrary/"]
COPY ["NewAgePOSModels/NewAgePOSModels.csproj", "NewAgePOSModels/"]
COPY ["ChannelAdvisorLibrary/ChannelAdvisorLibrary.csproj", "ChannelAdvisorLibrary/"]
COPY ["NewAgePOSLibrary/NewAgePOSLibrary.csproj", "NewAgePOSLibrary/"]
COPY ["EmailSenderLibrary/EmailSenderLibrary.csproj", "EmailSenderLibrary/"]
RUN dotnet restore "NewAgePOS/NewAgePOS.csproj"
COPY . .
WORKDIR "/src/NewAgePOS"
RUN dotnet build "NewAgePOS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewAgePOS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewAgePOS.dll"]