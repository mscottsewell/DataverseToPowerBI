'use client';
import * as React from 'react';
import { useControllableState, useEventCallback, slot } from '@fluentui/react-utilities';
import { useArrowNavigationGroup } from '@fluentui/react-tabster';
/**
 * Returns the props and state required to render the component
 * @param props - Accordion properties
 * @param ref - reference to root HTMLElement of Accordion
 */ export const useAccordion_unstable = (props, ref)=>{
    const { // eslint-disable-next-line @typescript-eslint/no-deprecated
    navigation, ...baseProps } = props;
    const state = useAccordionBase_unstable(baseProps, ref);
    /** FIXME: deprecated will be removed after navigation prop is removed */ const arrowNavigationProps = useArrowNavigationGroup({
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
/**
 * Returns the props and state required to render the component
 * @param props - Accordion properties
 * @param ref - reference to root HTMLElement of Accordion
 */ export const useAccordionBase_unstable = (props, ref)=>{
    const { openItems: controlledOpenItems, defaultOpenItems, multiple = false, collapsible = false, onToggle, ...rest } = props;
    const [openItems, setOpenItems] = useControllableState({
        state: React.useMemo(()=>normalizeValues(controlledOpenItems), [
            controlledOpenItems
        ]),
        defaultState: defaultOpenItems && (()=>initializeUncontrolledOpenItems({
                defaultOpenItems,
                multiple
            })),
        initialState: []
    });
    const requestToggle = useEventCallback((data)=>{
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
        root: slot.always({
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
