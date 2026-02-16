'use client';
"use strict";
Object.defineProperty(exports, "__esModule", {
    value: true
});
Object.defineProperty(exports, "useTypingAnnounce", {
    enumerable: true,
    get: function() {
        return useTypingAnnounce;
    }
});
const _interop_require_wildcard = require("@swc/helpers/_/_interop_require_wildcard");
const _react = /*#__PURE__*/ _interop_require_wildcard._(require("react"));
const _reactutilities = require("@fluentui/react-utilities");
const _reactsharedcontexts = require("@fluentui/react-shared-contexts");
const valueMutationOptions = {
    attributes: true,
    subtree: true,
    characterData: true,
    attributeFilter: [
        'value'
    ]
};
function useTypingAnnounce() {
    const { targetDocument } = (0, _reactsharedcontexts.useFluent_unstable)();
    const { announce } = (0, _reactsharedcontexts.useAnnounce)();
    const inputRef = _react.useRef(null);
    const observer = _react.useRef(undefined);
    const [setTypingTimeout, clearTypingTimeout] = (0, _reactutilities.useTimeout)();
    const messageQueue = _react.useRef([]);
    const callback = _react.useCallback((mutationList, mutationObserver)=>{
        setTypingTimeout(()=>{
            messageQueue.current.forEach(({ message, options })=>{
                announce(message, options);
            });
            messageQueue.current.length = 0;
            mutationObserver.disconnect();
        }, 500);
    }, [
        announce,
        setTypingTimeout
    ]);
    const typingAnnounce = _react.useCallback((message, options = {})=>{
        messageQueue.current.push({
            message,
            options
        });
        if (inputRef.current && observer.current) {
            observer.current.observe(inputRef.current, valueMutationOptions);
        }
        setTypingTimeout(()=>{
            observer.current && callback([], observer.current);
        }, 500);
    }, [
        callback,
        inputRef,
        setTypingTimeout
    ]);
    _react.useEffect(()=>{
        const win = targetDocument === null || targetDocument === void 0 ? void 0 : targetDocument.defaultView;
        if (!win) {
            return;
        }
        if (!observer.current) {
            observer.current = new win.MutationObserver(callback);
        }
        return ()=>{
            // Clean up the observer when the component unmounts
            if (observer.current) {
                observer.current.disconnect();
                clearTypingTimeout();
            }
        };
    }, [
        callback,
        clearTypingTimeout,
        targetDocument
    ]);
    return {
        typingAnnounce,
        inputRef
    };
}
