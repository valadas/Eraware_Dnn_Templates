import tseslint from 'typescript-eslint';
import eslint from '@eslint/js';
import { defineConfig } from 'eslint/config';
import stencil from "@stencil/eslint-plugin";
import dnnElements from "@dnncommunity/dnn-elements/eslint-plugin";

export default defineConfig(
  eslint.configs.recommended,
  tseslint.configs.recommendedTypeChecked,
  stencil.configs.flat.recommended,
  dnnElements.configs.flat.recommended,
  {
    ignores: [
      'node_modules/*',
      'dist',
      'loader',
      'www',
      '*.js',
      'eslint.config.ts',
      'stencil.config.ts',
      'src/components.d.ts',
      'src/services/services.d.ts',
    ],
  },
  {
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  {
    rules: {
      "@typescript-eslint/no-unsafe-return": "off",
      "react/jsx-no-bind": "off",
    },
  },
);
