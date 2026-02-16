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
const _CarouselContext = require("../CarouselContext");
const carouselSliderClassNames = {
    root: 'fui-CarouselSlider'
};
/**
 * Styles for the root slot
 */ const useStyles = /*#__PURE__*/ (0, _react.__styles)({
    root: {
        mc9l5x: "f22iagw",
        Eiaeu8: "f1115ve7"
    },
    elevated: {
        i8kkvl: 0,
        Belr9w4: 0,
        rmohyg: "f1vkj2z1"
    }
}, {
    d: [
        ".f22iagw{display:flex;}",
        ".f1115ve7{overflow-anchor:none;}",
        [
            ".f1vkj2z1{gap:var(--spacingHorizontalXXL);}",
            {
                p: -1
            }
        ]
    ]
});
const useCarouselSliderStyles_unstable = (state)=>{
    'use no memo';
    const appearance = (0, _CarouselContext.useCarouselContext_unstable)((context)=>context.appearance);
    const styles = useStyles();
    state.root.className = (0, _react.mergeClasses)(carouselSliderClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, state.root.className);
    return state;
};
