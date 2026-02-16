'use client';
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
    dialogSurfaceClassNames: function() {
        return dialogSurfaceClassNames;
    },
    useDialogSurfaceStyles_unstable: function() {
        return useDialogSurfaceStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
const _reacttabster = require("@fluentui/react-tabster");
const _contexts = require("../../contexts");
const dialogSurfaceClassNames = {
    root: 'fui-DialogSurface',
    backdrop: 'fui-DialogSurface__backdrop'
};
/**
 * Styles for the root slot
 */ const useRootBaseStyle = (0, _react.makeResetStyles)({
    ...(0, _reacttabster.createFocusOutlineStyle)(),
    inset: 0,
    padding: _contexts.SURFACE_PADDING,
    margin: 'auto',
    borderStyle: 'none',
    overflow: 'unset',
    border: `${_contexts.SURFACE_BORDER_WIDTH} solid ${_reacttheme.tokens.colorTransparentStroke}`,
    borderRadius: _reacttheme.tokens.borderRadiusXLarge,
    display: 'block',
    userSelect: 'unset',
    visibility: 'unset',
    position: 'fixed',
    height: 'fit-content',
    maxWidth: '600px',
    maxHeight: [
        '100vh',
        '100dvh'
    ],
    boxSizing: 'border-box',
    backgroundColor: _reacttheme.tokens.colorNeutralBackground1,
    color: _reacttheme.tokens.colorNeutralForeground1,
    // Same styles as DialogSurfaceMotion last keyframe,
    // to ensure dialog will be properly styled when surfaceMotion is opted-out
    boxShadow: _reacttheme.tokens.shadow64,
    [_contexts.DIALOG_MEDIA_QUERY_BREAKPOINT_SELECTOR]: {
        maxWidth: '100vw'
    },
    [_contexts.DIALOG_MEDIA_QUERY_SHORT_SCREEN]: {
        overflowY: 'auto',
        // We need to offset the scrollbar by adding transparent borders otherwise
        // it conflicts with the border radius.
        paddingRight: `calc(${_contexts.SURFACE_PADDING} - ${_contexts.DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET})`,
        borderRightWidth: _contexts.DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET,
        borderTopWidth: _contexts.DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET,
        borderBottomWidth: _contexts.DIALOG_FULLSCREEN_DIALOG_SCROLLBAR_OFFSET
    }
});
const useBackdropBaseStyle = (0, _react.makeResetStyles)({
    inset: '0px',
    backgroundColor: _reacttheme.tokens.colorBackgroundOverlay,
    position: 'fixed'
});
const useStyles = (0, _react.makeStyles)({
    nestedDialogBackdrop: {
        backgroundColor: _reacttheme.tokens.colorTransparentBackground
    },
    dialogHidden: {
        pointerEvents: 'none'
    }
});
const useDialogSurfaceStyles_unstable = (state)=>{
    'use no memo';
    const { root, backdrop, open, unmountOnClose, treatBackdropAsNested, backdropAppearance } = state;
    const rootBaseStyle = useRootBaseStyle();
    const backdropBaseStyle = useBackdropBaseStyle();
    const styles = useStyles();
    const isBackdropTransparent = backdropAppearance ? backdropAppearance === 'transparent' : treatBackdropAsNested;
    const mountedAndClosed = !unmountOnClose && !open;
    root.className = (0, _react.mergeClasses)(dialogSurfaceClassNames.root, rootBaseStyle, mountedAndClosed && styles.dialogHidden, root.className);
    if (backdrop) {
        backdrop.className = (0, _react.mergeClasses)(dialogSurfaceClassNames.backdrop, backdropBaseStyle, mountedAndClosed && styles.dialogHidden, isBackdropTransparent && styles.nestedDialogBackdrop, backdrop.className);
    }
    return state;
};
