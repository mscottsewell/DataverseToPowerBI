"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useDrawerDefaultProps", {
    enumerable: true,
    get: function() {
        return useDrawerDefaultProps;
    }
});
function useDrawerDefaultProps(props) {
    const { open = false, size = 'small', position = 'start', unmountOnClose = true } = props;
    return {
        size,
        position,
        open,
        unmountOnClose
    };
}
