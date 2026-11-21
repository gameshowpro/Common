// Per-viewer remembered state in localStorage (used by GspCollapsibleSection). Every access is wrapped: storage can be
// unavailable or throw (private window, blocked site data), and the caller then simply uses its default.
export function get(key) {
    try {
        return window.localStorage.getItem(key);
    } catch (e) {
        return null;
    }
}

export function set(key, value) {
    try {
        window.localStorage.setItem(key, value);
    } catch (e) {
        // Not remembered; nothing else to do.
    }
}
