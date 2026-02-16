"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "fadeAtom", {
    enumerable: true,
    get: function() {
        return fadeAtom;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const fadeAtom = ({ direction, duration, easing = _reactmotion.motionTokens.curveLinear, delay = 0, outOpacity = 0, inOpacity = 1 })=>{
    const keyframes = [
        {
            opacity: outOpacity
        },
        {
            opacity: inOpacity
        }
    ];
    if (direction === 'exit') {
        keyframes.reverse();
    }
    return {
        keyframes,
        duration,
        easing,
        delay,
        // Applying opacity backwards and forwards in time is important
        // to avoid a bug where a delayed animation is not hidden when it should be.
        fill: 'both'
    };
};
