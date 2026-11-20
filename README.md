# resolved.ce
Offline BOJ/solved.ac reader  
(Developing yet; Not all features are implemented)
<img src='./banner.png'/>

## Available
- Search problems
- Download problems
- Sorting problems/users
- Light/Dark theme and Acrylic/Mica backdrop
- Bookmark

## Unstable
- Download user
- Sometime wrong ac/wa coloring

## No working or implement
- Download BOJ website for offline
- Level tab
- Sprout tab
- Class tab
- Tag tab

## Required
- Windows 10 or higher
- .NET 8 Runtime (or SDK if you want to build)
- solved.ac API v4 application credentials (client ID and secret)

## solved.ac API key
Since acNET 1.0, resolved.ce uses the official solved.ac API v4, which needs an application client ID and secret for every request.
1. Register an application at [solved.ac Backstage](https://backstage.solved.ac/developer).
2. Open **Settings > BOJ/solved.ac data management > solved.ac API key** and enter the client ID and secret.

The credentials are saved only in the local `config.json`. For development, the `SOLVEDAC_CLIENT_ID` and `SOLVEDAC_CLIENT_SECRET` environment variables are used when the settings are empty.

## Dependency library
- [WinUI 3](https://github.com/microsoft/microsoft-ui-xaml)
- [acNET](https://github.com/614project/acNET)

## Naming
First considered name is 'unsolved.wa' but it's already used by other project.  
So, I changed it to 'resolved.ce'.  
