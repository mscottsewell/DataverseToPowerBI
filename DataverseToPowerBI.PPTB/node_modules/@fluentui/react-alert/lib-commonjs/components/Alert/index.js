/* eslint-disable @typescript-eslint/no-deprecated */ "use strict";
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
    Alert: function() {
        return _Alert.Alert;
    },
    alertClassNames: function() {
        return _useAlertStylesstyles.alertClassNames;
    },
    renderAlert_unstable: function() {
        return _renderAlert.renderAlert_unstable;
    },
    useAlertStyles_unstable: function() {
        return _useAlertStylesstyles.useAlertStyles_unstable;
    },
    useAlert_unstable: function() {
        return _useAlert.useAlert_unstable;
    }
});
const _Alert = require("./Alert");
const _renderAlert = require("./renderAlert");
const _useAlert = require("./useAlert");
const _useAlertStylesstyles = require("./useAlertStyles.styles");
