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
/**
 * Styles for the backdrop slot
 */ const useBackdropResetStyles = /*#__PURE__*/ (0, _react.__resetStyles)("rl76ifk", null, [
    ".rl76ifk{inset:0px;position:fixed;background-color:var(--colorBackgroundOverlay);}"
]);
const useBackdropStyles = /*#__PURE__*/ (0, _react.__styles)({
    nested: {
        De3pzq: "f1c21dwh"
    },
    drawerHidden: {
        Bkecrkj: "f1aehjj5"
    }
}, {
    d: [
        ".f1c21dwh{background-color:var(--colorTransparentBackground);}",
        ".f1aehjj5{pointer-events:none;}"
    ]
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
