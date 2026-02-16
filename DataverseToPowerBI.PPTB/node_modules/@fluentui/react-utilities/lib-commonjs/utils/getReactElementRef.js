"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "getReactElementRef", {
    enumerable: true,
    get: function() {
        return getReactElementRef;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const IS_REACT_19_OR_HIGHER = parseInt(_react.version, 10) >= 19;
function getReactElementRef(element) {
    if (!element) {
        return undefined;
    }
    if (IS_REACT_19_OR_HIGHER) {
        return element.props.ref;
    }
    return element.ref;
}
