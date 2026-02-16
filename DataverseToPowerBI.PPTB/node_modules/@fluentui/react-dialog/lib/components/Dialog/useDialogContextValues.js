export function useDialogContextValues_unstable(state) {
    const { modalType, open, dialogRef, dialogTitleId, isNestedDialog, inertTrapFocus, requestOpenChange, modalAttributes, triggerAttributes, unmountOnClose } = state;
    /**
   * This context is created with "@fluentui/react-context-selector",
   * there is no sense to memoize it
   */ const dialog = {
        open,
        modalType,
        dialogRef,
        dialogTitleId,
        isNestedDialog,
        inertTrapFocus,
        modalAttributes,
        triggerAttributes,
        unmountOnClose,
        requestOpenChange
    };
    const dialogSurface = false;
    return {
        dialog,
        dialogSurface
    };
}
