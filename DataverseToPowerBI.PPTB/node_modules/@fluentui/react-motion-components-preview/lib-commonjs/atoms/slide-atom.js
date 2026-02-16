"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "slideAtom", {
    enumerable: true,
    get: function() {
        return slideAtom;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const slideAtom = ({ direction, duration, easing = _reactmotion.motionTokens.curveLinear, delay = 0, outX = '0px', outY = '0px', inX = '0px', inY = '0px' })=>{
    const keyframes = [
        {
            translate: `${outX} ${outY}`
        },
        {
            translate: `${inX} ${inY}`
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
