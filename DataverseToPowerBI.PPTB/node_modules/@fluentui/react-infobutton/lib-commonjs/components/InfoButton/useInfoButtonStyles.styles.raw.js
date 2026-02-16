/* eslint-disable @typescript-eslint/no-deprecated */ 'use client';
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
    infoButtonClassNames: function() {
        return infoButtonClassNames;
    },
    useInfoButtonStyles_unstable: function() {
        return useInfoButtonStyles_unstable;
    }
});
const _reacttabster = require("@fluentui/react-tabster");
const _reacticons = require("@fluentui/react-icons");
const _react = require("@griffel/react");
const _reacttheme = require("@fluentui/react-theme");
const infoButtonClassNames = {
    root: 'fui-InfoButton',
    // this className won't be used, but it's needed to satisfy the type checker
    popover: 'fui-InfoButton__popover',
    info: 'fui-InfoButton__info'
};
/**
 * Styles for the root slot
 *
 * @deprecated use {@link @fluentui/react-components#InfoLabel} from `\@fluentui/react-components` or `\@fluentui/react-infolabel` instead
 */ const useButtonStyles = (0, _react.makeStyles)({
    base: {
        alignItems: 'center',
        boxSizing: 'border-box',
        display: 'inline-flex',
        justifyContent: 'center',
        textDecorationLine: 'none',
        verticalAlign: 'middle',
        position: 'relative',
        backgroundColor: _reacttheme.tokens.colorTransparentBackground,
        color: _reacttheme.tokens.colorNeutralForeground2,
        ..._react.shorthands.borderStyle('none'),
        ..._react.shorthands.borderRadius(_reacttheme.tokens.borderRadiusMedium),
        ..._react.shorthands.margin(0),
        ..._react.shorthands.padding(_reacttheme.tokens.spacingVerticalXS, _reacttheme.tokens.spacingHorizontalXS),
        [`& .${_reacticons.iconFilledClassName}`]: {
            display: 'none'
        },
        [`& .${_reacticons.iconRegularClassName}`]: {
            display: 'inline-flex'
        },
        ':hover': {
            backgroundColor: _reacttheme.tokens.colorTransparentBackgroundHover,
            color: _reacttheme.tokens.colorNeutralForeground2BrandHover,
            cursor: 'pointer',
            [`& .${_reacticons.iconFilledClassName}`]: {
                display: 'inline-flex'
            },
            [`& .${_reacticons.iconRegularClassName}`]: {
                display: 'none'
            }
        },
        ':hover:active': {
            backgroundColor: _reacttheme.tokens.colorTransparentBackgroundPressed,
            color: _reacttheme.tokens.colorNeutralForeground2BrandPressed
        }
    },
    selected: {
        backgroundColor: _reacttheme.tokens.colorTransparentBackgroundSelected,
        color: _reacttheme.tokens.colorNeutralForeground2BrandSelected,
        [`& .${_reacticons.iconFilledClassName}`]: {
            display: 'inline-flex'
        },
        [`& .${_reacticons.iconRegularClassName}`]: {
            display: 'none'
        },
        '@media (forced-colors: active)': {
            backgroundColor: 'Highlight',
            color: 'Canvas'
        }
    },
    highContrast: {
        '@media (forced-colors: active)': {
            color: 'CanvasText',
            ':hover,:hover:active': {
                forcedColorAdjust: 'none',
                backgroundColor: 'Highlight',
                color: 'Canvas'
            }
        }
    },
    focusIndicator: (0, _reacttabster.createFocusOutlineStyle)(),
    large: {
        ..._react.shorthands.padding(_reacttheme.tokens.spacingVerticalXXS, _reacttheme.tokens.spacingVerticalXXS)
    }
});
const usePopoverSurfaceStyles = (0, _react.makeStyles)({
    smallMedium: _reacttheme.typographyStyles.caption1,
    large: _reacttheme.typographyStyles.body1
});
const useInfoButtonStyles_unstable = (state)=>{
    const { size } = state;
    const { open } = state.popover;
    const buttonStyles = useButtonStyles();
    const popoverSurfaceStyles = usePopoverSurfaceStyles();
    // eslint-disable-next-line react-compiler/react-compiler
    state.info.className = (0, _react.mergeClasses)(infoButtonClassNames.info, size === 'large' ? popoverSurfaceStyles.large : popoverSurfaceStyles.smallMedium, state.info.className);
    state.root.className = (0, _react.mergeClasses)(infoButtonClassNames.root, buttonStyles.base, buttonStyles.highContrast, buttonStyles.focusIndicator, open && buttonStyles.selected, size === 'large' && buttonStyles.large, state.root.className);
    return state;
};
