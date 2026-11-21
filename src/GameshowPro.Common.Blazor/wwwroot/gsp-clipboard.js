// Copies text to the clipboard (used by GspClipboard). navigator.clipboard exists only in secure contexts (HTTPS or
// localhost), and an app is often served over plain HTTP on the LAN, so this falls back to a hidden textarea and
// document.execCommand('copy'). Resolves true when the text was copied, false otherwise; never throws.
export async function copy(text) {
    try {
        if (window.isSecureContext && navigator.clipboard && navigator.clipboard.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch (e) {
        // Permission denied or blocked: try the fallback.
    }

    let area = null;
    try {
        area = document.createElement('textarea');
        area.value = text;
        area.setAttribute('readonly', '');
        area.setAttribute('aria-hidden', 'true');
        area.style.position = 'fixed';
        area.style.top = '0';
        area.style.left = '-9999px';
        area.style.opacity = '0';
        document.body.appendChild(area);
        area.focus();
        area.select();
        area.setSelectionRange(0, text.length);
        return document.execCommand('copy') === true;
    } catch (e) {
        return false;
    } finally {
        if (area && area.parentNode) {
            area.parentNode.removeChild(area);
        }
    }
}
