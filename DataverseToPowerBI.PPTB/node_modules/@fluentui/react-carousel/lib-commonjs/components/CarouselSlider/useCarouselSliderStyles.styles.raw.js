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
    carouselSliderClassNames: function() {
        return carouselSliderClassNames;
    },
    useCarouselSliderStyles_unstable: function() {
        return useCarouselSliderStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
const _CarouselContext = require("../CarouselContext");
const carouselSliderClassNames = {
    root: 'fui-CarouselSlider'
};
/**
 * Styles for the root slot
 */ const useStyles = (0, _react.makeStyles)({
    root: {
        display: 'flex',
        overflowAnchor: 'none'
    },
    elevated: {
        gap: _reacttheme.tokens.spacingHorizontalXXL
    }
});
const useCarouselSliderStyles_unstable = (state)=>{
    'use no memo';
    const appearance = (0, _CarouselContext.useCarouselContext_unstable)((context)=>context.appearance);
    const styles = useStyles();
    state.root.className = (0, _react.mergeClasses)(carouselSliderClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
    return state;
};
