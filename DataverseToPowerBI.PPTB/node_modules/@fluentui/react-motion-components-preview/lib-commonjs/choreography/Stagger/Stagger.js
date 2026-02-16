'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "Stagger", {
    enumerable: true,
    get: function() {
        return Stagger;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _useStaggerItemsVisibility = require("./useStaggerItemsVisibility");
const _utils = require("./utils");
/**
 * Shared utility to detect optimal stagger modes based on children components.
 * Consolidates the auto-detection logic used by both StaggerMain and createStaggerDirection.
 */ const detectStaggerModes = (children, options)=>{
    const { hideMode, delayMode, fallbackHideMode = 'visibilityStyle' } = options;
    const childMapping = (0, _utils.getStaggerChildMapping)(children);
    const elements = Object.values(childMapping).map((item)=>item.element);
    const hasVisiblePropSupport = elements.every((child)=>(0, _utils.acceptsVisibleProp)(child));
    const hasDelayPropSupport = elements.every((child)=>(0, _utils.acceptsDelayProps)(child));
    return {
        hideMode: hideMode !== null && hideMode !== void 0 ? hideMode : hasVisiblePropSupport ? 'visibleProp' : fallbackHideMode,
        delayMode: delayMode !== null && delayMode !== void 0 ? delayMode : hasDelayPropSupport ? 'delayProp' : 'timing'
    };
};
const StaggerOneWay = ({ children, direction, itemDelay = _utils.DEFAULT_ITEM_DELAY, itemDuration = _utils.DEFAULT_ITEM_DURATION, reversed = false, hideMode, delayMode = 'timing', onMotionFinish })=>{
    const childMapping = _react.useMemo(()=>(0, _utils.getStaggerChildMapping)(children), [
        children
    ]);
    // Always call hooks at the top level, regardless of delayMode
    const { itemsVisibility } = (0, _useStaggerItemsVisibility.useStaggerItemsVisibility)({
        childMapping,
        itemDelay,
        itemDuration,
        direction,
        reversed,
        onMotionFinish,
        hideMode
    });
    // For delayProp mode, pass delay props directly to motion components
    if (delayMode === 'delayProp') {
        return /*#__PURE__*/ _react.createElement(_react.Fragment, null, Object.entries(childMapping).map(([key, { element, index }])=>{
            const staggerIndex = reversed ? Object.keys(childMapping).length - 1 - index : index;
            const delay = staggerIndex * itemDelay;
            // Clone element with delay prop (for enter direction) or exitDelay prop (for exit direction)
            const delayProp = direction === 'enter' ? {
                delay
            } : {
                exitDelay: delay
            };
            // Only set visible prop if the component supports it
            // Set visible based on direction: true for enter, false for exit
            const visibleProp = (0, _utils.acceptsVisibleProp)(element) ? {
                visible: direction === 'enter'
            } : {};
            return /*#__PURE__*/ _react.cloneElement(element, {
                key,
                ...visibleProp,
                ...delayProp
            });
        }));
    }
    // For timing mode, use the existing timing-based implementation
    return /*#__PURE__*/ _react.createElement(_react.Fragment, null, Object.entries(childMapping).map(([key, { element }])=>{
        if (hideMode === 'visibleProp') {
            // Use a generic record type for props to avoid `any` while still allowing unknown prop shapes.
            return /*#__PURE__*/ _react.cloneElement(element, {
                key,
                visible: itemsVisibility[key]
            });
        } else if (hideMode === 'visibilityStyle') {
            const childProps = element.props;
            const style = {
                ...childProps === null || childProps === void 0 ? void 0 : childProps.style,
                visibility: itemsVisibility[key] ? 'visible' : 'hidden'
            };
            return /*#__PURE__*/ _react.cloneElement(element, {
                key,
                style
            });
        } else {
            // unmount mode
            return itemsVisibility[key] ? /*#__PURE__*/ _react.cloneElement(element, {
                key
            }) : null;
        }
    }));
};
// Shared helper for StaggerIn and StaggerOut
const createStaggerDirection = (direction)=>{
    const StaggerDirection = ({ hideMode, delayMode, children, ...props })=>{
        // Auto-detect modes for better performance with motion components
        const { hideMode: resolvedHideMode, delayMode: resolvedDelayMode } = detectStaggerModes(children, {
            hideMode,
            delayMode,
            // One-way stagger falls back to visibilityStyle if it doesn't detect visibleProp support
            fallbackHideMode: 'visibilityStyle'
        });
        return /*#__PURE__*/ _react.createElement(StaggerOneWay, {
            ...props,
            children: children,
            direction: direction,
            hideMode: resolvedHideMode,
            delayMode: resolvedDelayMode
        });
    };
    return StaggerDirection;
};
const StaggerIn = createStaggerDirection('enter');
const StaggerOut = createStaggerDirection('exit');
// Main Stagger component with auto-detection or explicit modes
const StaggerMain = (props)=>{
    const { children, visible = false, hideMode, delayMode, ...rest } = props;
    // Auto-detect modes for bidirectional stagger
    const { hideMode: resolvedHideMode, delayMode: resolvedDelayMode } = detectStaggerModes(children, {
        hideMode,
        delayMode,
        // Bidirectional stagger falls back to visibilityStyle if it doesn't detect visibleProp support
        fallbackHideMode: 'visibilityStyle'
    });
    const direction = visible ? 'enter' : 'exit';
    return /*#__PURE__*/ _react.createElement(StaggerOneWay, {
        ...rest,
        children: children,
        hideMode: resolvedHideMode,
        delayMode: resolvedDelayMode,
        direction: direction
    });
};
const Stagger = Object.assign(StaggerMain, {
    In: StaggerIn,
    Out: StaggerOut
});
