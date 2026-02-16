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
    acceptsDelayProps: function() {
        return acceptsDelayProps;
    },
    acceptsVisibleProp: function() {
        return acceptsVisibleProp;
    },
    isMotionComponent: function() {
        return isMotionComponent;
    },
    isPresenceComponent: function() {
        return isPresenceComponent;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
function isMotionComponent(element) {
    if (!(element === null || element === void 0 ? void 0 : element.type) || typeof element.type !== 'function') {
        return false;
    }
    // Check if the component has the MOTION_DEFINITION symbol (internal to createMotionComponent)
    const symbols = Object.getOwnPropertySymbols(element.type);
    return symbols.some((sym)=>sym.description === 'MOTION_DEFINITION');
}
function isPresenceComponent(element) {
    if (!(element === null || element === void 0 ? void 0 : element.type) || typeof element.type !== 'function') {
        return false;
    }
    // Check if the component has the PRESENCE_MOTION_DEFINITION symbol (internal to createPresenceComponent)
    const symbols = Object.getOwnPropertySymbols(element.type);
    return symbols.some((sym)=>sym.description === 'PRESENCE_MOTION_DEFINITION');
}
function acceptsDelayProps(element) {
    return isPresenceComponent(element) || isMotionComponent(element);
}
function acceptsVisibleProp(element) {
    return isPresenceComponent(element);
}
