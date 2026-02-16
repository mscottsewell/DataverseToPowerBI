// src/index.ts
import { createStore } from "zustand";

// src/temporal.ts
var temporalStateCreator = (userSet, userGet, options) => {
  const stateCreator = (set, get) => {
    return {
      pastStates: options?.pastStates || [],
      futureStates: options?.futureStates || [],
      undo: (steps = 1) => {
        if (get().pastStates.length) {
          const currentState = options?.partialize?.(userGet()) || userGet();
          const statesToApply = get().pastStates.splice(-steps, steps);
          const nextState = statesToApply.shift();
          userSet(nextState);
          set({
            pastStates: get().pastStates,
            futureStates: get().futureStates.concat(
              options?.diff?.(currentState, nextState) || currentState,
              statesToApply.reverse()
            )
          });
        }
      },
      redo: (steps = 1) => {
        if (get().futureStates.length) {
          const currentState = options?.partialize?.(userGet()) || userGet();
          const statesToApply = get().futureStates.splice(-steps, steps);
          const nextState = statesToApply.shift();
          userSet(nextState);
          set({
            pastStates: get().pastStates.concat(
              options?.diff?.(currentState, nextState) || currentState,
              statesToApply.reverse()
            ),
            futureStates: get().futureStates
          });
        }
      },
      clear: () => set({ pastStates: [], futureStates: [] }),
      isTracking: true,
      pause: () => set({ isTracking: false }),
      resume: () => set({ isTracking: true }),
      setOnSave: (_onSave) => set({ _onSave }),
      // Internal properties
      _onSave: options?.onSave,
      _handleSet: (pastState, replace, currentState, deltaState) => {
        if (options?.limit && get().pastStates.length >= options?.limit) {
          get().pastStates.shift();
        }
        get()._onSave?.(pastState, currentState);
        set({
          pastStates: get().pastStates.concat(deltaState || pastState),
          futureStates: []
        });
      }
    };
  };
  return stateCreator;
};

// src/index.ts
var temporal = (config, options) => {
  const configWithTemporal = (set, get, store) => {
    store.temporal = createStore(
      options?.wrapTemporal?.(temporalStateCreator(set, get, options)) || temporalStateCreator(set, get, options)
    );
    const curriedHandleSet = options?.handleSet?.(
      store.temporal.getState()._handleSet
    ) || store.temporal.getState()._handleSet;
    const temporalHandleSet = (pastState) => {
      if (!store.temporal.getState().isTracking) return;
      const currentState = options?.partialize?.(get()) || get();
      const deltaState = options?.diff?.(pastState, currentState);
      if (
        // Don't call handleSet if state hasn't changed, as determined by diff fn or equality fn
        !// If the user has provided a diff function but nothing has been changed, deltaState will be null
        (deltaState === null || // If the user has provided an equality function, use it
        options?.equality?.(pastState, currentState))
      ) {
        curriedHandleSet(
          pastState,
          void 0,
          currentState,
          deltaState
        );
      }
    };
    const setState = store.setState;
    store.setState = (...args) => {
      const pastState = options?.partialize?.(get()) || get();
      setState(...args);
      temporalHandleSet(pastState);
    };
    return config(
      // Modify the set function to call the userlandSet function
      (...args) => {
        const pastState = options?.partialize?.(get()) || get();
        set(...args);
        temporalHandleSet(pastState);
      },
      get,
      store
    );
  };
  return configWithTemporal;
};
export {
  temporal
};
//# sourceMappingURL=index.js.map