"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "scaleAtom", {
    enumerable: true,
    get: function() {
        return scaleAtom;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const scaleAtom = ({ direction, duration, easing = _reactmotion.motionTokens.curveLinear, delay = 0, outScale = 0.9, inScale = 1 })=>{
    const keyframes = [
        {
            scale: outScale
        },
        {
            scale: inScale
        }
    ];
    if (direction === 'exit') {
        keyframes.reverse();
    }
    return {
        keyframes,
        duration,
        easing,
        delay
    };
};
