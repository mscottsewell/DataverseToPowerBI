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
    inlineDrawerClassNames: function() {
        return inlineDrawerClassNames;
    },
    useInlineDrawerStyles_unstable: function() {
        return useInlineDrawerStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
const _useDrawerBaseStylesstyles = require("../../shared/useDrawerBaseStyles.styles");
const inlineDrawerClassNames = {
    root: 'fui-InlineDrawer'
};
const useDrawerResetStyles = /*#__PURE__*/ (0, _react.__resetStyles)("rkjj3x6", null, [
    ".rkjj3x6{overflow:hidden;width:var(--fui-Drawer--size);max-width:100vw;height:auto;max-height:100vh;box-sizing:border-box;display:flex;flex-direction:column;align-items:flex-start;justify-content:flex-start;background-color:var(--colorNeutralBackground1);color:var(--colorNeutralForeground1);position:relative;}"
]);
/**
 * Styles for the root slot
 */ const borderValue = `1px solid ${_reacttheme.tokens.colorNeutralBackground3}`;
const useDrawerRootStyles = /*#__PURE__*/ (0, _react.__styles)({
    separatorStart: {
        h3c5rm: 0,
        vrafjx: 0,
        Bekrc4i: 0,
        u1mtju: [
            "f1cxmi7i",
            "f1j970fk"
        ]
    },
    separatorEnd: {
        zhjwy3: 0,
        wvpqe5: 0,
        ibv6hh: 0,
        B4g9neb: [
            "f1j970fk",
            "f1cxmi7i"
        ]
    },
    separatorBottom: {
        g2u3we: 0,
        icvyot: 0,
        B4j52fo: 0,
        i8vvqc: "f1n3kblk"
    },
    start: {},
    end: {},
    bottom: {
        a9b677: "fly5x3f",
        Bqenvij: "fub80nq"
    },
    animationExitStart: {
        a9b677: "f3tsq5r",
        Bgfg5da: 0,
        B9xav0g: 0,
        oivjwe: 0,
        Bn0qgzm: 0,
        B4g9neb: 0,
        zhjwy3: 0,
        wvpqe5: 0,
        ibv6hh: 0,
        u1mtju: 0,
        h3c5rm: 0,
        vrafjx: 0,
        Bekrc4i: 0,
        i8vvqc: 0,
        g2u3we: 0,
        icvyot: 0,
        B4j52fo: 0,
        irswps: "f3bhgqh"
    },
    animationExitEnd: {
        a9b677: "f3tsq5r",
        Bgfg5da: 0,
        B9xav0g: 0,
        oivjwe: 0,
        Bn0qgzm: 0,
        B4g9neb: 0,
        zhjwy3: 0,
        wvpqe5: 0,
        ibv6hh: 0,
        u1mtju: 0,
        h3c5rm: 0,
        vrafjx: 0,
        Bekrc4i: 0,
        i8vvqc: 0,
        g2u3we: 0,
        icvyot: 0,
        B4j52fo: 0,
        irswps: "f3bhgqh"
    },
    animationExitBottom: {
        Bqenvij: "fniina8",
        Bgfg5da: 0,
        B9xav0g: 0,
        oivjwe: 0,
        Bn0qgzm: 0,
        B4g9neb: 0,
        zhjwy3: 0,
        wvpqe5: 0,
        ibv6hh: 0,
        u1mtju: 0,
        h3c5rm: 0,
        vrafjx: 0,
        Bekrc4i: 0,
        i8vvqc: 0,
        g2u3we: 0,
        icvyot: 0,
        B4j52fo: 0,
        irswps: "f3bhgqh"
    }
}, {
    d: [
        [
            ".f1cxmi7i{border-right:1px solid var(--colorNeutralBackground3);}",
            {
                p: -1
            }
        ],
        [
            ".f1j970fk{border-left:1px solid var(--colorNeutralBackground3);}",
            {
                p: -1
            }
        ],
        [
            ".f1j970fk{border-left:1px solid var(--colorNeutralBackground3);}",
            {
                p: -1
            }
        ],
        [
            ".f1cxmi7i{border-right:1px solid var(--colorNeutralBackground3);}",
            {
                p: -1
            }
        ],
        [
            ".f1n3kblk{border-top:1px solid var(--colorNeutralBackground3);}",
            {
                p: -1
            }
        ],
        ".fly5x3f{width:100%;}",
        ".fub80nq{height:var(--fui-Drawer--size);}",
        ".f3tsq5r{width:0;}",
        [
            ".f3bhgqh{border:none;}",
            {
                p: -2
            }
        ],
        [
            ".f3bhgqh{border:none;}",
            {
                p: -2
            }
        ],
        ".fniina8{height:0;}",
        [
            ".f3bhgqh{border:none;}",
            {
                p: -2
            }
        ]
    ]
});
function getSeparatorClass(state, classNames) {
    if (!state.separator) {
        return undefined;
    }
    switch(state.position){
        case 'start':
            return classNames.separatorStart;
        case 'end':
            return classNames.separatorEnd;
        case 'bottom':
            return classNames.separatorBottom;
        default:
            return undefined;
    }
}
function getAnimationExitClass(state, classNames) {
    switch(state.position){
        case 'start':
            return classNames.animationExitStart;
        case 'end':
            return classNames.animationExitEnd;
        case 'bottom':
            return classNames.animationExitBottom;
        default:
            return undefined;
    }
}
const useInlineDrawerStyles_unstable = (state)=>{
    'use no memo';
    const resetStyles = useDrawerResetStyles();
    const baseClassNames = (0, _useDrawerBaseStylesstyles.useDrawerBaseClassNames)(state);
    const rootStyles = useDrawerRootStyles();
    state.root.className = (0, _react.mergeClasses)(inlineDrawerClassNames.root, resetStyles, baseClassNames, getSeparatorClass(state, rootStyles), rootStyles[state.position], state.animationDirection === 'exit' && getAnimationExitClass(state, rootStyles), state.root.className);
    return state;
};
