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
const useDrawerResetStyles = (0, _react.makeResetStyles)({
    ..._useDrawerBaseStylesstyles.drawerDefaultStyles,
    position: 'relative'
});
/**
 * Styles for the root slot
 */ const borderValue = `1px solid ${_reacttheme.tokens.colorNeutralBackground3}`;
const useDrawerRootStyles = (0, _react.makeStyles)({
    /* Separator */ separatorStart: {
        borderRight: borderValue
    },
    separatorEnd: {
        borderLeft: borderValue
    },
    separatorBottom: {
        borderTop: borderValue
    },
    /* Positioning */ start: {},
    end: {},
    bottom: {
        width: '100%',
        height: `var(${_useDrawerBaseStylesstyles.drawerCSSVars.drawerSizeVar})`
    },
    /* Animation exit states */ animationExitStart: {
        width: '0',
        border: 'none'
    },
    animationExitEnd: {
        width: '0',
        border: 'none'
    },
    animationExitBottom: {
        height: '0',
        border: 'none'
    }
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
