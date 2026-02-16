"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "getStaggerChildMapping", {
    enumerable: true,
    get: function() {
        return getStaggerChildMapping;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
function getStaggerChildMapping(children) {
    const childMapping = {};
    if (children) {
        _react.Children.toArray(children).forEach((child, index)=>{
            if (_react.isValidElement(child)) {
                var _child_key;
                childMapping[(_child_key = child.key) !== null && _child_key !== void 0 ? _child_key : ''] = {
                    element: child,
                    index
                };
            }
        });
    }
    return childMapping;
}
