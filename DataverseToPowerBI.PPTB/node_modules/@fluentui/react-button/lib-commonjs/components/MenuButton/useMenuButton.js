'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useMenuButton_unstable", {
    enumerable: true,
    get: function() {
        return useMenuButton_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reacticons = require("@fluentui/react-icons");
const _reactutilities = require("@fluentui/react-utilities");
const _index = require("../Button/index");
const useMenuButton_unstable = (props, ref)=>{
    const { menuIcon, ...buttonProps } = props;
    const buttonState = (0, _index.useButton_unstable)(buttonProps, ref);
    return {
        ...buttonState,
        iconOnly: Boolean(!props.children),
        // Slots definition
        components: {
            // eslint-disable-next-line @typescript-eslint/no-deprecated
            ...buttonState.components,
            menuIcon: 'span'
        },
        root: {
            ...buttonState.root,
            // force aria-expanded to be a boolean, not a string
            'aria-expanded': props['aria-expanded'] ? props['aria-expanded'] === 'true' || props['aria-expanded'] === true : false
        },
        menuIcon: _reactutilities.slot.optional(menuIcon, {
            defaultProps: {
                children: /*#__PURE__*/ _react.createElement(_reacticons.ChevronDownRegular, null)
            },
            renderByDefault: true,
            elementType: 'span'
        })
    };
};
