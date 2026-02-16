/**
 * Default timing constants for stagger animations (milliseconds).
 */ "use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
function _export(target, all) {
    for(var name in all)Object.defineProperty(target, name, {
        enumerable: true,
        get: all[name]
    });
}
_export(exports, {
    DEFAULT_ITEM_DELAY: function() {
        return DEFAULT_ITEM_DELAY;
    },
    DEFAULT_ITEM_DURATION: function() {
        return DEFAULT_ITEM_DURATION;
    }
});
const _reactmotion = require("@fluentui/react-motion");
const DEFAULT_ITEM_DELAY = _reactmotion.motionTokens.durationFaster;
const DEFAULT_ITEM_DURATION = _reactmotion.motionTokens.durationNormal;
