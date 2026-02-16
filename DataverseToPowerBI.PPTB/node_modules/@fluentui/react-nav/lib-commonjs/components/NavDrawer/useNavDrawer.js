'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useNavDrawer_unstable", {
    enumerable: true,
    get: function() {
        return useNavDrawer_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactdrawer = require("@fluentui/react-drawer");
const _reactutilities = require("@fluentui/react-utilities");
const _useNav = require("../Nav/useNav");
const useNavDrawer_unstable = (props, ref)=>{
    const { size = undefined, tabbable = false } = props;
    const navState = (0, _useNav.useNav_unstable)(props, ref);
    return {
        ...navState,
        size,
        tabbable,
        components: {
            // TODO: remove once React v18 slot API is modified
            // this is a problem with the lack of support for union types on React v18
            // ComponentState is using React.ComponentType which will try to infer propType
            // propTypes WeakValidator signature will break distributive unions making this type invalid
            root: _reactdrawer.Drawer
        },
        root: _reactutilities.slot.always({
            ref,
            role: 'navigation',
            ...props
        }, {
            // TODO: remove once React v18 slot API is modified
            // this is a problem with the lack of support for union types on React v18
            // ComponentState is using React.ComponentType which will try to infer propType
            // propTypes WeakValidator signature will break distributive unions making this type invalid
            elementType: _reactdrawer.Drawer
        })
    };
};
