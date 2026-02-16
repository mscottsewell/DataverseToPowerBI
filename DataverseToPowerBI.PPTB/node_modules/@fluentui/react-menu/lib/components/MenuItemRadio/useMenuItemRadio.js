'use client';
import * as React from 'react';
import { slot } from '@fluentui/react-utilities';
import { Checkmark16Filled } from '@fluentui/react-icons';
import { useMenuListContext_unstable } from '../../contexts/menuListContext';
import { useMenuItemBase_unstable } from '../MenuItem/useMenuItem';
/**
 * Given user props, returns state and render function for a MenuItemRadio.
 */ export const useMenuItemRadio_unstable = (props, ref)=>{
    const state = useMenuItemRadioBase_unstable(props, ref);
    // Set default checkmark icon
    if (state.checkmark) {
        var _state_checkmark;
        var _children;
        (_children = (_state_checkmark = state.checkmark).children) !== null && _children !== void 0 ? _children : _state_checkmark.children = /*#__PURE__*/ React.createElement(Checkmark16Filled, null);
    }
    return state;
};
/**
 * Base hook for MenuItemRadio component, produces state required to render the component.
 * It doesn't set any design-related props specific to MenuItemRadio.
 *
 * @internal
 */ export const useMenuItemRadioBase_unstable = (props, ref)=>{
    const { name, value } = props;
    const checked = useMenuListContext_unstable((context)=>{
        var _context_checkedValues;
        const checkedItems = ((_context_checkedValues = context.checkedValues) === null || _context_checkedValues === void 0 ? void 0 : _context_checkedValues[name]) || [];
        return checkedItems.indexOf(value) !== -1;
    });
    const selectRadio = useMenuListContext_unstable((context)=>context.selectRadio);
    return {
        ...useMenuItemBase_unstable({
            ...props,
            role: 'menuitemradio',
            'aria-checked': checked,
            checkmark: slot.optional(props.checkmark, {
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
