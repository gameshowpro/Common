// Opens a native <dialog> modally and closes it when the dimmed backdrop is clicked. Escape, focus return and the inert page
// behind come from the browser. Used by GspAboutDialog.
export function showModal(dialog) {
    if (!dialog) {
        return;
    }
    if (!dialog.dataset.gspBackdrop) {
        dialog.dataset.gspBackdrop = "1";
        // A click whose target is the dialog element itself landed on the backdrop: the dialog's content fills the rest.
        dialog.addEventListener("click", e => {
            if (e.target === dialog) {
                dialog.close();
            }
        });
    }
    if (!dialog.open) {
        dialog.showModal();
    }
}
