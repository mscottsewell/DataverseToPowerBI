'use client';
import * as React from 'react';
import { useTimeout } from '@fluentui/react-utilities';
import { useAnnounce, useFluent_unstable as useFluent } from '@fluentui/react-shared-contexts';
const valueMutationOptions = {
    attributes: true,
    subtree: true,
    characterData: true,
    attributeFilter: [
        'value'
    ]
};
export function useTypingAnnounce() {
    const { targetDocument } = useFluent();
    const { announce } = useAnnounce();
    const inputRef = React.useRef(null);
    const observer = React.useRef(undefined);
    const [setTypingTimeout, clearTypingTimeout] = useTimeout();
    const messageQueue = React.useRef([]);
    const callback = React.useCallback((mutationList, mutationObserver)=>{
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
    const typingAnnounce = React.useCallback((message, options = {})=>{
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
    React.useEffect(()=>{
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
