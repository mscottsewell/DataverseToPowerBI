'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useScrollbarWidth", {
    enumerable: true,
    get: function() {
        return useScrollbarWidth;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _measureScrollBarWidth = require("../utils/measureScrollBarWidth");
const cache = new WeakMap();
function useScrollbarWidth(options) {
    const { targetDocument, force } = options;
    return _react.useMemo(()=>{
        if (!targetDocument) {
            return 0;
        }
        if (!force && cache.has(targetDocument)) {
            return cache.get(targetDocument);
        }
        const scrollbarWidth = (0, _measureScrollBarWidth.measureScrollbarWidth)(targetDocument);
        cache.set(targetDocument, scrollbarWidth);
        return scrollbarWidth;
    }, [
        targetDocument,
        force
    ]);
}
