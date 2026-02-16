'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useOverlayDrawerSurfaceStyles_unstable", {
    enumerable: true,
    get: function() {
        return useOverlayDrawerSurfaceStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
/**
 * Styles for the backdrop slot
 */ const useBackdropResetStyles = (0, _react.makeResetStyles)({
    inset: '0px',
    position: 'fixed',
    backgroundColor: _reacttheme.tokens.colorBackgroundOverlay
});
const useBackdropStyles = (0, _react.makeStyles)({
    nested: {
        backgroundColor: _reacttheme.tokens.colorTransparentBackground
    },
    drawerHidden: {
        pointerEvents: 'none'
    }
});
const useOverlayDrawerSurfaceStyles_unstable = (state)=>{
    'use no memo';
    const { treatBackdropAsNested, backdrop, open, unmountOnClose } = state;
    const backdropResetStyles = useBackdropResetStyles();
    const backdropStyles = useBackdropStyles();
    const mountedAndClosed = !unmountOnClose && !open;
    if (backdrop) {
        backdrop.className = (0, _react.mergeClasses)(backdropResetStyles, treatBackdropAsNested && backdropStyles.nested, mountedAndClosed && backdropStyles.drawerHidden, backdrop.className);
    }
    return state;
};
