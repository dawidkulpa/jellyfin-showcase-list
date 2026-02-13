# Jellyfin Showcase List Plugin

Maintains a `list.txt` file (one Jellyfin item ID per line) for a web mainpage showcase slider.

## Behavior

- Watches the Jellyfin library.
- Computes **Top 10** items across **Movies + Series**.
- Sort key: `CommunityRating` (0–10) with fallback to normalized `CriticRating/10`.
- Items without ratings are kept with score `0` so the list always fills (up to 10).
- Writes IDs to the configured output path (default: `/jellyfin/jellyfin-web/list.txt`).
- Debounces library storms (default 30 seconds).

## Admin UI

In Dashboard → Plugins → Showcase List:
- Configure output path + debounce
- **Refresh now** button
- Preview of current computed Top 10

## Build

On a machine with .NET SDK installed:

```bash
cd Jellyfin.Plugin.ShowcaseList
# ensure package versions match your server (10.11.6)
dotnet restore
dotnet publish -c Release -o ../dist
```

Copy the resulting DLLs from `dist/` into your Jellyfin plugins directory.

## Notes

This repo is a scaffold. Jellyfin's internal API surface can vary between releases; if you see compile errors, we should adjust types/namespaces to match 10.11.6 exactly.
