export function read(key) {
    try {
        return globalThis.localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function write(key, value) {
    try {
        globalThis.localStorage.setItem(key, value);
        return true;
    } catch {
        return false;
    }
}

export function hideSplash() {
    document.querySelector('.avalonia-splash')?.remove();

    // Avalonia's WebAssembly renderer exposes the app through a canvas. Preserve
    // the chart's purpose and its primary interaction hints in the browser AX tree.
    document.querySelector('canvas')?.setAttribute(
        'aria-label',
        'Astrarium all-sky horizon chart. Select a visible object to inspect it; after selection, use the arrow keys to move between nearby objects. Drag to pan, scroll to zoom, and press Home to reset.'
    );
}
