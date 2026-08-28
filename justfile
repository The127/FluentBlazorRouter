# FluentBlazorRouter task runner. Run `just` to see the available recipes.

set shell := ["bash", "-euo", "pipefail", "-c"]

lib := "FluentBlazorRouter/FluentBlazorRouter.csproj"
sln := "FluentBlazorRouter.sln"
artifacts := "artifacts"

# List the available recipes.
default:
    @just --list

# Build the solution in Release.
build:
    dotnet build {{sln}} --configuration Release

# Remove build output.
clean:
    rm -rf {{artifacts}}
    dotnet clean {{sln}} --configuration Release

# Build a package locally. Defaults to 0.0.0-dev so a local build is never mistaken for a release.
pack version="0.0.0-dev":
    rm -rf {{artifacts}}
    dotnet pack {{lib}} --configuration Release -p:Version={{version}} --output {{artifacts}}

# Everything CI checks, run locally.
check: check-version-source build
    @echo "OK"

# Assert the package version still follows the version passed in at build time.
check-version-source:
    #!/usr/bin/env bash
    set -euo pipefail
    # The release workflow derives the version from the git tag and passes -p:Version.
    # A <PackageVersion> property anywhere (csproj, Directory.Build.props) silently
    # overrides that, decoupling the published package from its tag - which is what
    # shipped the broken 1.2.2 (issue #3). Probe the evaluated property rather than
    # grepping for text, so any way of pinning it is caught.
    sentinel="9.9.9-versionguard"
    actual="$(dotnet msbuild {{lib}} -getProperty:PackageVersion -p:Version="$sentinel" -nologo)"
    actual="$(tr -d '[:space:]' <<<"$actual")"
    if [[ "$actual" != "$sentinel" ]]; then
        echo "error: PackageVersion evaluated to '$actual' instead of following -p:Version." >&2
        echo "       Something pins the package version; remove it. See issue #3." >&2
        exit 1
    fi
    echo "OK: package version follows -p:Version."

# Pack a version and assert the package and assembly versions both match it.
verify version: (pack version)
    #!/usr/bin/env bash
    set -euo pipefail
    expected="{{version}}"
    work="$(mktemp -d)"
    trap 'rm -rf "$work"' EXIT

    nupkg="$(ls {{artifacts}}/*.nupkg)"
    unzip -q "$nupkg" -d "$work"

    package_version="$(sed -n 's:.*<version>\(.*\)</version>.*:\1:p' "$work"/*.nuspec | head -1)"
    if [[ "$package_version" != "$expected" ]]; then
        echo "error: package version '$package_version' != requested '$expected'." >&2
        exit 1
    fi

    # The assembly version is what NuGet clients and decompilers actually see. If it does not
    # move between releases, consumers keep resolving their cached copy of the old binaries.
    expected_assembly="${expected%%-*}.0"
    dll="$(find "$work/lib" -name 'FluentBlazorRouter.dll' | head -1)"
    if ! strings -el "$dll" | grep -qx "$expected_assembly"; then
        echo "error: assembly version in $dll is not '$expected_assembly'." >&2
        exit 1
    fi

    echo "OK: package $package_version, assembly $expected_assembly"

# Cut a release (`just release 1.2.3`): tag the current commit and push it.
release version:
    #!/usr/bin/env bash
    set -euo pipefail
    # GitHub Actions packs and publishes. Nothing is published from here - pushing the tag
    # is the only action this recipe takes, and it asks before doing so.
    version="{{version}}"
    tag="v${version}"

    if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
        echo "error: '$version' is not MAJOR.MINOR.PATCH[-prerelease]." >&2
        exit 1
    fi

    if [[ -n "$(git status --porcelain)" ]]; then
        echo "error: working tree is not clean; commit or stash first." >&2
        exit 1
    fi

    branch="$(git rev-parse --abbrev-ref HEAD)"
    if [[ "$branch" != "master" ]]; then
        echo "error: releases are cut from master, not '$branch'." >&2
        exit 1
    fi

    git fetch origin master --tags --quiet
    if [[ "$(git rev-parse HEAD)" != "$(git rev-parse origin/master)" ]]; then
        echo "error: master is not in sync with origin/master; pull or push first." >&2
        exit 1
    fi

    if git rev-parse -q --verify "refs/tags/${tag}" >/dev/null; then
        echo "error: tag ${tag} already exists locally." >&2
        exit 1
    fi
    if git ls-remote --exit-code --tags origin "refs/tags/${tag}" >/dev/null 2>&1; then
        echo "error: tag ${tag} already exists on origin." >&2
        exit 1
    fi

    published="$(curl -sf https://api.nuget.org/v3-flatcontainer/fluentblazorrouter/index.json || true)"
    if [[ -n "$published" ]] && grep -qi "\"${version}\"" <<<"$published"; then
        echo "error: ${version} is already published on nuget.org; versions cannot be replaced." >&2
        exit 1
    fi

    just verify "$version"

    echo
    echo "About to tag $(git rev-parse --short HEAD) as ${tag} and push it to origin."
    echo "That starts the release workflow, which publishes ${version} to nuget.org."
    read -r -p "Continue? [y/N] " reply
    if [[ ! "$reply" =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 1
    fi

    git tag -a "${tag}" -m "Release ${version}"
    git push origin "${tag}"

    echo
    echo "Pushed ${tag}. Watch the release at:"
    echo "  https://github.com/The127/FluentBlazorRouter/actions/workflows/release.yml"
