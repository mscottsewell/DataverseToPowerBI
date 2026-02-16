"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useDialogContextValues_unstable", {
    enumerable: true,
    get: function() {
        return useDialogContextValues_unstable;
    }
});
function useDialogContextValues_unstable(state) {
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
