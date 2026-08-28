# Releasing

Releases are cut from a git tag and published by GitHub Actions. Nothing is packed or
pushed from a developer machine.

```bash
just release 1.2.3
```

That validates the version, checks the tag and version are not already taken, packs and
verifies the package locally, asks for confirmation, then tags and pushes. Pushing the tag
triggers [`.github/workflows/release.yml`](.github/workflows/release.yml), which packs,
verifies, publishes to nuget.org and opens a GitHub Release.

To release by hand instead, push a tag of the form `vMAJOR.MINOR.PATCH`:

```bash
git tag -a v1.2.3 -m "Release 1.2.3" && git push origin v1.2.3
```

A tag with a prerelease suffix (`v1.3.0-beta.1`) publishes a prerelease and marks the
GitHub Release accordingly.

## One-time setup

The workflow authenticates to nuget.org with [trusted publishing][tp] — a short-lived key
issued per run, so there is no long-lived API key stored in the repository.

[tp]: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing

1. **Register the trusted publishing policy on nuget.org.** Sign in, click your username,
   choose **Trusted Publishing**, and add a policy:

   | Field            | Value                     |
   | ---------------- | ------------------------- |
   | Repository Owner | `The127`                  |
   | Repository       | `FluentBlazorRouter`      |
   | Workflow File    | `release.yml`             |
   | Environment      | *(leave empty)*           |

   Enter the workflow **file name only**, not the `.github/workflows/` path. Renaming
   `release.yml` breaks the policy, so update it there if the file ever moves.

2. **Add the `NUGET_USER` repository secret** (Settings → Secrets and variables → Actions)
   containing your nuget.org **username / profile name** — not your email address.

A new policy on some accounts starts out *temporarily active for 7 days*. If no publish
happens in that window it goes inactive; you can restart the window from the same page.

## Versioning

The git tag is the only source of the version. `FluentBlazorRouter.csproj` carries
`<Version>0.0.0-dev</Version>` purely so local builds are obviously not releases, and the
release workflow overrides it with `-p:Version=<tag>`.

**Do not add a `<PackageVersion>` property.** It overrides `-p:Version`, which would pin
every release to one hardcoded number regardless of the tag.

This matters because of [issue #3][i3]: 1.2.2 was published with `<PackageVersion>1.2.2</PackageVersion>`
but `<Version>1.0.0</Version>`, so the package version moved while the *assembly* version
stayed `1.0.0.0` across every release since 1.0.0. NuGet clients that already had 1.2.1
kept resolving their cached binaries, and the fix never reached anyone who upgraded.
Setting `Version` alone keeps the assembly, file, informational and package versions moving
together.

[i3]: https://github.com/The127/FluentBlazorRouter/issues/3

Three checks now make that failure impossible to ship:

- **On every PR** (`ci.yml`) — builds with a sentinel version and fails if the evaluated
  `PackageVersion` does not follow it, catching a pin reintroduced anywhere.
- **Locally** (`just check`) — the same probe, before you push.
- **At release time** (`release.yml`) — unpacks the built `.nupkg` and asserts both the
  nuspec version *and* the compiled assembly version match the tag, so a mismatched package
  fails the release instead of reaching nuget.org.

## Other recipes

```
just build     # build the solution in Release
just check     # everything CI checks, run locally
just pack 1.2.3  # build a package without releasing it
just verify 1.2.3  # pack, then assert package and assembly versions match
just clean
```
