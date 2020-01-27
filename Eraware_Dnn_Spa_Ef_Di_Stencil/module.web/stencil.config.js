"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.config = {
    namespace: '$ext_scopeprefixkebab$',
    outputTargets: [
        {
            type: 'dist',
            esmLoaderPath: '../loader'
        },
        {
            type: 'docs-readme'
        },
        {
            type: 'www',
            serviceWorker: null // disable service workers
        }
    ],
    devServer: {
        reloadStrategy: "pageReload"
    }
};
//# sourceMappingURL=stencil.config.js.map