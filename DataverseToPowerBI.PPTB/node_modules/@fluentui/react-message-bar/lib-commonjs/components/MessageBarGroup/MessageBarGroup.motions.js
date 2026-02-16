"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "MessageBarMotion", {
    enumerable: true,
    get: function() {
        return MessageBarMotion;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const _reactmotioncomponentspreview = require("@fluentui/react-motion-components-preview");
const MessageBarMotion = (0, _reactmotion.createPresenceComponent)(({ animate })=>{
    const duration = _reactmotion.motionTokens.durationGentle;
    return {
        enter: animate === 'both' ? [
            (0, _reactmotioncomponentspreview.fadeAtom)({
                direction: 'enter',
                duration
            }),
            (0, _reactmotioncomponentspreview.slideAtom)({
                direction: 'enter',
                outY: '-100%',
                duration
            })
        ] : [],
        // Always exit with a fade
        exit: (0, _reactmotioncomponentspreview.fadeAtom)({
            direction: 'exit',
            duration
        })
    };
});
