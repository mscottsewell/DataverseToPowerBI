'use client';
import * as React from 'react';
export function useNavContextValues_unstable(state) {
    const { selectedValue, selectedCategoryValue, onRegister, onUnregister, onSelect, getRegisteredNavItems, onRequestNavCategoryItemToggle, openCategories, multiple, density, onNavItemSelect, tabbable } = state;
    const navContext = React.useMemo(()=>({
            selectedValue,
            selectedCategoryValue,
            onSelect,
            onRegister,
            onUnregister,
            getRegisteredNavItems,
            onRequestNavCategoryItemToggle,
            openCategories,
            multiple,
            density,
            onNavItemSelect,
            tabbable
        }), [
        selectedValue,
        selectedCategoryValue,
        onSelect,
        onRegister,
        onUnregister,
        getRegisteredNavItems,
        onRequestNavCategoryItemToggle,
        openCategories,
        multiple,
        density,
        onNavItemSelect,
        tabbable
    ]);
    return {
        nav: navContext
    };
}
