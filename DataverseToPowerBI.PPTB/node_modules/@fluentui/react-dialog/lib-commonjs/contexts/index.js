"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
function _export(target, all) {
    for(var name in all)Object.defineProperty(target, name, {
        enumerable: true,
        get: all[name]
    });
}
_export(exports, {
    DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET: function() {
        return _constants.DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET;
    },
    DIALOG_GAP: function() {
        return _constants.DIALOG_GAP;
    },
    DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR: function() {
        return _constants.DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR;
    },
    DIALOG_MEDIA_QUERY_SHORT_SCREEN: function() {
        return _constants.DIALOG_MEDIA_QUERY_SHORT_SCREEN;
    },
    DialogBackdropContext: function() {
        return _dialogBackdropContext.DialogBackdropContext;
    },
    DialogBackdropProvider: function() {
        return _dialogBackdropContext.DialogBackdropProvider;
    },
    DialogContext: function() {
        return _dialogContext.DialogContext;
    },
    DialogProvider: function() {
        return _dialogContext.DialogProvider;
    },
    DialogSurfaceContext: function() {
        return _dialogSurfaceContext.DialogSurfaceContext;
    },
    DialogSurfaceProvider: function() {
        return _dialogSurfaceContext.DialogSurfaceProvider;
    },
    SURFACE_BORDER_WIDTH: function() {
        return _constants.SURFACE_BORDER_WIDTH;
    },
    SURFACE_PADDING: function() {
        return _constants.SURFACE_PADDING;
    },
    useDialogBackdropContext_unstable: function() {
        return _dialogBackdropContext.useDialogBackdropContext_unstable;
    },
    useDialogContext_unstable: function() {
        return _dialogContext.useDialogContext_unstable;
    },
    useDialogSurfaceContext_unstable: function() {
        return _dialogSurfaceContext.useDialogSurfaceContext_unstable;
    }
});
const _constants = require("./constants");
const _dialogContext = require("./dialogContext");
const _dialogSurfaceContext = require("./dialogSurfaceContext");
const _dialogBackdropContext = require("./dialogBackdropContext");
