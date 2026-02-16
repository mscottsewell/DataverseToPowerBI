"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "renderMessageBarGroup_unstable", {
    enumerable: true,
    get: function() {
        return renderMessageBarGroup_unstable;
    }
});
const _jsxruntime = require("@fluentui/react-jsx-runtime/jsx-runtime");
const _reactutilities = require("@fluentui/react-utilities");
const _reactmotion = require("@fluentui/react-motion");
const _MessageBarGroupmotions = require("./MessageBarGroup.motions");
const _MotionRefForwarder = require("../MotionRefForwarder");
const renderMessageBarGroup_unstable = (state)=>{
    (0, _reactutilities.assertSlots)(state);
    return /*#__PURE__*/ (0, _jsxruntime.jsx)(state.root, {
        children: /*#__PURE__*/ (0, _jsxruntime.jsx)(_reactmotion.PresenceGroup, {
            children: state.children.map((child)=>/*#__PURE__*/ (0, _jsxruntime.jsx)(_MessageBarGroupmotions.MessageBarMotion, {
                    animate: state.animate,
                    unmountOnExit: true,
                    children: /*#__PURE__*/ (0, _jsxruntime.jsx)(_MotionRefForwarder.MotionRefForwarder, {
                        children: child
                    })
                }, child.key))
        })
    });
};
