'use client';
import * as React from 'react';
export const DialogBackdropContext = React.createContext(undefined);
export const DialogBackdropProvider = DialogBackdropContext.Provider;
export const useDialogBackdropContext_unstable = ()=>{
    return React.useContext(DialogBackdropContext);
};
