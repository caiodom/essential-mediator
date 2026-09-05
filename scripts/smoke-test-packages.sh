#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
package_dir="$repo_root/artifacts/packages"
smoke_dir="$repo_root/artifacts/package-smoke"
consumer_dir="$smoke_dir/Consumer"
consumer_project="$consumer_dir/Consumer.csproj"
nuget_config="$smoke_dir/NuGet.config"

shopt -s nullglob
packages=("$package_dir"/EssentialMediator.Extensions.DependencyInjection.*.nupkg)

if (( ${#packages[@]} != 1 )); then
  echo "Expected exactly one EssentialMediator.Extensions.DependencyInjection .nupkg, found ${#packages[@]}." >&2
  exit 1
fi

package_file="$(basename "${packages[0]}")"
package_version="${package_file#EssentialMediator.Extensions.DependencyInjection.}"
package_version="${package_version%.nupkg}"

rm -rf "$smoke_dir"
mkdir -p "$consumer_dir"
trap 'rm -rf "$smoke_dir"' EXIT

cat > "$consumer_project" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="EssentialMediator.Extensions.DependencyInjection" Version="$package_version" />
  </ItemGroup>
</Project>
EOF

cat > "$nuget_config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$package_dir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="EssentialMediator*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="Microsoft.*" />
      <package pattern="System.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
EOF

cat > "$consumer_dir/Program.cs" <<'EOF'
using EssentialMediator.Abstractions.Handlers;
using EssentialMediator.Abstractions.Messages;
using EssentialMediator.Extensions;
using EssentialMediator.Mediation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddEssentialMediator(typeof(Program).Assembly);

if (!services.Any(descriptor => descriptor.ServiceType == typeof(IMediator)))
{
    throw new InvalidOperationException("IMediator was not registered from the packaged DI extension.");
}

if (!services.Any(descriptor =>
        descriptor.ServiceType == typeof(IRequestHandler<PackageSmokeRequest, string>)))
{
    throw new InvalidOperationException("The packaged assembly scanner did not register the request handler.");
}

Console.WriteLine("NuGet package smoke test passed.");

public sealed record PackageSmokeRequest(string Value) : IRequest<string>;

public sealed class PackageSmokeRequestHandler : IRequestHandler<PackageSmokeRequest, string>
{
    public Task<string> Handle(
        PackageSmokeRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(request.Value);
}
EOF

echo "Restoring package consumer against EssentialMediator.Extensions.DependencyInjection $package_version..."
dotnet restore "$consumer_project" --configfile "$nuget_config"

echo "Building package consumer with warnings as errors..."
dotnet build "$consumer_project" --no-restore --configuration Release --warnaserror

echo "Running package consumer..."
dotnet run --project "$consumer_project" --configuration Release --no-build
