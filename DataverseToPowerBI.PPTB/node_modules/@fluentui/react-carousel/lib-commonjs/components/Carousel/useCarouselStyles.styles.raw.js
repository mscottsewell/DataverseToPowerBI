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
    carouselClassNames: function() {
        return carouselClassNames;
    },
    useCarouselStyles_unstable: function() {
        return useCarouselStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
const carouselClassNames = {
    root: 'fui-Carousel'
};
/**
 * Styles for the root slot
 */ const useStyles = (0, _react.makeStyles)({
    root: {
        // Only hide horizontal overflow to enable focus border to bleed bounds vertically
        overflowX: 'hidden',
        overflowAnchor: 'none',
        position: 'relative'
    },
    elevated: {
        // Reserve vertical space so the drop shadow has room to render
        // without being clipped by surrounding layout or overflow rules.
        margin: _reacttheme.tokens.spacingVerticalL,
        padding: _reacttheme.tokens.spacingVerticalL
    }
});
const useCarouselStyles_unstable = (state)=>{
    'use no memo';
    const styles = useStyles();
    const { appearance } = state;
    state.root.className = (0, _react.mergeClasses)(carouselClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
    return state;
};
