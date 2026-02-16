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
    useAccordionBase_unstable: function() {
        return useAccordionBase_unstable;
    },
    useAccordion_unstable: function() {
        return useAccordion_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _reacttabster = require("@fluentui/react-tabster");
const useAccordion_unstable = (props, ref)=>{
    const { navigation, ...baseProps } = props;
    const state = useAccordionBase_unstable(baseProps, ref);
    /** FIXME: deprecated will be removed after navigation prop is removed */ const arrowNavigationProps = (0, _reacttabster.useArrowNavigationGroup)({
        circular: navigation === 'circular',
        tabbable: true
    });
    return {
        navigation,
        ...state,
        root: {
            ...state.root,
            ...navigation ? arrowNavigationProps : undefined
        }
    };
};
const useAccordionBase_unstable = (props, ref)=>{
    const { openItems: controlledOpenItems, defaultOpenItems, multiple = false, collapsible = false, onToggle, ...rest } = props;
    const [openItems, setOpenItems] = (0, _reactutilities.useControllableState)({
        state: _react.useMemo(()=>normalizeValues(controlledOpenItems), [
            controlledOpenItems
        ]),
        defaultState: defaultOpenItems && (()=>initializeUncontrolledOpenItems({
                defaultOpenItems,
                multiple
            })),
        initialState: []
    });
    const requestToggle = (0, _reactutilities.useEventCallback)((data)=>{
        const nextOpenItems = updateOpenItems(data.value, openItems, multiple, collapsible);
        onToggle === null || onToggle === void 0 ? void 0 : onToggle(data.event, {
            value: data.value,
            openItems: nextOpenItems
        });
        setOpenItems(nextOpenItems);
    });
    return {
        collapsible,
        multiple,
        openItems,
        requestToggle,
        components: {
            root: 'div'
        },
        root: _reactutilities.slot.always({
            ref: ref,
            ...rest
        }, {
            elementType: 'div'
        })
    };
};
/**
 * Initial value for the uncontrolled case of the list of open indexes
 */ function initializeUncontrolledOpenItems({ defaultOpenItems, multiple }) {
    if (defaultOpenItems !== undefined) {
        if (Array.isArray(defaultOpenItems)) {
            return multiple ? defaultOpenItems : [
                defaultOpenItems[0]
            ];
        }
        return [
            defaultOpenItems
        ];
    }
    return [];
}
/**
 * Updates the list of open indexes based on an index that changes
 * @param value - the index that will change
 * @param previousOpenItems - list of current open indexes
 * @param multiple - if Accordion support multiple Panels opened at the same time
 * @param collapsible - if Accordion support multiple Panels closed at the same time
 */ function updateOpenItems(value, previousOpenItems, multiple, collapsible) {
    if (multiple) {
        if (previousOpenItems.includes(value)) {
            if (previousOpenItems.length > 1 || collapsible) {
                return previousOpenItems.filter((i)=>i !== value);
            }
        } else {
            return [
                ...previousOpenItems,
                value
            ].sort();
        }
    } else {
        return previousOpenItems[0] === value && collapsible ? [] : [
            value
        ];
    }
    return previousOpenItems;
}
/**
 * Normalizes Accordion index into an array of indexes
 */ function normalizeValues(index) {
    if (index === undefined) {
        return undefined;
    }
    return Array.isArray(index) ? index : [
        index
    ];
}
