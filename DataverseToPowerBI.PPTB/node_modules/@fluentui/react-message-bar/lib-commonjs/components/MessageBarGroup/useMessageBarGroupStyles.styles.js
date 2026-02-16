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
    messageBarGroupClassNames: function() {
        return messageBarGroupClassNames;
    },
    useMessageBarGroupStyles_unstable: function() {
        return useMessageBarGroupStyles_unstable;
    }
});
const _react = require("@griffel/react");
const messageBarGroupClassNames = {
    root: 'fui-MessageBarGroup'
};
const useMessageBarGroupStyles_unstable = (state)=>{
    'use no memo';
    state.root.className = (0, _react.mergeClasses)(messageBarGroupClassNames.root, state.root.className);
    return state;
};
