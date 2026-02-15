/**
 * PPTB API Type Augmentations
 * 
 * These type definitions extend the window object to include
 * PowerPlatformToolBox APIs: toolboxAPI and dataverseAPI.
 * 
 * Install @pptb/types package for full type definitions.
 */

import '@pptb/types';

declare global {
  interface Window {
    toolboxAPI: any; // Will be typed from @pptb/types
    dataverseAPI: any; // Will be typed from @pptb/types
  }
}

export {};
