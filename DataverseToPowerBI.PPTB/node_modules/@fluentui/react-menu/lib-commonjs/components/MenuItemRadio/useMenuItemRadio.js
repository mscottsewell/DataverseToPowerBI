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
    useMenuItemRadioBase_unstable: function() {
        return useMenuItemRadioBase_unstable;
    },
    useMenuItemRadio_unstable: function() {
        return useMenuItemRadio_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _reacticons = require("@fluentui/react-icons");
const _menuListContext = require("../../contexts/menuListContext");
const _useMenuItem = require("../MenuItem/useMenuItem");
const useMenuItemRadio_unstable = (props, ref)=>{
    const state = useMenuItemRadioBase_unstable(props, ref);
    // Set default checkmark icon
    if (state.checkmark) {
        var _state_checkmark;
        var _children;
        (_children = (_state_checkmark = state.checkmark).children) !== null && _children !== void 0 ? _children : _state_checkmark.children = /*#__PURE__*/ _react.createElement(_reacticons.Checkmark16Filled, null);
    }
    return state;
};
const useMenuItemRadioBase_unstable = (props, ref)=>{
    const { name, value } = props;
    const checked = (0, _menuListContext.useMenuListContext_unstable)((context)=>{
        var _context_checkedValues;
        const checkedItems = ((_context_checkedValues = context.checkedValues) === null || _context_checkedValues === void 0 ? void 0 : _context_checkedValues[name]) || [];
        return checkedItems.indexOf(value) !== -1;
    });
    const selectRadio = (0, _menuListContext.useMenuListContext_unstable)((context)=>context.selectRadio);
    return {
        ...(0, _useMenuItem.useMenuItemBase_unstable)({
            ...props,
            role: 'menuitemradio',
            'aria-checked': checked,
            checkmark: _reactutilities.slot.optional(props.checkmark, {
                renderByDefault: true,
                elementType: 'span'
            }),
            onClick: (e)=>{
                var _props_onClick;
                selectRadio === null || selectRadio === void 0 ? void 0 : selectRadio(e, name, value, checked);
                (_props_onClick = props.onClick) === null || _props_onClick === void 0 ? void 0 : _props_onClick.call(props, e);
            }
        }, ref),
        checked,
        name,
        value
    };
};
