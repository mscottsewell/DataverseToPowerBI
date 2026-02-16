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
    alertClassNames: function() {
        return alertClassNames;
    },
    useAlertStyles_unstable: function() {
        return useAlertStyles_unstable;
    }
});
const _reacttheme = require("@fluentui/react-theme");
const _react = require("@griffel/react");
const _reacttabster = require("@fluentui/react-tabster");
const alertClassNames = {
    root: 'fui-Alert',
    icon: 'fui-Alert__icon',
    action: 'fui-Alert__action',
    avatar: 'fui-Alert__avatar'
};
const useStyles = (0, _react.makeStyles)({
    root: {
        display: 'flex',
        alignItems: 'center',
        minHeight: '44px',
        padding: '0 12px',
        borderRadius: '4px',
        border: `1px solid ${_reacttheme.tokens.colorTransparentStroke}`,
        boxShadow: _reacttheme.tokens.shadow8,
        fontSize: _reacttheme.tokens.fontSizeBase300,
        fontWeight: _reacttheme.tokens.fontWeightSemibold,
        color: _reacttheme.tokens.colorNeutralForeground1,
        backgroundColor: _reacttheme.tokens.colorNeutralBackground1
    },
    inverted: {
        color: _reacttheme.tokens.colorNeutralForegroundInverted2,
        backgroundColor: _reacttheme.tokens.colorNeutralBackgroundInverted
    },
    icon: {
        height: '16px',
        fontSize: '16px',
        padding: '0 8px 0 0'
    },
    avatar: {
        margin: '0 8px 0 0'
    },
    action: {
        padding: '5px 10px',
        minWidth: 0,
        marginLeft: 'auto',
        color: _reacttheme.tokens.colorBrandForeground1
    }
});
const useIntentIconStyles = (0, _react.makeStyles)({
    success: {
        color: _reacttheme.tokens.colorPaletteGreenForeground3
    },
    error: {
        color: _reacttheme.tokens.colorPaletteRedForeground3
    },
    warning: {
        color: _reacttheme.tokens.colorPaletteYellowForeground2
    },
    info: {
        color: _reacttheme.tokens.colorNeutralForeground2
    }
});
const useIntentIconStylesInverted = (0, _react.makeStyles)({
    success: {
        color: _reacttheme.tokens.colorPaletteGreenForegroundInverted
    },
    error: {
        color: _reacttheme.tokens.colorPaletteRedForegroundInverted
    },
    warning: {
        color: _reacttheme.tokens.colorPaletteYellowForegroundInverted
    },
    info: {
        color: _reacttheme.tokens.colorNeutralForegroundInverted2
    }
});
const useActionButtonColorInverted = (0, _react.makeStyles)({
    action: {
        color: _reacttheme.tokens.colorBrandForegroundInverted,
        ...(0, _reacttabster.createCustomFocusIndicatorStyle)({
            ..._react.shorthands.borderColor(_reacttheme.tokens.colorTransparentStrokeInteractive),
            outlineColor: _reacttheme.tokens.colorNeutralBackground5Pressed
        }, {
            enableOutline: true
        })
    }
});
const useAlertStyles_unstable = (state)=>{
    const inverted = state.appearance === 'inverted';
    const styles = useStyles();
    const intentIconStylesPrimary = useIntentIconStyles();
    const intentIconStylesInverted = useIntentIconStylesInverted();
    const actionStylesInverted = useActionButtonColorInverted();
    // eslint-disable-next-line react-compiler/react-compiler
    state.root.className = (0, _react.mergeClasses)(alertClassNames.root, styles.root, inverted && styles.inverted, state.root.className);
    if (state.icon) {
        state.icon.className = (0, _react.mergeClasses)(alertClassNames.icon, styles.icon, state.intent && (inverted ? intentIconStylesInverted[state.intent] : intentIconStylesPrimary[state.intent]), state.icon.className);
    }
    if (state.avatar) {
        state.avatar.className = (0, _react.mergeClasses)(alertClassNames.avatar, styles.avatar, state.avatar.className);
    }
    if (state.action) {
        // Note: inverted && actionStylesInverted.action has the highest piority and must be merged last
        state.action.className = (0, _react.mergeClasses)(alertClassNames.action, styles.action, inverted && actionStylesInverted.action, state.action.className);
    }
    return state;
};
