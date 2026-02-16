'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useOverflowItem", {
    enumerable: true,
    get: function() {
        return useOverflowItem;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _overflowContext = require("./overflowContext");
function useOverflowItem(id, priority, groupId, pinned) {
    const ref = _react.useRef(null);
    const registerItem = (0, _overflowContext.useOverflowContext)((v)=>v.registerItem);
    (0, _reactutilities.useIsomorphicLayoutEffect)(()=>{
        if (process.env.NODE_ENV !== 'production') {
            if (typeof pinned !== 'undefined' && typeof priority !== 'undefined' && pinned) {
                // eslint-disable-next-line no-console
                console.error(`useOverflowItem: Overflow item with id "${id}" has pinned=true and priority<0. ` + `Pinned items are always visible and should not have defined priority.`);
            }
        }
        if (ref.current) {
            return registerItem({
                element: ref.current,
                id,
                priority: priority !== null && priority !== void 0 ? priority : 0,
                groupId,
                pinned
            });
        }
    }, [
        id,
        priority,
        registerItem,
        groupId,
        pinned
    ]);
    return ref;
}
