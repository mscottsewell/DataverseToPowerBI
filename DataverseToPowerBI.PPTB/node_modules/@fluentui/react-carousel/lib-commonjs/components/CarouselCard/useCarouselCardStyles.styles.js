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
    carouselCardClassNames: function() {
        return carouselCardClassNames;
    },
    useCarouselCardStyles_unstable: function() {
        return useCarouselCardStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _CarouselContext = require("../CarouselContext");
const carouselCardClassNames = {
    root: 'fui-CarouselCard'
};
/**
 * Styles for the root slot
 */ const useStyles = /*#__PURE__*/ (0, _react.__styles)({
    root: {
        xawz: 0,
        Bh6795r: 0,
        Bnnss6s: 0,
        fkmc3a: "fg68ejw",
        B2u0y6b: "f6dzj5z"
    },
    autoSize: {
        xawz: 0,
        Bh6795r: 0,
        Bnnss6s: 0,
        fkmc3a: "fd9q35j",
        Bf4jedk: "fy77jfu",
        a9b677: "f14z66ap",
        B2u0y6b: "f6dzj5z"
    },
    elevated: {
        Beyfa6y: 0,
        Bbmb7ep: 0,
        Btl43ni: 0,
        B7oj6ja: 0,
        Dimara: "f1kijzfu",
        E5pizo: "f1hg901r",
        B68tc82: 0,
        Bmxbyg5: 0,
        Bpg54ce: "f1a3p1vp"
    }
}, {
    d: [
        [
            ".fg68ejw{flex:0 0 100%;}",
            {
                p: -1
            }
        ],
        ".f6dzj5z{max-width:100%;}",
        [
            ".fd9q35j{flex:0 0 auto;}",
            {
                p: -1
            }
        ],
        ".fy77jfu{min-width:0;}",
        ".f14z66ap{width:auto;}",
        [
            ".f1kijzfu{border-radius:var(--borderRadiusXLarge);}",
            {
                p: -1
            }
        ],
        ".f1hg901r{box-shadow:var(--shadow16);}",
        [
            ".f1a3p1vp{overflow:hidden;}",
            {
                p: -1
            }
        ]
    ]
});
const useCarouselCardStyles_unstable = (state)=>{
    'use no memo';
    const { autoSize } = state;
    const appearance = (0, _CarouselContext.useCarouselContext_unstable)((context)=>context.appearance);
    const styles = useStyles();
    state.root.className = (0, _react.mergeClasses)(carouselCardClassNames.root, styles.root, appearance === 'elevated' && styles.elevated, autoSize && styles.autoSize, state.root.className);
    return state;
};
