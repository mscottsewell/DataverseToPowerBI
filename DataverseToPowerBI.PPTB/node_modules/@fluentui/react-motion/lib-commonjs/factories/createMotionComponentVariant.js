"use strict";
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
    createMotionComponentVariant: function() {
        return createMotionComponentVariant;
    },
    createMotionFnVariant: function() {
        return createMotionFnVariant;
    }
});
const _createMotionComponent = require("./createMotionComponent");
function createMotionFnVariant(motionFn, variantParams) {
    const variantFn = (runtimeParams)=>motionFn({
            ...variantParams,
            ...runtimeParams
        });
    return variantFn;
}
function createMotionComponentVariant(component, variantParams) {
    const originalFn = component[_createMotionComponent.MOTION_DEFINITION];
    // The variant params become new defaults, but they can still be overridden by runtime params.
    const variantFn = createMotionFnVariant(originalFn, variantParams);
    return (0, _createMotionComponent.createMotionComponent)(variantFn);
}
