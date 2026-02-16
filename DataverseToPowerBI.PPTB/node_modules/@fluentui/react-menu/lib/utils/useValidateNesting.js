'use client';
import * as React from 'react';
import { useFluent_unstable as useFluent } from '@fluentui/react-shared-contexts';
import { useMenuContext_unstable } from '../contexts/menuContext';
export const useValidateNesting = (componentName)=>{
    'use no memo';
    const { targetDocument } = useFluent();
    const triggerRef = useMenuContext_unstable((context)=>context.triggerRef);
    const inline = useMenuContext_unstable((context)=>context.inline);
    const ref = React.useRef(null);
    if (process.env.NODE_ENV !== 'production') {
        // This check should run only in development mode
        // It's okay to disable the ESLint rule because we ar checking env variable statically (not at runtime)
        // eslint-disable-next-line react-hooks/rules-of-hooks
        React.useEffect(()=>{
            let ancestor = ref.current;
            let ancestorComponentName = '';
            do {
                var _ancestor_parentElement;
                ancestor = (_ancestor_parentElement = ancestor === null || ancestor === void 0 ? void 0 : ancestor.parentElement) !== null && _ancestor_parentElement !== void 0 ? _ancestor_parentElement : null;
                if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuList')) {
                    break;
                } else if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuGrid')) {
                    ancestorComponentName = 'MenuGrid';
                } else if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuGridItem')) {
                    ancestorComponentName = 'MenuGridItem';
                } else if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuGridRow')) {
                    ancestorComponentName = 'MenuGridRow';
                } else if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuGridCell')) {
                    ancestorComponentName = 'MenuGridCell';
                }
                if ([
                    'MenuItem',
                    'MenuItemCheckbox',
                    'MenuItemRadio'
                ].includes(componentName)) {
                    if ([
                        'MenuGrid',
                        'MenuGridItem',
                        'MenuGridRow',
                        'MenuGridCell'
                    ].includes(ancestorComponentName)) {
                        throw new Error(`${componentName} is incorrectly nested within ${ancestorComponentName}. You probably want to wrap it in a MenuList instead.`);
                    }
                } else if (componentName === 'MenuList') {
                    if (ancestorComponentName === 'MenuGridCell') {
                        if (inline && getCellOfTrigger(triggerRef.current, targetDocument) === ancestor) {
                            break;
                        }
                        throw new Error(`MenuList is incorrectly nested within MenuGridCell.`);
                    } else if ([
                        'MenuGrid',
                        'MenuGridItem',
                        'MenuGridRow'
                    ].includes(ancestorComponentName)) {
                        throw new Error(`MenuList is incorrectly nested within ${ancestorComponentName}.`);
                    }
                }
            }while (ancestor && ancestor !== (targetDocument === null || targetDocument === void 0 ? void 0 : targetDocument.body))
        }, [
            componentName,
            ref,
            triggerRef,
            inline,
            targetDocument
        ]);
    }
    return ref;
};
const getCellOfTrigger = (trigger, targetDocument)=>{
    let ancestor = trigger === null || trigger === void 0 ? void 0 : trigger.parentElement;
    while(ancestor && ancestor !== (targetDocument === null || targetDocument === void 0 ? void 0 : targetDocument.body)){
        if (ancestor === null || ancestor === void 0 ? void 0 : ancestor.classList.contains('fui-MenuGridCell')) {
            return ancestor;
        }
        var _ancestor_parentElement;
        ancestor = (_ancestor_parentElement = ancestor === null || ancestor === void 0 ? void 0 : ancestor.parentElement) !== null && _ancestor_parentElement !== void 0 ? _ancestor_parentElement : null;
    }
    return null;
};
