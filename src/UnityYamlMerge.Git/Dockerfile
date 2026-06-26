FROM docker:28-cli@sha256:625d9431a9f54c5a2bc90f24f0e1c3d55b1349fd857dd85035f98c2c9acbdd4d AS base
RUN apk add --no-cache git
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:548d93f8a18a1acbe6cc127bc4f47281430d34a9e35c18afa80a8d6741c2adc3 AS build
RUN apt-get update && apt-get install -y --no-install-recommends clang musl-tools && rm -rf /var/lib/apt/lists/*
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UnityYamlMerge.Git/UnityYamlMerge.Git.csproj", "src/UnityYamlMerge.Git/"]
RUN dotnet restore "src/UnityYamlMerge.Git/UnityYamlMerge.Git.csproj"
COPY . .
WORKDIR "/src/src/UnityYamlMerge.Git"
RUN dotnet build "./UnityYamlMerge.Git.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UnityYamlMerge.Git.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./UnityYamlMerge.Git"]