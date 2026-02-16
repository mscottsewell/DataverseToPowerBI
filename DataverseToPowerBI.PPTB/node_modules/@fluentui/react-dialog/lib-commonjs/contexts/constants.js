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
        return DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET;
    },
    DIALOG_GAP: function() {
        return DIALOG_GAP;
    },
    DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR: function() {
        return DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR;
    },
    DIALOG_MEDIA_QUERY_SHORT_SCREEN: function() {
        return DIALOG_MEDIA_QUERY_SHORT_SCREEN;
    },
    SURFACE_BORDER_WIDTH: function() {
        return SURFACE_BORDER_WIDTH;
    },
    SURFACE_PADDING: function() {
        return SURFACE_PADDING;
    }
});
const DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR = '@media screen and (max-width: 480px)';
const DIALOG_MEDIA_QUERY_SHORT_SCREEN = '@media screen and (max-height: 359px)';
const SURFACE_PADDING = '24px';
const DIALOG_GAP = '8px';
const SURFACE_BORDER_WIDTH = '1px';
const DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET = '4px';
