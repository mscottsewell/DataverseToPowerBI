'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useApplyScrollbarWidth", {
    enumerable: true,
    get: function() {
        return useApplyScrollbarWidth;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _measureScrollBarWidth = require("../utils/measureScrollBarWidth");
const cache = new WeakMap();
function useApplyScrollbarWidth(options = {}) {
    const { force, property = 'width' } = options;
    const applyScrollbarWidth = _react.useCallback((element)=>{
        if (!element) {
            return;
        }
        // If we have a cached value, use it
        if (!force && cache.has(element.ownerDocument)) {
            const cachedWidth = cache.get(element.ownerDocument);
            if (cachedWidth !== undefined) {
                element.style.setProperty(property, `${cachedWidth}px`);
                return;
            }
        }
        // Measure the scrollbar width and apply it to the element
        const scrollbarWidth = (0, _measureScrollBarWidth.measureScrollbarWidth)(element.ownerDocument);
        cache.set(element.ownerDocument, scrollbarWidth);
        element.style.setProperty(property, `${scrollbarWidth}px`);
    }, [
        force,
        property
    ]);
    return applyScrollbarWidth;
}
