'use client';
import * as React from 'react';
import { createContext, useContextSelector } from '@fluentui/react-context-selector';
export const MenuContext = createContext(undefined);
const menuContextDefaultValue = {
    open: false,
    setOpen: ()=>false,
    checkedValues: {},
    onCheckedValueChange: ()=>null,
    isSubmenu: false,
    // eslint-disable-next-line @typescript-eslint/no-deprecated
    triggerRef: {
        current: null
    },
    // eslint-disable-next-line @typescript-eslint/no-deprecated
    menuPopoverRef: {
        current: null
    },
    mountNode: null,
    triggerId: '',
    openOnContext: false,
    openOnHover: false,
    hasIcons: false,
    hasCheckmarks: false,
    inline: false,
    persistOnItemClick: false
};
export const MenuProvider = MenuContext.Provider;
export const useMenuContext_unstable = (selector)=>useContextSelector(MenuContext, (ctx = menuContextDefaultValue)=>selector(ctx));
