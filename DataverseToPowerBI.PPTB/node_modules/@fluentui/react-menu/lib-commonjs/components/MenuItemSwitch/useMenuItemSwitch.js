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
    useMenuItemSwitchBase_unstable: function() {
        return useMenuItemSwitchBase_unstable;
    },
    useMenuItemSwitch_unstable: function() {
        return useMenuItemSwitch_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _useMenuItemCheckbox = require("../MenuItemCheckbox/useMenuItemCheckbox");
const _reacticons = require("@fluentui/react-icons");
const _useMenuItemSwitchStylesstyles = require("./useMenuItemSwitchStyles.styles");
const useMenuItemSwitch_unstable = (props, ref)=>{
    const state = useMenuItemSwitchBase_unstable(props, ref);
    // Set default icon for switch indicator
    if (state.switchIndicator) {
        var _state_switchIndicator;
        var _children;
        (_children = (_state_switchIndicator = state.switchIndicator).children) !== null && _children !== void 0 ? _children : _state_switchIndicator.children = /*#__PURE__*/ _react.createElement(_reacticons.CircleFilled, {
            className: _useMenuItemSwitchStylesstyles.circleFilledClassName
        });
    }
    return state;
};
const useMenuItemSwitchBase_unstable = (props, ref)=>{
    const baseState = (0, _useMenuItemCheckbox.useMenuItemCheckboxBase_unstable)(props, ref);
    return {
        ...baseState,
        switchIndicator: _reactutilities.slot.optional(props.switchIndicator, {
            renderByDefault: true,
            elementType: 'span'
        }),
        components: {
            // eslint-disable-next-line @typescript-eslint/no-deprecated
            ...baseState.components,
            switchIndicator: 'span'
        }
    };
};
