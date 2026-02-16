'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useNavDrawerBody_unstable", {
    enumerable: true,
    get: function() {
        return useNavDrawerBody_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reacttabster = require("@fluentui/react-tabster");
const _reactdrawer = require("@fluentui/react-drawer");
const _NavContext = require("../NavContext");
const useNavDrawerBody_unstable = (props, ref)=>{
    const { tabbable } = (0, _NavContext.useNavContext_unstable)();
    const focusAttributes = (0, _reacttabster.useArrowNavigationGroup)({
        axis: 'vertical',
        circular: true,
        tabbable
    });
    return (0, _reactdrawer.useDrawerBody_unstable)({
        ...focusAttributes,
        ...props
    }, ref);
};
