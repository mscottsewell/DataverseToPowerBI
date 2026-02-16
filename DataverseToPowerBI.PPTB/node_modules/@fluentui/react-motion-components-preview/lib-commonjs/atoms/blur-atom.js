"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "blurAtom", {
    enumerable: true,
    get: function() {
        return blurAtom;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const blurAtom = ({ direction, duration, easing = _reactmotion.motionTokens.curveLinear, delay = 0, outRadius = '10px', inRadius = '0px' })=>{
    const keyframes = [
        {
            filter: `blur(${outRadius})`
        },
        {
            filter: `blur(${inRadius})`
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
