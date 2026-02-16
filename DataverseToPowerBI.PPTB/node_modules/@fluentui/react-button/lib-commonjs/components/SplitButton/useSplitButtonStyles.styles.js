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
    splitButtonClassNames: function() {
        return splitButtonClassNames;
    },
    useSplitButtonStyles_unstable: function() {
        return useSplitButtonStyles_unstable;
    }
});
const _react = require("@griffel/react");
const splitButtonClassNames = {
    root: 'fui-SplitButton',
    menuButton: 'fui-SplitButton__menuButton',
    primaryActionButton: 'fui-SplitButton__primaryActionButton'
};
// WCAG minimum target size for pointer targets that are immediately adjacent to other targets:
// https://w3c.github.io/wcag/guidelines/22/#target-size-minimum
const MIN_TARGET_SIZE = '24px';
const useFocusStyles = /*#__PURE__*/ (0, _react.__styles)({
    primaryActionButton: {
        B6xbmo0: [
            "f1x37qnr",
            "f1um7c6d"
        ],
        kdpuga: [
            "fn4c73s",
            "f6pwzcr"
        ]
    },
    menuButton: {
        lbo84a: [
            "frrbwxo",
            "f1rgcpbv"
        ],
        dm238s: [
            "f1um7c6d",
            "f1x37qnr"
        ],
        Bw81rd7: [
            "f6pwzcr",
            "fn4c73s"
        ]
    }
}, {
    d: [
        ".f1x37qnr[data-fui-focus-visible]{border-top-right-radius:0;}",
        ".f1um7c6d[data-fui-focus-visible]{border-top-left-radius:0;}",
        ".fn4c73s[data-fui-focus-visible]{border-bottom-right-radius:0;}",
        ".f6pwzcr[data-fui-focus-visible]{border-bottom-left-radius:0;}",
        ".frrbwxo[data-fui-focus-visible]{border-left-width:0;}",
        ".f1rgcpbv[data-fui-focus-visible]{border-right-width:0;}"
    ]
});
const useRootStyles = /*#__PURE__*/ (0, _react.__styles)({
    base: {
        mc9l5x: "ftuwxu6",
        Brf1p80: "fsxf2b5",
        qhf8xq: "f10pi13n",
        ha4doy: "fmrv4ls",
        kn2xc0: [
            "f14uur2j",
            "fc1btbj"
        ],
        Bs76p8a: [
            "fye5tvs",
            "fc597qq"
        ],
        cuxpm9: [
            "f1e8brtx",
            "fr36rk3"
        ],
        Biffepf: [
            "fxp12j1",
            "f1m6nt2y"
        ],
        Defnvf: [
            "fr7y8no",
            "f1dn0c6m"
        ],
        z0pv9t: "f1b65x5h"
    },
    outline: {},
    primary: {
        B1l9wao: [
            "f4rm5b0",
            "f1tuwo13"
        ],
        lcnrd8: [
            "fdwdeeo",
            "f1ezdslh"
        ],
        vlshuh: [
            "f1ju2vgk",
            "f1r6p88l"
        ],
        B3s9tpx: [
            "f2z0mmn",
            "f12iqdwp"
        ],
        rfylfo: [
            "f1btorfl",
            "fxofj4p"
        ],
        k5lds2: [
            "f161sdhp",
            "f10m3pjc"
        ]
    },
    secondary: {},
    subtle: {
        B1l9wao: [
            "f16kf41h",
            "fxiafvi"
        ],
        lcnrd8: [
            "ffl6mx9",
            "f1t5sw6t"
        ],
        vlshuh: [
            "f1mww3cx",
            "f1wtv0vd"
        ]
    },
    transparent: {
        B1l9wao: [
            "f16kf41h",
            "fxiafvi"
        ],
        lcnrd8: [
            "ffl6mx9",
            "f1t5sw6t"
        ],
        vlshuh: [
            "f1mww3cx",
            "f1wtv0vd"
        ]
    },
    circular: {},
    rounded: {},
    square: {},
    disabled: {
        B1l9wao: [
            "f10xrnr8",
            "f15nylwb"
        ],
        lcnrd8: [
            "f11fwhjz",
            "f18vtcsx"
        ],
        vlshuh: [
            "f12kx9re",
            "f1hyxdqk"
        ]
    },
    disabledHighContrast: {
        B3s9tpx: [
            "fslo9ob",
            "ftovg2z"
        ],
        rfylfo: [
            "f1fuq5cn",
            "f168zpha"
        ],
        k5lds2: [
            "f1t6bo0o",
            "f1dnwhli"
        ]
    }
}, {
    d: [
        ".ftuwxu6{display:inline-flex;}",
        ".fsxf2b5{justify-content:stretch;}",
        ".f10pi13n{position:relative;}",
        ".fmrv4ls{vertical-align:middle;}",
        ".f14uur2j .fui-SplitButton__primaryActionButton{border-top-right-radius:0;}",
        ".fc1btbj .fui-SplitButton__primaryActionButton{border-top-left-radius:0;}",
        ".fye5tvs .fui-SplitButton__primaryActionButton{border-bottom-right-radius:0;}",
        ".fc597qq .fui-SplitButton__primaryActionButton{border-bottom-left-radius:0;}",
        ".f1e8brtx .fui-SplitButton__menuButton{border-left-width:0;}",
        ".fr36rk3 .fui-SplitButton__menuButton{border-right-width:0;}",
        ".fxp12j1 .fui-SplitButton__menuButton{border-top-left-radius:0;}",
        ".f1m6nt2y .fui-SplitButton__menuButton{border-top-right-radius:0;}",
        ".fr7y8no .fui-SplitButton__menuButton{border-bottom-left-radius:0;}",
        ".f1dn0c6m .fui-SplitButton__menuButton{border-bottom-right-radius:0;}",
        ".f1b65x5h .fui-SplitButton__menuButton{min-width:24px;}",
        ".f4rm5b0 .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeOnBrand);}",
        ".f1tuwo13 .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeOnBrand);}",
        ".f16kf41h .fui-SplitButton__primaryActionButton{border-right-color:var(--colorTransparentBackground);}",
        ".fxiafvi .fui-SplitButton__primaryActionButton{border-left-color:var(--colorTransparentBackground);}",
        ".f10xrnr8 .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeDisabled);}",
        ".f15nylwb .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeDisabled);}"
    ],
    h: [
        ".fdwdeeo:hover .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeOnBrand);}",
        ".f1ezdslh:hover .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeOnBrand);}",
        ".f1ju2vgk:hover:active .fui-SplitButton__primaryActionButton,.f1ju2vgk:active:focus-visible .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeOnBrand);}",
        ".f1r6p88l:hover:active .fui-SplitButton__primaryActionButton,.f1r6p88l:active:focus-visible .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeOnBrand);}",
        ".ffl6mx9:hover .fui-SplitButton__primaryActionButton{border-right-color:var(--colorTransparentBackgroundHover);}",
        ".f1t5sw6t:hover .fui-SplitButton__primaryActionButton{border-left-color:var(--colorTransparentBackgroundHover);}",
        ".f1mww3cx:hover:active .fui-SplitButton__primaryActionButton,.f1mww3cx:active:focus-visible .fui-SplitButton__primaryActionButton{border-right-color:var(--colorTransparentBackgroundPressed);}",
        ".f1wtv0vd:hover:active .fui-SplitButton__primaryActionButton,.f1wtv0vd:active:focus-visible .fui-SplitButton__primaryActionButton{border-left-color:var(--colorTransparentBackgroundPressed);}",
        ".f11fwhjz:hover .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeDisabled);}",
        ".f18vtcsx:hover .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeDisabled);}",
        ".f12kx9re:hover:active .fui-SplitButton__primaryActionButton,.f12kx9re:active:focus-visible .fui-SplitButton__primaryActionButton{border-right-color:var(--colorNeutralStrokeDisabled);}",
        ".f1hyxdqk:hover:active .fui-SplitButton__primaryActionButton,.f1hyxdqk:active:focus-visible .fui-SplitButton__primaryActionButton{border-left-color:var(--colorNeutralStrokeDisabled);}"
    ],
    m: [
        [
            "@media (forced-colors: active){.f12iqdwp .fui-SplitButton__primaryActionButton{border-left-color:HighlightText;}.f2z0mmn .fui-SplitButton__primaryActionButton{border-right-color:HighlightText;}}",
            {
                m: "(forced-colors: active)"
            }
        ],
        [
            "@media (forced-colors: active){.f1btorfl:hover .fui-SplitButton__primaryActionButton{border-right-color:Highlight;}.fxofj4p:hover .fui-SplitButton__primaryActionButton{border-left-color:Highlight;}}",
            {
                m: "(forced-colors: active)"
            }
        ],
        [
            "@media (forced-colors: active){.f10m3pjc:hover:active .fui-SplitButton__primaryActionButton,.f10m3pjc:active:focus-visible .fui-SplitButton__primaryActionButton{border-left-color:Highlight;}.f161sdhp:hover:active .fui-SplitButton__primaryActionButton,.f161sdhp:active:focus-visible .fui-SplitButton__primaryActionButton{border-right-color:Highlight;}}",
            {
                m: "(forced-colors: active)"
            }
        ],
        [
            "@media (forced-colors: active){.fslo9ob .fui-SplitButton__primaryActionButton{border-right-color:GrayText;}.ftovg2z .fui-SplitButton__primaryActionButton{border-left-color:GrayText;}}",
            {
                m: "(forced-colors: active)"
            }
        ],
        [
            "@media (forced-colors: active){.f168zpha:hover .fui-SplitButton__primaryActionButton{border-left-color:GrayText;}.f1fuq5cn:hover .fui-SplitButton__primaryActionButton{border-right-color:GrayText;}}",
            {
                m: "(forced-colors: active)"
            }
        ],
        [
            "@media (forced-colors: active){.f1dnwhli:hover:active .fui-SplitButton__primaryActionButton,.f1dnwhli:active:focus-visible .fui-SplitButton__primaryActionButton{border-left-color:GrayText;}.f1t6bo0o:hover:active .fui-SplitButton__primaryActionButton,.f1t6bo0o:active:focus-visible .fui-SplitButton__primaryActionButton{border-right-color:GrayText;}}",
            {
                m: "(forced-colors: active)"
            }
        ]
    ]
});
const useSplitButtonStyles_unstable = (state)=>{
    'use no memo';
    const rootStyles = useRootStyles();
    const focusStyles = useFocusStyles();
    const { appearance, disabled, disabledFocusable } = state;
    state.root.className = (0, _react.mergeClasses)(splitButtonClassNames.root, rootStyles.base, appearance && rootStyles[appearance], (disabled || disabledFocusable) && rootStyles.disabled, (disabled || disabledFocusable) && rootStyles.disabledHighContrast, state.root.className);
    if (state.menuButton) {
        state.menuButton.className = (0, _react.mergeClasses)(splitButtonClassNames.menuButton, focusStyles.menuButton, state.menuButton.className);
    }
    if (state.primaryActionButton) {
        state.primaryActionButton.className = (0, _react.mergeClasses)(splitButtonClassNames.primaryActionButton, focusStyles.primaryActionButton, state.primaryActionButton.className);
    }
    return state;
};
