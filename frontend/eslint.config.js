// eslint.config.js
import js from '@eslint/js'
import tseslint from 'typescript-eslint'

export default tseslint.config(
  { 
    ignores: ['dist', 'dist/**', 'node_modules', '*.config.js']   // ← Add this
  },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    files: ['**/*.{ts,tsx}'],
    rules: {
      // your custom rules here
      'no-console': 'warn',
      '@typescript-eslint/no-unused-vars': 'warn'
    }
  }
)