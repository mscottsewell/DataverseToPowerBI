"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "DialogSurfaceMotion", {
    enumerable: true,
    get: function() {
        return DialogSurfaceMotion;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const _reactmotioncomponentspreview = require("@fluentui/react-motion-components-preview");
const DialogSurfaceMotion = (0, _reactmotion.createPresenceComponentVariant)(_reactmotioncomponentspreview.Scale, {
    outScale: 0.85,
    easing: _reactmotion.motionTokens.curveDecelerateMid,
    duration: _reactmotion.motionTokens.durationGentle,
    exitEasing: _reactmotion.motionTokens.curveAccelerateMin,
    exitDuration: _reactmotion.motionTokens.durationGentle
});
