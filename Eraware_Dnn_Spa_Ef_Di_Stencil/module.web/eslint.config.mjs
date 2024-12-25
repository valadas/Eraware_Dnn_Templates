import typescriptEslintEslintPlugin from "@typescript-eslint/eslint-plugin";
import tsdoc from "eslint-plugin-tsdoc";
import tsParser from "@typescript-eslint/parser";
import path from "node:path";
import { fileURLToPath } from "node:url";
import js from "@eslint/js";
import { FlatCompat } from "@eslint/eslintrc";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const compat = new FlatCompat({
  baseDirectory: __dirname,
  recommendedConfig: js.configs.recommended,
  allConfig: js.configs.all
});

export default [{
  ignores: ["**/dist", "**/loader", "**/www", "src/services/services.d.ts"],
}, ...compat.extends("plugin:@stencil-community/recommended"), {
  plugins: {
    "@typescript-eslint": typescriptEslintEslintPlugin,
    tsdoc,
  },

  languageOptions: {
    parser: tsParser,
    ecmaVersion: 5,
    sourceType: "script",

    parserOptions: {
      project: "./tsconfig.json",
    },
  },

  rules: {
    "no-console": "warn",
    "react/jsx-no-bind": "off",
    "tsdoc/syntax": "warn",
  },
}];
