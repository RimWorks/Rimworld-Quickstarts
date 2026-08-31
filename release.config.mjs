// semantic-release-steam updates an existing item and never creates one, so this id has
// to come from a first manual upload. Unset means the Steam step is skipped entirely.
const WORKSHOP_ID = process.env.QUICKSTARTS_WORKSHOP_ID ?? '';

/** @type {import('semantic-release').GlobalConfig} */
export default {
    branches: ['main'],
    plugins: [
        [
            '@semantic-release/commit-analyzer',
            {
                releaseRules: [
                    { type: 'refactor', release: 'patch' },
                    { type: 'style', release: 'patch' },
                    { type: 'ci', release: 'patch' },
                ],
            },
        ],
        '@semantic-release/release-notes-generator',
        [
            '@semantic-release/exec',
            {
                // Harmony/ and Concord/ hold the patching backends loadFolders.xml picks
                // between; a zip without them installs a mod that cannot patch anything.
                prepareCmd: [
                    'dotnet build Quickstarts.slnx -c Release -p:Version=${nextRelease.version}',
                    'dotnet pack Source/Quickstarts.Ref/Quickstarts.Ref.csproj -c Release -p:Version=${nextRelease.version} -o artifacts',
                    'zip -r Quickstarts-${nextRelease.version}.zip About Assemblies Harmony Concord Languages loadFolders.xml -x "*.pdb" "About/Preview.xcf"',
                ].join(' && '),

                // The workflow gets NUGET_API_KEY from trusted publishing. Skipped when it
                // is absent, so a local dry run does not try to push.
                publishCmd:
                    'if [ -n "$NUGET_API_KEY" ]; then dotnet nuget push "artifacts/RimWorks.Quickstarts.Ref.${nextRelease.version}.nupkg" --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json --skip-duplicate; else echo "no NUGET_API_KEY, skipping nuget push"; fi',
            },
        ],
        ...(WORKSHOP_ID
            ? [
                  [
                      'semantic-release-steam',
                      {
                          appId: '294100',
                          branchTargets: { main: 'stable' },
                          mods: [
                              {
                                  name: 'Quickstarts',
                                  path: '.',
                                  workshopIds: { stable: WORKSHOP_ID },
                              },
                          ],
                      },
                  ],
              ]
            : []),
        [
            '@semantic-release/github',
            {
                assets: [
                    { path: 'Quickstarts-*.zip', label: 'Quickstarts mod' },
                    { path: 'artifacts/RimWorks.Quickstarts.Ref.*.nupkg', label: 'Reference package' },
                ],
            },
        ],
    ],
};
