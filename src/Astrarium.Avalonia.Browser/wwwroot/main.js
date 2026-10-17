import { dotnet } from './_framework/dotnet.js';

if (typeof window === 'undefined') {
    throw new Error('Astrarium Browser requires a web browser.');
}

const runtime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = runtime.getConfig();
await runtime.runMain(config.mainAssemblyName, [globalThis.location.href]);
