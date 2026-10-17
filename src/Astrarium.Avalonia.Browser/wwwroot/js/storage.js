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
}
