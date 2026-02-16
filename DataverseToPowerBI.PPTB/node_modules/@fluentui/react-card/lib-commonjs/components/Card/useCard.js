'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useCard_unstable", {
    enumerable: true,
    get: function() {
        return useCard_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _reacttabster = require("@fluentui/react-tabster");
const _useCardSelectable = require("./useCardSelectable");
const _CardContext = require("./CardContext");
const focusMap = {
    off: undefined,
    'no-tab': 'limited-trap-focus',
    'tab-exit': 'limited',
    'tab-only': 'unlimited'
};
/**
 * Create the state for interactive cards.
 *
 * This internal hook defines if the card is interactive
 * and control focus properties based on that.
 *
 * @param props - props from this instance of Card
 */ const useCardInteractive = ({ focusMode: initialFocusMode, disabled = false, ...props })=>{
    const interactive = [
        'onClick',
        'onDoubleClick',
        'onMouseUp',
        'onMouseDown',
        'onPointerUp',
        'onPointerDown',
        'onTouchStart',
        'onTouchEnd',
        'onDragStart',
        'onDragEnd'
    ].some((prop)=>props[prop]);
    // default focusMode to tab-only when interactive, and off when not
    const focusMode = initialFocusMode !== null && initialFocusMode !== void 0 ? initialFocusMode : interactive ? 'no-tab' : 'off';
    const groupperAttrs = (0, _reacttabster.useFocusableGroup)({
        tabBehavior: focusMap[focusMode]
    });
    if (disabled) {
        return {
            interactive: false,
            focusAttributes: null
        };
    }
    if (focusMode === 'off') {
        return {
            interactive,
            focusAttributes: null
        };
    }
    return {
        interactive,
        focusAttributes: {
            ...groupperAttrs,
            tabIndex: 0
        }
    };
};
const useCard_unstable = (props, ref)=>{
    const { appearance = 'filled', orientation = 'vertical', size = 'medium', disabled = false, ...restProps } = props;
    const [referenceId, setReferenceId] = _react.useState(_CardContext.cardContextDefaultValue.selectableA11yProps.referenceId);
    const [referenceLabel, setReferenceLabel] = _react.useState(_CardContext.cardContextDefaultValue.selectableA11yProps.referenceId);
    const cardBaseRef = (0, _reacttabster.useFocusWithin)();
    const { selectable, selected, selectableCardProps, selectFocused, checkboxSlot, floatingActionSlot } = (0, _useCardSelectable.useCardSelectable)(props, {
        referenceId,
        referenceLabel
    }, cardBaseRef);
    const cardRef = (0, _reactutilities.useMergedRefs)(cardBaseRef, ref);
    const { interactive, focusAttributes } = useCardInteractive(props);
    let cardRootProps = {
        ...!selectable ? focusAttributes : null,
        ...restProps,
        ...selectableCardProps
    };
    if (disabled) {
        cardRootProps = {
            ...restProps,
            'aria-disabled': true,
            onClick: undefined
        };
    }
    return {
        appearance,
        orientation,
        size,
        interactive,
        selectable,
        selectFocused,
        selected,
        disabled,
        selectableA11yProps: {
            setReferenceId,
            referenceId,
            referenceLabel,
            setReferenceLabel
        },
        components: {
            root: 'div',
            floatingAction: 'div',
            checkbox: 'input'
        },
        root: _reactutilities.slot.always((0, _reactutilities.getIntrinsicElementProps)('div', {
            ref: cardRef,
            role: 'group',
            ...cardRootProps
        }), {
            elementType: 'div'
        }),
        floatingAction: floatingActionSlot,
        checkbox: checkboxSlot
    };
};
