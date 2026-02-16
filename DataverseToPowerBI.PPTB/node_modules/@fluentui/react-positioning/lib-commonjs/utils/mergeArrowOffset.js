'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "mergeArrowOffset", {
    enumerable: true,
    get: function() {
        return mergeArrowOffset;
    }
});
function mergeArrowOffset(userOffset, arrowHeight) {
    if (typeof userOffset === 'number') {
        return addArrowOffset(userOffset, arrowHeight);
    }
    if (typeof userOffset === 'object' && userOffset !== null) {
        return addArrowOffset(userOffset, arrowHeight);
    }
    if (typeof userOffset === 'function') {
        return (offsetParams)=>{
            const offset = userOffset(offsetParams);
            return addArrowOffset(offset, arrowHeight);
        };
    }
    return {
        mainAxis: arrowHeight
    };
}
const addArrowOffset = (offset, arrowHeight)=>{
    if (typeof offset === 'number') {
        return {
            mainAxis: offset + arrowHeight
        };
    }
    var _offset_mainAxis;
    return {
        ...offset,
        mainAxis: ((_offset_mainAxis = offset.mainAxis) !== null && _offset_mainAxis !== void 0 ? _offset_mainAxis : 0) + arrowHeight
    };
};
