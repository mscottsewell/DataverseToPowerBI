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
    dialogActionsClassNames: function() {
        return dialogActionsClassNames;
    },
    useDialogActionsStyles_unstable: function() {
        return useDialogActionsStyles_unstable;
    }
});
const _react = require("@griffel/react");
const dialogActionsClassNames = {
    root: 'fui-DialogActions'
};
/**
 * Styles for the root slot
 */ const useResetStyles = /*#__PURE__*/ (0, _react.__resetStyles)("rhfpeu0", null, {
    r: [
        ".rhfpeu0{gap:8px;height:fit-content;box-sizing:border-box;display:flex;grid-row-start:3;grid-row-end:3;}"
    ],
    s: [
        "@media screen and (max-width: 480px){.rhfpeu0{flex-direction:column;justify-self:stretch;}}"
    ]
});
const useStyles = /*#__PURE__*/ (0, _react.__styles)({
    gridPositionEnd: {
        Bdqf98w: "f1a7i8kp",
        Br312pm: "fd46tj4",
        Bw0ie65: "fsyjsko",
        Btsd7tp: "f1n00o3b",
        ufxxby: "f1mvsp37",
        Bq5p579: "flbz1vp"
    },
    gridPositionStart: {
        Bdqf98w: "fsxvdwy",
        Br312pm: "fwpfdsa",
        Bw0ie65: "f1e2fz10",
        Ew0qkd: "f119phc2",
        ufxxby: "f1j719yo",
        Bq5p579: "flbz1vp"
    },
    fluidStart: {
        Bw0ie65: "fsyjsko"
    },
    fluidEnd: {
        Br312pm: "fwpfdsa"
    }
}, {
    d: [
        ".f1a7i8kp{justify-self:end;}",
        ".fd46tj4{grid-column-start:2;}",
        ".fsyjsko{grid-column-end:4;}",
        ".fsxvdwy{justify-self:start;}",
        ".fwpfdsa{grid-column-start:1;}",
        ".f1e2fz10{grid-column-end:2;}"
    ],
    m: [
        [
            "@media screen and (max-width: 480px){.f1n00o3b{grid-column-start:1;}}",
            {
                m: "screen and (max-width: 480px)"
            }
        ],
        [
            "@media screen and (max-width: 480px){.f1mvsp37{grid-row-start:4;}}",
            {
                m: "screen and (max-width: 480px)"
            }
        ],
        [
            "@media screen and (max-width: 480px){.flbz1vp{grid-row-end:auto;}}",
            {
                m: "screen and (max-width: 480px)"
            }
        ],
        [
            "@media screen and (max-width: 480px){.f119phc2{grid-column-end:4;}}",
            {
                m: "screen and (max-width: 480px)"
            }
        ],
        [
            "@media screen and (max-width: 480px){.f1j719yo{grid-row-start:3;}}",
            {
                m: "screen and (max-width: 480px)"
            }
        ]
    ]
});
const useDialogActionsStyles_unstable = (state)=>{
    'use no memo';
    const resetStyles = useResetStyles();
    const styles = useStyles();
    state.root.className = (0, _react.mergeClasses)(dialogActionsClassNames.root, resetStyles, state.position === 'start' && styles.gridPositionStart, state.position === 'end' && styles.gridPositionEnd, state.fluid && state.position === 'start' && styles.fluidStart, state.fluid && state.position === 'end' && styles.fluidEnd, state.root.className);
    return state;
};
