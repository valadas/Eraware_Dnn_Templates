import { Linter } from "eslint";
import typescriptEslintParser from "@typescript-eslint/parser";
import typescriptEslintPlugin from "@typescript-eslint/eslint-plugin";

/** @type {Linter.Config} */
const config = {
  languageOptions: {
    parser: typescriptEslintParser,
    parserOptions: {
      project: "./tsconfig.json",
    },
  },
  plugins: {
    typescript: typescriptEslintPlugin,
  },
  extends: [
    "plugin:@stencil-community/recommended",
  ],
  ingorePatterns: [
    "node_modules/",
    "dist/",
    "loader",
    "www",
    "src/services/services.d.ts",
  ],
  rules: {
    "no-console": "warn",
    "react/jsx-no-bind": "off",
    "tsdoc/syntax": "warn"
  },
};

export default config;
