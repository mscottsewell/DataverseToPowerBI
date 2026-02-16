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
    DialogBackdropContext: function() {
        return DialogBackdropContext;
    },
    DialogBackdropProvider: function() {
        return DialogBackdropProvider;
    },
    useDialogBackdropContext_unstable: function() {
        return useDialogBackdropContext_unstable;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const DialogBackdropContext = /*#__PURE__*/ _react.createContext(undefined);
const DialogBackdropProvider = DialogBackdropContext.Provider;
const useDialogBackdropContext_unstable = ()=>{
    return _react.useContext(DialogBackdropContext);
};
