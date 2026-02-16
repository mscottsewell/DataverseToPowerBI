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
    hamburgerClassNames: function() {
        return hamburgerClassNames;
    },
    useHamburgerStyles_unstable: function() {
        return useHamburgerStyles_unstable;
    }
});
const _react = require("@griffel/react");
const _reactbutton = require("@fluentui/react-button");
const hamburgerClassNames = {
    root: 'fui-Hamburger',
    icon: 'fui-Hamburger__icon'
};
/**
 * Styles for the root slot
 */ const useStyles = /*#__PURE__*/ (0, _react.__styles)({
    root: {
        w71qe1: "f1iuv45f",
        De3pzq: "f1ctqxl6",
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
        irswps: "f3bhgqh",
        Jwef8y: "f11oyicx",
        ecr2s2: "f9fof1w"
    }
}, {
    d: [
        ".f1iuv45f{text-decoration-line:none;}",
        ".f1ctqxl6{background-color:var(--colorNeutralBackground4);}",
        [
            ".f3bhgqh{border:none;}",
            {
                p: -2
            }
        ]
    ],
    h: [
        ".f11oyicx:hover{background-color:var(--colorNeutralBackground4Hover);}"
    ],
    a: [
        ".f9fof1w:active{background-color:var(--colorNeutralBackground4Pressed);}"
    ]
});
const useHamburgerStyles_unstable = (state)=>{
    'use no memo';
    (0, _reactbutton.useButtonStyles_unstable)(state);
    const styles = useStyles();
    state.root.className = (0, _react.mergeClasses)(hamburgerClassNames.root, styles.root, state.root.className);
    if (state.icon) {
        state.icon.className = (0, _react.mergeClasses)(hamburgerClassNames.icon, state.icon.className);
    }
    return state;
};
