import { readFileSync } from 'node:fs';

import { declaredVersions, releaseConfig } from '@rimworks/mod-ci';

// semantic-release-steam only updates an existing item, so this id needs a manual first upload.
const WORKSHOP_ID = process.env.WORKSHOP_ID || '3793646067';

/** @type {import('semantic-release').GlobalConfig} */
export default releaseConfig({
    solution: 'Quickstarts.slnx',
    versions: declaredVersions(readFileSync('loadFolders.xml', 'utf8')),
    mods: [{ name: 'Quickstarts', workshopId: WORKSHOP_ID }],
    pack: 'Source/Quickstarts.Ref/Quickstarts.Ref.csproj',
    nupkgGlob: 'artifacts/RimWorks.Quickstarts.Ref.${nextRelease.version}.nupkg',
    assets: [
        { path: 'dist/Quickstarts-*.zip', label: 'Quickstarts mod' },
        { path: 'artifacts/RimWorks.Quickstarts.Ref.*.nupkg', label: 'Reference package' },
    ],
});
